using DiscreteMathToolkit.Visualization.Layout;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public enum TreeNodeBadge { None, Visited, Current }

public sealed class RenderableTreeNode
{
    public int Value { get; }
    public Point2D Position { get; }
    public TreeNodeBadge Badge { get; }
    public string? Annotation { get; }    // optional traversal index "#3"

    public RenderableTreeNode(int value, Point2D position, TreeNodeBadge badge, string? annotation)
    {
        Value = value;
        Position = position;
        Badge = badge;
        Annotation = annotation;
    }
}

public sealed class RenderableTreeEdge
{
    public Point2D From { get; }
    public Point2D To { get; }

    public RenderableTreeEdge(Point2D from, Point2D to) { From = from; To = to; }
}

public sealed class TreeRenderState
{
    public IReadOnlyList<RenderableTreeNode> Nodes { get; }
    public IReadOnlyList<RenderableTreeEdge> Edges { get; }
    public double Width { get; }
    public double Height { get; }

    public TreeRenderState(IReadOnlyList<RenderableTreeNode> nodes, IReadOnlyList<RenderableTreeEdge> edges, double width, double height)
    {
        Nodes = nodes;
        Edges = edges;
        Width = width;
        Height = height;
    }

    public static TreeRenderState Empty { get; } =
        new(Array.Empty<RenderableTreeNode>(), Array.Empty<RenderableTreeEdge>(), 0, 0);
}
