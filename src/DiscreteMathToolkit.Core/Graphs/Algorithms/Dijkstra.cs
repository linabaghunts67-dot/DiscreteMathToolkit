using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Core.Graphs.Algorithms;

public sealed class DijkstraState
{
    public IReadOnlyDictionary<int, double> Distance { get; }
    public IReadOnlyDictionary<int, int?> Previous { get; }
    public IReadOnlySet<int> Settled { get; }
    public int? Current { get; }
    public IReadOnlyList<int> Frontier { get; }

    public DijkstraState(
        IReadOnlyDictionary<int, double> distance,
        IReadOnlyDictionary<int, int?> previous,
        IReadOnlySet<int> settled,
        int? current,
        IReadOnlyList<int> frontier)
    {
        Distance = distance;
        Previous = previous;
        Settled = settled;
        Current = current;
        Frontier = frontier;
    }
}

public sealed class DijkstraResult
{
    public IReadOnlyDictionary<int, double> Distance { get; }
    public IReadOnlyDictionary<int, int?> Previous { get; }

    public DijkstraResult(IReadOnlyDictionary<int, double> distance, IReadOnlyDictionary<int, int?> previous)
    {
        Distance = distance;
        Previous = previous;
    }

    /// <summary>
    /// Reconstructs the shortest path from the start node (used in <see cref="Dijkstra"/>) to <paramref name="target"/>.
    /// Returns an empty list if no path exists.
    /// </summary>
    public IReadOnlyList<int> ReconstructPath(int target)
    {
        if (!Distance.TryGetValue(target, out var d) || double.IsPositiveInfinity(d))
            return Array.Empty<int>();

        var path = new List<int>();
        int? cur = target;
        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = Previous[cur.Value];
        }
        path.Reverse();
        return path;
    }
}

/// <summary>
/// Dijkstra's single-source shortest path algorithm. Requires non-negative edge weights.
/// Uses a SortedSet as the priority queue for clarity (O((V+E) log V)).
/// </summary>
public static class Dijkstra
{
    public static AlgorithmTrace<DijkstraState, DijkstraResult> Run(Graph graph, int start)
    {
        if (!graph.Nodes.ContainsKey(start))
            throw new ArgumentException($"Start node {start} is not in the graph.", nameof(start));

        foreach (var edge in graph.Edges())
        {
            if (edge.Weight < 0)
                throw new InvalidOperationException(
                    "Dijkstra cannot be applied to graphs with negative edge weights.");
        }

        var distance = graph.Nodes.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var previous = graph.Nodes.Keys.ToDictionary(id => id, _ => (int?)null);
        var settled = new HashSet<int>();
        distance[start] = 0;

        // (distance, id) tuples; SortedSet ordered by distance then id for determinism
        var pq = new SortedSet<(double Dist, int Id)>(Comparer<(double, int)>.Create(
            (a, b) => a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2)));
        pq.Add((0, start));

        var steps = new List<AlgorithmStep<DijkstraState>>();
        steps.Add(Snapshot(0, $"Initialize: distance[{graph.Nodes[start].Label}] = 0; all others = infinity.",
            distance, previous, settled, null, pq, new[] { start }));

        while (pq.Count > 0)
        {
            var (d, u) = pq.Min;
            pq.Remove(pq.Min);
            if (settled.Contains(u)) continue;

            settled.Add(u);
            steps.Add(Snapshot(steps.Count,
                $"Settle {graph.Nodes[u].Label} with distance {Format(d)}.",
                distance, previous, settled, u, pq, new[] { u }));

            foreach (var edge in graph.Neighbors(u).OrderBy(e => e.To))
            {
                int v = edge.To;
                if (settled.Contains(v)) continue;
                double alt = distance[u] + edge.Weight;
                if (alt < distance[v])
                {
                    pq.Remove((distance[v], v));
                    distance[v] = alt;
                    previous[v] = u;
                    pq.Add((alt, v));

                    steps.Add(Snapshot(steps.Count,
                        $"Relax edge {graph.Nodes[u].Label} -> {graph.Nodes[v].Label}: distance updated to {Format(alt)}.",
                        distance, previous, settled, u, pq,
                        new[] { u, v }, new[] { (u, v) }));
                }
                else
                {
                    steps.Add(Snapshot(steps.Count,
                        $"No improvement on {graph.Nodes[u].Label} -> {graph.Nodes[v].Label} ({Format(alt)} >= {Format(distance[v])}).",
                        distance, previous, settled, u, pq,
                        new[] { u, v }, new[] { (u, v) }));
                }
            }
        }

        steps.Add(Snapshot(steps.Count,
            "Dijkstra complete. All reachable nodes have shortest distances assigned.",
            distance, previous, settled, null, pq));

        return new AlgorithmTrace<DijkstraState, DijkstraResult>(
            steps,
            new DijkstraResult(distance, previous),
            "O((V + E) log V)",
            "O(V)");
    }

    private static AlgorithmStep<DijkstraState> Snapshot(
        int index, string desc,
        Dictionary<int, double> distance, Dictionary<int, int?> previous,
        HashSet<int> settled, int? current, SortedSet<(double, int)> pq,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        var frontier = pq.Select(t => t.Item2).ToList();
        var state = new DijkstraState(
            new Dictionary<int, double>(distance),
            new Dictionary<int, int?>(previous),
            new HashSet<int>(settled),
            current,
            frontier);
        return new AlgorithmStep<DijkstraState>(index, desc, state, highlightedNodes, highlightedEdges);
    }

    private static string Format(double d) =>
        double.IsPositiveInfinity(d) ? "inf" : d.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
}
