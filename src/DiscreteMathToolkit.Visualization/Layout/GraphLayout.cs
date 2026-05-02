using DiscreteMathToolkit.Core.Graphs;

namespace DiscreteMathToolkit.Visualization.Layout;

public readonly record struct Point2D(double X, double Y)
{
    public Point2D Offset(double dx, double dy) => new(X + dx, Y + dy);
    public double DistanceTo(Point2D other)
    {
        double dx = X - other.X, dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public sealed class GraphLayout
{
    public IReadOnlyDictionary<int, Point2D> NodePositions { get; }
    public double Width { get; }
    public double Height { get; }

    public GraphLayout(IReadOnlyDictionary<int, Point2D> positions, double width, double height)
    {
        NodePositions = positions;
        Width = width;
        Height = height;
    }
}

public interface IGraphLayoutEngine
{
    GraphLayout Compute(Graph graph, double width, double height);
}
