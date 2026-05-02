using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Core.Graphs.Algorithms;

/// <summary>Snapshot of BFS state at a point in time.</summary>
public sealed class BfsState
{
    public IReadOnlyList<int> Queue { get; }
    public IReadOnlySet<int> Visited { get; }
    public int? Current { get; }
    public IReadOnlyDictionary<int, int> Distance { get; }
    public IReadOnlyDictionary<int, int?> Parent { get; }

    public BfsState(
        IReadOnlyList<int> queue,
        IReadOnlySet<int> visited,
        int? current,
        IReadOnlyDictionary<int, int> distance,
        IReadOnlyDictionary<int, int?> parent)
    {
        Queue = queue;
        Visited = visited;
        Current = current;
        Distance = distance;
        Parent = parent;
    }
}

public sealed class BfsResult
{
    public IReadOnlyList<int> Order { get; }
    public IReadOnlyDictionary<int, int> Distance { get; }
    public IReadOnlyDictionary<int, int?> Parent { get; }

    public BfsResult(IReadOnlyList<int> order, IReadOnlyDictionary<int, int> distance, IReadOnlyDictionary<int, int?> parent)
    {
        Order = order;
        Distance = distance;
        Parent = parent;
    }
}

/// <summary>
/// Breadth-First Search. Computes visit order, shortest unweighted distance and parent pointers.
/// </summary>
public static class BreadthFirstSearch
{
    public static AlgorithmTrace<BfsState, BfsResult> Run(Graph graph, int start)
    {
        if (!graph.Nodes.ContainsKey(start))
            throw new ArgumentException($"Start node {start} is not in the graph.", nameof(start));

        var queue = new Queue<int>();
        var visited = new HashSet<int>();
        var distance = new Dictionary<int, int> { [start] = 0 };
        var parent = new Dictionary<int, int?> { [start] = null };
        var order = new List<int>();
        var steps = new List<AlgorithmStep<BfsState>>();

        queue.Enqueue(start);
        visited.Add(start);

        steps.Add(Snapshot(0, $"Initialize: enqueue start node {graph.Nodes[start].Label}; mark it visited.",
            queue, visited, null, distance, parent, new[] { start }));

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            order.Add(current);

            steps.Add(Snapshot(steps.Count,
                $"Dequeue {graph.Nodes[current].Label}. Examine its neighbors.",
                queue, visited, current, distance, parent, new[] { current }));

            foreach (var edge in graph.Neighbors(current).OrderBy(e => e.To))
            {
                int neighbor = edge.To;
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    distance[neighbor] = distance[current] + 1;
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);

                    steps.Add(Snapshot(steps.Count,
                        $"Visit {graph.Nodes[neighbor].Label} (distance {distance[neighbor]}); enqueue.",
                        queue, visited, current, distance, parent,
                        new[] { current, neighbor },
                        new[] { (current, neighbor) }));
                }
                else
                {
                    steps.Add(Snapshot(steps.Count,
                        $"Skip {graph.Nodes[neighbor].Label} - already visited.",
                        queue, visited, current, distance, parent,
                        new[] { current, neighbor }));
                }
            }
        }

        steps.Add(Snapshot(steps.Count,
            $"BFS complete. Visited {order.Count} node(s) reachable from {graph.Nodes[start].Label}.",
            queue, visited, null, distance, parent));

        var result = new BfsResult(order, distance, parent);
        return new AlgorithmTrace<BfsState, BfsResult>(steps, result, "O(V + E)", "O(V)");
    }

    private static AlgorithmStep<BfsState> Snapshot(
        int index, string desc,
        Queue<int> queue, HashSet<int> visited, int? current,
        Dictionary<int, int> distance, Dictionary<int, int?> parent,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        var state = new BfsState(
            queue.ToArray(),
            new HashSet<int>(visited),
            current,
            new Dictionary<int, int>(distance),
            new Dictionary<int, int?>(parent));
        return new AlgorithmStep<BfsState>(index, desc, state, highlightedNodes, highlightedEdges);
    }
}
