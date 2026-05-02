using System.Globalization;

namespace DiscreteMathToolkit.Core.Graphs;

/// <summary>
/// A node in a graph. Identity is by <see cref="Id"/>; <see cref="Label"/> is for display.
/// </summary>
public sealed class GraphNode
{
    public int Id { get; }
    public string Label { get; set; }

    public GraphNode(int id, string? label = null)
    {
        Id = id;
        Label = label ?? id.ToString(CultureInfo.InvariantCulture);
    }

    public override string ToString() => Label;
    public override bool Equals(object? obj) => obj is GraphNode n && n.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// A directed edge from <see cref="From"/> to <see cref="To"/> with a numeric weight.
/// In an undirected graph, two mirrored edges represent one logical edge.
/// </summary>
public sealed class GraphEdge
{
    public int From { get; }
    public int To { get; }
    public double Weight { get; }

    public GraphEdge(int from, int to, double weight = 1.0)
    {
        From = from;
        To = to;
        Weight = weight;
    }

    public override string ToString() =>
        $"{From} -> {To} (w={Weight.ToString("0.##", CultureInfo.InvariantCulture)})";
}
