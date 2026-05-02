using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Core.Graphs.Algorithms;

public sealed class PrimState
{
    public IReadOnlyList<GraphEdge> AcceptedEdges { get; }
    public IReadOnlySet<int> InTree { get; }
    public GraphEdge? CurrentEdge { get; }
    public double TotalWeight { get; }

    public PrimState(IReadOnlyList<GraphEdge> accepted, IReadOnlySet<int> inTree, GraphEdge? current, double totalWeight)
    {
        AcceptedEdges = accepted;
        InTree = inTree;
        CurrentEdge = current;
        TotalWeight = totalWeight;
    }
}

/// <summary>
/// Prim's algorithm. Grow an MST from a start node by always taking the cheapest edge
/// crossing the current cut.
/// </summary>
public static class Prim
{
    public static AlgorithmTrace<PrimState, MstResult> Run(Graph graph, int? startId = null)
    {
        if (graph.NodeCount == 0)
            throw new InvalidOperationException("Graph has no nodes.");

        int start = startId ?? graph.Nodes.Keys.Min();
        if (!graph.Nodes.ContainsKey(start))
            throw new ArgumentException($"Start node {start} is not in the graph.", nameof(startId));

        var inTree = new HashSet<int> { start };
        var accepted = new List<GraphEdge>();
        double total = 0;
        var steps = new List<AlgorithmStep<PrimState>>();

        steps.Add(Snapshot(0,
            $"Initialize tree with start node {graph.Nodes[start].Label}.",
            accepted, inTree, null, total, new[] { start }));

        while (inTree.Count < graph.NodeCount)
        {
            // collect all crossing edges
            GraphEdge? best = null;
            foreach (var u in inTree)
            {
                foreach (var edge in graph.Neighbors(u))
                {
                    if (!inTree.Contains(edge.To))
                    {
                        if (best is null ||
                            edge.Weight < best.Weight ||
                            (edge.Weight == best.Weight && (edge.From, edge.To).CompareTo((best.From, best.To)) < 0))
                        {
                            best = edge;
                        }
                    }
                }
            }

            if (best is null)
            {
                steps.Add(Snapshot(steps.Count,
                    "No edge crosses the cut. Graph is disconnected; stopping (minimum spanning forest of one component).",
                    accepted, inTree, null, total));
                break;
            }

            inTree.Add(best.To);
            accepted.Add(best);
            total += best.Weight;

            steps.Add(Snapshot(steps.Count,
                $"Add cheapest crossing edge {graph.Nodes[best.From].Label}-{graph.Nodes[best.To].Label} (weight {best.Weight:0.##}).",
                accepted, inTree, best, total,
                new[] { best.From, best.To }, new[] { (best.From, best.To) }));
        }

        bool isSpanning = inTree.Count == graph.NodeCount;
        steps.Add(Snapshot(steps.Count,
            isSpanning
                ? $"Prim complete. MST weight: {total:0.##}."
                : $"Stopped: only {inTree.Count}/{graph.NodeCount} nodes reached. Graph is disconnected.",
            accepted, inTree, null, total));

        return new AlgorithmTrace<PrimState, MstResult>(
            steps,
            new MstResult(accepted, total, isSpanning),
            "O(E log V)",
            "O(V)");
    }

    private static AlgorithmStep<PrimState> Snapshot(
        int index, string desc,
        List<GraphEdge> accepted, HashSet<int> inTree, GraphEdge? current, double total,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        var state = new PrimState(
            new List<GraphEdge>(accepted),
            new HashSet<int>(inTree),
            current,
            total);
        return new AlgorithmStep<PrimState>(index, desc, state, highlightedNodes, highlightedEdges);
    }
}
