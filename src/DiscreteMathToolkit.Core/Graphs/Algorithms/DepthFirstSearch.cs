using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Core.Graphs.Algorithms;

public sealed class DfsState
{
    public IReadOnlyList<int> Stack { get; }
    public IReadOnlySet<int> Visited { get; }
    public int? Current { get; }
    public IReadOnlyDictionary<int, int?> Parent { get; }
    public IReadOnlyList<int> VisitOrder { get; }

    public DfsState(
        IReadOnlyList<int> stack,
        IReadOnlySet<int> visited,
        int? current,
        IReadOnlyDictionary<int, int?> parent,
        IReadOnlyList<int> visitOrder)
    {
        Stack = stack;
        Visited = visited;
        Current = current;
        Parent = parent;
        VisitOrder = visitOrder;
    }
}

public sealed class DfsResult
{
    public IReadOnlyList<int> Order { get; }
    public IReadOnlyDictionary<int, int?> Parent { get; }

    public DfsResult(IReadOnlyList<int> order, IReadOnlyDictionary<int, int?> parent)
    {
        Order = order;
        Parent = parent;
    }
}

/// <summary>
/// Depth-First Search using an explicit stack. The iterative form makes step recording clean
/// and avoids stack overflows on large graphs.
/// </summary>
public static class DepthFirstSearch
{
    public static AlgorithmTrace<DfsState, DfsResult> Run(Graph graph, int start)
    {
        if (!graph.Nodes.ContainsKey(start))
            throw new ArgumentException($"Start node {start} is not in the graph.", nameof(start));

        var stack = new Stack<int>();
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int?> { [start] = null };
        var order = new List<int>();
        var steps = new List<AlgorithmStep<DfsState>>();

        stack.Push(start);

        steps.Add(Snapshot(0, $"Initialize: push start node {graph.Nodes[start].Label} onto the stack.",
            stack, visited, null, parent, order, new[] { start }));

        while (stack.Count > 0)
        {
            int current = stack.Pop();
            if (visited.Contains(current))
            {
                steps.Add(Snapshot(steps.Count,
                    $"Pop {graph.Nodes[current].Label} - already visited, skip.",
                    stack, visited, current, parent, order, new[] { current }));
                continue;
            }

            visited.Add(current);
            order.Add(current);

            steps.Add(Snapshot(steps.Count,
                $"Visit {graph.Nodes[current].Label}. Push unvisited neighbors.",
                stack, visited, current, parent, order, new[] { current }));

            // push in reverse sorted order so smallest id is processed first (matches recursive DFS)
            foreach (var edge in graph.Neighbors(current).OrderByDescending(e => e.To))
            {
                int neighbor = edge.To;
                if (!visited.Contains(neighbor))
                {
                    if (!parent.ContainsKey(neighbor))
                        parent[neighbor] = current;
                    stack.Push(neighbor);

                    steps.Add(Snapshot(steps.Count,
                        $"Push {graph.Nodes[neighbor].Label} (neighbor of {graph.Nodes[current].Label}).",
                        stack, visited, current, parent, order,
                        new[] { current, neighbor },
                        new[] { (current, neighbor) }));
                }
            }
        }

        steps.Add(Snapshot(steps.Count,
            $"DFS complete. Visit order: {string.Join(", ", order.Select(id => graph.Nodes[id].Label))}.",
            stack, visited, null, parent, order));

        var result = new DfsResult(order, parent);
        return new AlgorithmTrace<DfsState, DfsResult>(steps, result, "O(V + E)", "O(V)");
    }

    private static AlgorithmStep<DfsState> Snapshot(
        int index, string desc,
        Stack<int> stack, HashSet<int> visited, int? current,
        Dictionary<int, int?> parent, List<int> order,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        var state = new DfsState(
            stack.ToArray(),
            new HashSet<int>(visited),
            current,
            new Dictionary<int, int?>(parent),
            new List<int>(order));
        return new AlgorithmStep<DfsState>(index, desc, state, highlightedNodes, highlightedEdges);
    }
}
