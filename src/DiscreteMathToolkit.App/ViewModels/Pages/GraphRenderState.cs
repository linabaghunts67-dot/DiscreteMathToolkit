using DiscreteMathToolkit.Visualization.Layout;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public enum NodeBadge { None, Visited, Frontier, Settled, Current, InTree }
public enum EdgeBadge { None, Active, Accepted, Rejected, Tree }

public sealed class RenderableNode
{
    public int Id { get; }
    public string Label { get; }
    public Point2D Position { get; }
    public NodeBadge Badge { get; }
    public string? Annotation { get; }   // e.g. distance "0", "∞", parent id...

    public RenderableNode(int id, string label, Point2D pos, NodeBadge badge, string? annotation)
    {
        Id = id;
        Label = label;
        Position = pos;
        Badge = badge;
        Annotation = annotation;
    }
}

public sealed class RenderableEdge
{
    public int From { get; }
    public int To { get; }
    public string? Label { get; }
    public EdgeBadge Badge { get; }
    public bool IsDirected { get; }

    public RenderableEdge(int from, int to, string? label, EdgeBadge badge, bool isDirected)
    {
        From = from;
        To = to;
        Label = label;
        Badge = badge;
        IsDirected = isDirected;
    }
}

public sealed class GraphRenderState
{
    public IReadOnlyList<RenderableNode> Nodes { get; }
    public IReadOnlyList<RenderableEdge> Edges { get; }

    public GraphRenderState(IReadOnlyList<RenderableNode> nodes, IReadOnlyList<RenderableEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }

    public static GraphRenderState Empty { get; } =
        new(Array.Empty<RenderableNode>(), Array.Empty<RenderableEdge>());
}
