using System.Text.Json;
using System.Text.Json.Serialization;
using DiscreteMathToolkit.Core.Graphs;

namespace DiscreteMathToolkit.Infrastructure.Persistence;

/// <summary>JSON-friendly DTOs to keep the Core models free of serialization concerns.</summary>
public sealed class GraphDocument
{
    public string SchemaVersion { get; set; } = "1.0";
    public bool Directed { get; set; }
    public bool Weighted { get; set; }
    public List<NodeDto> Nodes { get; set; } = new();
    public List<EdgeDto> Edges { get; set; } = new();
}

public sealed class NodeDto
{
    public int Id { get; set; }
    public string? Label { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

public sealed class EdgeDto
{
    public int From { get; set; }
    public int To { get; set; }
    public double Weight { get; set; } = 1.0;
}

public interface IGraphRepository
{
    GraphDocument Load(string path);
    void Save(string path, GraphDocument document);
    GraphDocument FromGraph(Graph graph, IReadOnlyDictionary<int, (double X, double Y)>? positions = null);
    Graph ToGraph(GraphDocument document);
}

public sealed class JsonGraphRepository : IGraphRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GraphDocument Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Graph file not found.", path);
        var text = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<GraphDocument>(text, Options)
                  ?? throw new InvalidDataException("Empty or invalid graph file.");
        return doc;
    }

    public void Save(string path, GraphDocument document)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(document, Options));
    }

    public GraphDocument FromGraph(Graph graph, IReadOnlyDictionary<int, (double X, double Y)>? positions = null)
    {
        var doc = new GraphDocument
        {
            Directed = graph.IsDirected,
            Weighted = graph.IsWeighted
        };
        foreach (var node in graph.Nodes.Values.OrderBy(n => n.Id))
        {
            var (x, y) = positions != null && positions.TryGetValue(node.Id, out var p) ? p : (0.0, 0.0);
            doc.Nodes.Add(new NodeDto { Id = node.Id, Label = node.Label, X = x, Y = y });
        }
        foreach (var edge in graph.Edges())
            doc.Edges.Add(new EdgeDto { From = edge.From, To = edge.To, Weight = edge.Weight });
        return doc;
    }

    public Graph ToGraph(GraphDocument document)
    {
        var graph = new Graph(document.Directed, document.Weighted);
        foreach (var n in document.Nodes) graph.AddNode(n.Id, n.Label);
        foreach (var e in document.Edges) graph.AddEdge(e.From, e.To, e.Weight);
        return graph;
    }
}
