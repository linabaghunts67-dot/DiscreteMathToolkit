using DiscreteMathToolkit.Core.Graphs;

namespace DiscreteMathToolkit.Visualization.Layout;

/// <summary>
/// Places all nodes on a circle. Deterministic and ideal for small graphs (≤ 15 nodes)
/// or as a starting point for force-directed layout.
/// </summary>
public sealed class CircularLayoutEngine : IGraphLayoutEngine
{
    public double Padding { get; init; } = 60;

    public GraphLayout Compute(Graph graph, double width, double height)
    {
        var positions = new Dictionary<int, Point2D>();
        var ids = graph.Nodes.Keys.OrderBy(x => x).ToArray();
        if (ids.Length == 0) return new GraphLayout(positions, width, height);

        double cx = width / 2.0;
        double cy = height / 2.0;
        double r = Math.Min(width, height) / 2.0 - Padding;
        if (r < 20) r = 20;

        if (ids.Length == 1)
        {
            positions[ids[0]] = new Point2D(cx, cy);
            return new GraphLayout(positions, width, height);
        }

        for (int i = 0; i < ids.Length; i++)
        {
            // start at top (angle = -π/2) and go clockwise
            double angle = -Math.PI / 2.0 + 2.0 * Math.PI * i / ids.Length;
            positions[ids[i]] = new Point2D(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
        }
        return new GraphLayout(positions, width, height);
    }
}
