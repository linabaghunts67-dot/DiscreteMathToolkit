using DiscreteMathToolkit.Core.Algorithms;
using DiscreteMathToolkit.Core.Common;

namespace DiscreteMathToolkit.Core.Graphs.Algorithms;

public sealed class KruskalState
{
    public IReadOnlyList<GraphEdge> AcceptedEdges { get; }
    public IReadOnlyList<GraphEdge> RejectedEdges { get; }
    public GraphEdge? CurrentEdge { get; }
    public IReadOnlyList<IReadOnlyList<int>> Components { get; }
    public double TotalWeight { get; }

    public KruskalState(
        IReadOnlyList<GraphEdge> accepted,
        IReadOnlyList<GraphEdge> rejected,
        GraphEdge? current,
        IReadOnlyList<IReadOnlyList<int>> components,
        double totalWeight)
    {
        AcceptedEdges = accepted;
        RejectedEdges = rejected;
        CurrentEdge = current;
        Components = components;
        TotalWeight = totalWeight;
    }
}

public sealed class MstResult
{
    public IReadOnlyList<GraphEdge> Edges { get; }
    public double TotalWeight { get; }
    public bool IsSpanning { get; }

    public MstResult(IReadOnlyList<GraphEdge> edges, double totalWeight, bool isSpanning)
    {
        Edges = edges;
        TotalWeight = totalWeight;
        IsSpanning = isSpanning;
    }
}

/// <summary>
/// Kruskal's algorithm. Sort edges by weight, accept the next one whenever it does not form a cycle.
/// Works on undirected graphs; for directed input we treat edges as undirected.
/// </summary>
public static class Kruskal
{
    public static AlgorithmTrace<KruskalState, MstResult> Run(Graph graph)
    {
        if (graph.NodeCount == 0)
            throw new InvalidOperationException("Graph has no nodes.");

        var dsu = new DisjointSetUnion();
        foreach (var id in graph.Nodes.Keys) dsu.MakeSet(id);

        var sortedEdges = graph.Edges()
            .OrderBy(e => e.Weight)
            .ThenBy(e => e.From)
            .ThenBy(e => e.To)
            .ToList();

        var accepted = new List<GraphEdge>();
        var rejected = new List<GraphEdge>();
        double total = 0;
        var steps = new List<AlgorithmStep<KruskalState>>();

        steps.Add(Snapshot(0,
            $"Initialize: {graph.NodeCount} singleton components; sort {sortedEdges.Count} edges by weight.",
            accepted, rejected, null, dsu, graph.Nodes.Keys, total));

        foreach (var edge in sortedEdges)
        {
            if (!dsu.Connected(edge.From, edge.To))
            {
                dsu.Union(edge.From, edge.To);
                accepted.Add(edge);
                total += edge.Weight;
                steps.Add(Snapshot(steps.Count,
                    $"Accept edge {graph.Nodes[edge.From].Label}-{graph.Nodes[edge.To].Label} (weight {edge.Weight:0.##}); merge components.",
                    accepted, rejected, edge, dsu, graph.Nodes.Keys, total,
                    new[] { edge.From, edge.To }, new[] { (edge.From, edge.To) }));

                if (accepted.Count == graph.NodeCount - 1) break;
            }
            else
            {
                rejected.Add(edge);
                steps.Add(Snapshot(steps.Count,
                    $"Reject edge {graph.Nodes[edge.From].Label}-{graph.Nodes[edge.To].Label} (would create a cycle).",
                    accepted, rejected, edge, dsu, graph.Nodes.Keys, total,
                    new[] { edge.From, edge.To }, new[] { (edge.From, edge.To) }));
            }
        }

        bool isSpanning = accepted.Count == graph.NodeCount - 1;
        steps.Add(Snapshot(steps.Count,
            isSpanning
                ? $"MST complete. Total weight: {total:0.##}."
                : $"Graph is not connected. Found a minimum spanning forest with {accepted.Count} edge(s); total {total:0.##}.",
            accepted, rejected, null, dsu, graph.Nodes.Keys, total));

        return new AlgorithmTrace<KruskalState, MstResult>(
            steps,
            new MstResult(accepted, total, isSpanning),
            "O(E log E)",
            "O(V + E)");
    }

    private static AlgorithmStep<KruskalState> Snapshot(
        int index, string desc,
        List<GraphEdge> accepted, List<GraphEdge> rejected,
        GraphEdge? current, DisjointSetUnion dsu, IEnumerable<int> nodeIds, double total,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        var groups = nodeIds.GroupBy(dsu.Find)
                            .Select(g => (IReadOnlyList<int>)g.OrderBy(x => x).ToList())
                            .ToList();
        var state = new KruskalState(
            new List<GraphEdge>(accepted),
            new List<GraphEdge>(rejected),
            current,
            groups,
            total);
        return new AlgorithmStep<KruskalState>(index, desc, state, highlightedNodes, highlightedEdges);
    }
}
