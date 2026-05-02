using DiscreteMathToolkit.Core.Graphs;

namespace DiscreteMathToolkit.Visualization.Layout;

/// <summary>
/// Force-directed layout using Fruchterman–Reingold (1991).
/// Repulsive forces between every pair of nodes; attractive forces along edges.
/// Deterministic when seeded; converges in O(iterations · V²).
/// </summary>
public sealed class ForceDirectedLayoutEngine : IGraphLayoutEngine
{
    public int Iterations { get; init; } = 200;
    public double Padding { get; init; } = 50;
    public int Seed { get; init; } = 1729;

    public GraphLayout Compute(Graph graph, double width, double height)
    {
        var ids = graph.Nodes.Keys.ToArray();
        var positions = new Dictionary<int, (double X, double Y)>();
        if (ids.Length == 0) return new GraphLayout(new Dictionary<int, Point2D>(), width, height);
        if (ids.Length == 1)
        {
            positions[ids[0]] = (width / 2, height / 2);
            return Build(positions, width, height);
        }

        // 1. Initialize: place on a circle (much better starting condition than random for small graphs)
        var rand = new Random(Seed);
        double cx = width / 2.0, cy = height / 2.0;
        double startR = Math.Min(width, height) / 3.0;
        for (int i = 0; i < ids.Length; i++)
        {
            double angle = 2.0 * Math.PI * i / ids.Length + rand.NextDouble() * 0.1;
            positions[ids[i]] = (cx + startR * Math.Cos(angle), cy + startR * Math.Sin(angle));
        }

        double area = (width - 2 * Padding) * (height - 2 * Padding);
        double k = Math.Sqrt(area / ids.Length);            // ideal edge length
        double t = (width + height) / 10.0;                 // initial "temperature" — max move per iter

        var disp = new Dictionary<int, (double X, double Y)>();

        for (int iter = 0; iter < Iterations; iter++)
        {
            foreach (var id in ids) disp[id] = (0, 0);

            // repulsive forces between all pairs
            for (int i = 0; i < ids.Length; i++)
            {
                for (int j = i + 1; j < ids.Length; j++)
                {
                    var (px, py) = positions[ids[i]];
                    var (qx, qy) = positions[ids[j]];
                    double dx = px - qx, dy = py - qy;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    if (d < 0.01) { d = 0.01; dx = (rand.NextDouble() - 0.5) * 0.1; dy = (rand.NextDouble() - 0.5) * 0.1; }
                    double force = (k * k) / d;
                    var (vx, vy) = disp[ids[i]];
                    disp[ids[i]] = (vx + dx / d * force, vy + dy / d * force);
                    var (wx, wy) = disp[ids[j]];
                    disp[ids[j]] = (wx - dx / d * force, wy - dy / d * force);
                }
            }

            // attractive forces along edges
            foreach (var edge in graph.Edges())
            {
                var (px, py) = positions[edge.From];
                var (qx, qy) = positions[edge.To];
                double dx = px - qx, dy = py - qy;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d < 0.01) { d = 0.01; }
                double force = (d * d) / k;
                var (vx, vy) = disp[edge.From];
                disp[edge.From] = (vx - dx / d * force, vy - dy / d * force);
                var (wx, wy) = disp[edge.To];
                disp[edge.To] = (wx + dx / d * force, wy + dy / d * force);
            }

            // limit movement by temperature & keep inside frame
            foreach (var id in ids)
            {
                var (vx, vy) = disp[id];
                double mag = Math.Sqrt(vx * vx + vy * vy);
                if (mag < 0.001) continue;
                double scale = Math.Min(mag, t) / mag;
                var (px, py) = positions[id];
                double nx = px + vx * scale;
                double ny = py + vy * scale;
                nx = Math.Clamp(nx, Padding, width - Padding);
                ny = Math.Clamp(ny, Padding, height - Padding);
                positions[id] = (nx, ny);
            }

            // cool down (linear cooling schedule)
            t *= 1.0 - (1.0 / Iterations);
        }

        return Build(positions, width, height);
    }

    private static GraphLayout Build(Dictionary<int, (double X, double Y)> raw, double w, double h)
    {
        var dict = new Dictionary<int, Point2D>(raw.Count);
        foreach (var (id, (x, y)) in raw) dict[id] = new Point2D(x, y);
        return new GraphLayout(dict, w, h);
    }
}
