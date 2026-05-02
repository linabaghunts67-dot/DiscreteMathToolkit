using System.Globalization;
using System.Text;

namespace DiscreteMathToolkit.Core.Graphs;

/// <summary>
/// A finite graph with integer node ids. Supports directed and undirected modes
/// and per-edge weights. Thread-safety is the caller's responsibility.
/// </summary>
public sealed class Graph
{
    private readonly Dictionary<int, GraphNode> _nodes = new();
    private readonly Dictionary<int, List<GraphEdge>> _adjacency = new();

    /// <summary>True for directed graphs; false for undirected graphs.</summary>
    public bool IsDirected { get; }

    /// <summary>True for weighted graphs (used as a hint for the UI; weights are always stored).</summary>
    public bool IsWeighted { get; }

    public IReadOnlyDictionary<int, GraphNode> Nodes => _nodes;

    public Graph(bool directed = false, bool weighted = false)
    {
        IsDirected = directed;
        IsWeighted = weighted;
    }

    public GraphNode AddNode(int id, string? label = null)
    {
        if (_nodes.ContainsKey(id))
            throw new InvalidOperationException($"Node with id {id} already exists.");
        var node = new GraphNode(id, label);
        _nodes[id] = node;
        _adjacency[id] = new List<GraphEdge>();
        return node;
    }

    public bool RemoveNode(int id)
    {
        if (!_nodes.Remove(id)) return false;
        _adjacency.Remove(id);
        // remove dangling edges that pointed to this node
        foreach (var list in _adjacency.Values)
            list.RemoveAll(e => e.To == id);
        return true;
    }

    public void AddEdge(int from, int to, double weight = 1.0)
    {
        if (!_nodes.ContainsKey(from))
            throw new InvalidOperationException($"Node {from} does not exist.");
        if (!_nodes.ContainsKey(to))
            throw new InvalidOperationException($"Node {to} does not exist.");
        if (double.IsNaN(weight) || double.IsInfinity(weight))
            throw new ArgumentException("Edge weight must be a finite number.", nameof(weight));

        _adjacency[from].Add(new GraphEdge(from, to, weight));
        if (!IsDirected && from != to)
            _adjacency[to].Add(new GraphEdge(to, from, weight));
    }

    public bool RemoveEdge(int from, int to)
    {
        if (!_adjacency.TryGetValue(from, out var fromList)) return false;
        var removed = fromList.RemoveAll(e => e.To == to) > 0;
        if (!IsDirected && _adjacency.TryGetValue(to, out var toList))
            toList.RemoveAll(e => e.To == from);
        return removed;
    }

    /// <summary>Outgoing edges from <paramref name="nodeId"/> (empty if node is unknown).</summary>
    public IReadOnlyList<GraphEdge> Neighbors(int nodeId) =>
        _adjacency.TryGetValue(nodeId, out var list)
            ? list
            : Array.Empty<GraphEdge>();

    /// <summary>All edges. For undirected graphs each logical edge appears once (canonical orientation).</summary>
    public IEnumerable<GraphEdge> Edges()
    {
        foreach (var (from, list) in _adjacency)
        {
            foreach (var edge in list)
            {
                if (IsDirected || from <= edge.To)
                    yield return edge;
            }
        }
    }

    public int NodeCount => _nodes.Count;

    public int EdgeCount => Edges().Count();

    /// <summary>Adjacency matrix in stable id order. Cell value: weight, or <see cref="double.PositiveInfinity"/> if no edge.</summary>
    public double[,] ToAdjacencyMatrix(out int[] orderedIds)
    {
        orderedIds = _nodes.Keys.OrderBy(x => x).ToArray();
        int n = orderedIds.Length;
        var index = new Dictionary<int, int>(n);
        for (int i = 0; i < n; i++) index[orderedIds[i]] = i;

        var m = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                m[i, j] = i == j ? 0.0 : double.PositiveInfinity;

        foreach (var (from, list) in _adjacency)
        {
            int fi = index[from];
            foreach (var edge in list)
            {
                int ti = index[edge.To];
                // keep the smallest weight if duplicates exist
                if (edge.Weight < m[fi, ti]) m[fi, ti] = edge.Weight;
            }
        }
        return m;
    }

    /// <summary>Human-readable adjacency-list view, sorted by source id.</summary>
    public string ToAdjacencyListString()
    {
        var sb = new StringBuilder();
        foreach (var id in _nodes.Keys.OrderBy(x => x))
        {
            sb.Append(_nodes[id].Label).Append(": ");
            var neighbors = _adjacency[id]
                .OrderBy(e => e.To)
                .Select(e => IsWeighted
                    ? $"{_nodes[e.To].Label}({e.Weight.ToString("0.##", CultureInfo.InvariantCulture)})"
                    : _nodes[e.To].Label);
            sb.AppendLine(string.Join(", ", neighbors));
        }
        return sb.ToString();
    }

    public Graph Clone()
    {
        var copy = new Graph(IsDirected, IsWeighted);
        foreach (var node in _nodes.Values)
            copy.AddNode(node.Id, node.Label);
        foreach (var (from, list) in _adjacency)
            foreach (var edge in list)
                if (IsDirected || from <= edge.To)
                    copy.AddEdge(edge.From, edge.To, edge.Weight);
        return copy;
    }
}
