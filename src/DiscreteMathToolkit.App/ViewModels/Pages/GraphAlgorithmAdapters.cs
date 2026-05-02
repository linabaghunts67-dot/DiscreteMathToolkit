using DiscreteMathToolkit.Core.Algorithms;
using DiscreteMathToolkit.Core.Graphs;
using DiscreteMathToolkit.Core.Graphs.Algorithms;
using DiscreteMathToolkit.Infrastructure.Export;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

internal abstract class GraphAlgorithmAdapter<TState, TResult> : GraphTheoryViewModel.IPlaybackAdapter
{
    protected readonly GraphTheoryViewModel _owner;
    protected readonly AlgorithmTrace<TState, TResult> _trace;
    private int _index;

    protected GraphAlgorithmAdapter(GraphTheoryViewModel owner, AlgorithmTrace<TState, TResult> trace)
    {
        _owner = owner;
        _trace = trace;
    }

    public int TotalSteps => _trace.Steps.Count;
    public int Index => _index;
    public bool AtStart => _index == 0;
    public bool AtEnd => _index >= _trace.Steps.Count - 1;

    public void Reset()
    {
        _index = 0;
        Apply(_trace.Steps[_index]);
    }

    public bool Next()
    {
        if (AtEnd) return false;
        _index++;
        Apply(_trace.Steps[_index]);
        return true;
    }

    public bool Previous()
    {
        if (AtStart) return false;
        _index--;
        Apply(_trace.Steps[_index]);
        return true;
    }

    private void Apply(AlgorithmStep<TState> step)
    {
        var nodeBadges = new Dictionary<int, NodeBadge>();
        var nodeAnn = new Dictionary<int, string?>();
        var edgeBadges = new Dictionary<(int, int), EdgeBadge>();
        DecorateState(step, nodeBadges, nodeAnn, edgeBadges);

        // overlay highlights from the step itself
        foreach (var nid in step.HighlightedNodes)
        {
            if (!nodeBadges.ContainsKey(nid)) nodeBadges[nid] = NodeBadge.Current;
        }
        foreach (var (a, b) in step.HighlightedEdges)
        {
            if (!edgeBadges.ContainsKey((a, b))) edgeBadges[(a, b)] = EdgeBadge.Active;
        }

        _owner.ApplyStepRender(step.Index, step.Description, nodeBadges, nodeAnn, edgeBadges);
    }

    protected abstract void DecorateState(AlgorithmStep<TState> step,
        IDictionary<int, NodeBadge> nodeBadges,
        IDictionary<int, string?> nodeAnnotations,
        IDictionary<(int, int), EdgeBadge> edgeBadges);

    public abstract TabularData BuildExport();
}

// ============================================================
// BFS
// ============================================================
internal sealed class BfsAdapter : GraphAlgorithmAdapter<BfsState, BfsResult>
{
    public BfsAdapter(GraphTheoryViewModel owner, AlgorithmTrace<BfsState, BfsResult> trace) : base(owner, trace) { }

    protected override void DecorateState(AlgorithmStep<BfsState> step,
        IDictionary<int, NodeBadge> nb,
        IDictionary<int, string?> na,
        IDictionary<(int, int), EdgeBadge> eb)
    {
        var s = step.State;
        foreach (var v in s.Visited) nb[v] = NodeBadge.Visited;
        foreach (var f in s.Queue) nb[f] = NodeBadge.Frontier;
        if (s.Current.HasValue) nb[s.Current.Value] = NodeBadge.Current;
        foreach (var (id, d) in s.Distance) na[id] = $"d={d}";
    }

    public override TabularData BuildExport()
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var (id, dist) in _trace.Result.Distance.OrderBy(kv => kv.Key))
        {
            string parent = _trace.Result.Parent[id]?.ToString() ?? "—";
            rows.Add(new[] { id.ToString(), dist.ToString(), parent });
        }
        return new TabularData("BFS Result",
            new[] { "Node", "Distance", "Parent" }, rows,
            new[] { $"Time complexity: {_trace.TimeComplexity}, Space: {_trace.SpaceComplexity}" });
    }
}

// ============================================================
// DFS
// ============================================================
internal sealed class DfsAdapter : GraphAlgorithmAdapter<DfsState, DfsResult>
{
    public DfsAdapter(GraphTheoryViewModel owner, AlgorithmTrace<DfsState, DfsResult> trace) : base(owner, trace) { }

    protected override void DecorateState(AlgorithmStep<DfsState> step,
        IDictionary<int, NodeBadge> nb,
        IDictionary<int, string?> na,
        IDictionary<(int, int), EdgeBadge> eb)
    {
        var s = step.State;
        foreach (var v in s.Visited) nb[v] = NodeBadge.Visited;
        foreach (var f in s.Stack) nb[f] = NodeBadge.Frontier;
        if (s.Current.HasValue) nb[s.Current.Value] = NodeBadge.Current;
        for (int i = 0; i < s.VisitOrder.Count; i++) na[s.VisitOrder[i]] = $"#{i + 1}";
    }

    public override TabularData BuildExport()
    {
        var rows = new List<IReadOnlyList<string>>();
        for (int i = 0; i < _trace.Result.Order.Count; i++)
        {
            int id = _trace.Result.Order[i];
            string parent = _trace.Result.Parent[id]?.ToString() ?? "—";
            rows.Add(new[] { (i + 1).ToString(), id.ToString(), parent });
        }
        return new TabularData("DFS Result",
            new[] { "Order", "Node", "Parent" }, rows,
            new[] { $"Time complexity: {_trace.TimeComplexity}, Space: {_trace.SpaceComplexity}" });
    }
}

// ============================================================
// Dijkstra
// ============================================================
internal sealed class DijkstraAdapter : GraphAlgorithmAdapter<DijkstraState, DijkstraResult>
{
    public DijkstraAdapter(GraphTheoryViewModel owner, AlgorithmTrace<DijkstraState, DijkstraResult> trace) : base(owner, trace) { }

    protected override void DecorateState(AlgorithmStep<DijkstraState> step,
        IDictionary<int, NodeBadge> nb,
        IDictionary<int, string?> na,
        IDictionary<(int, int), EdgeBadge> eb)
    {
        var s = step.State;
        foreach (var v in s.Settled) nb[v] = NodeBadge.Settled;
        foreach (var f in s.Frontier) nb[f] = NodeBadge.Frontier;
        if (s.Current.HasValue) nb[s.Current.Value] = NodeBadge.Current;
        foreach (var (id, d) in s.Distance)
            na[id] = double.IsPositiveInfinity(d) ? "∞" : d.ToString("0.##");
    }

    public override TabularData BuildExport()
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var (id, d) in _trace.Result.Distance.OrderBy(kv => kv.Key))
        {
            string dist = double.IsPositiveInfinity(d) ? "∞" : d.ToString("0.##");
            string prev = _trace.Result.Previous[id]?.ToString() ?? "—";
            var path = _trace.Result.ReconstructPath(id);
            rows.Add(new[] { id.ToString(), dist, prev, string.Join(" → ", path) });
        }
        return new TabularData("Dijkstra Shortest Paths",
            new[] { "Node", "Distance", "Previous", "Path" }, rows,
            new[] { $"Time complexity: {_trace.TimeComplexity}, Space: {_trace.SpaceComplexity}" });
    }
}

// ============================================================
// Kruskal
// ============================================================
internal sealed class KruskalAdapter : GraphAlgorithmAdapter<KruskalState, MstResult>
{
    public KruskalAdapter(GraphTheoryViewModel owner, AlgorithmTrace<KruskalState, MstResult> trace) : base(owner, trace) { }

    protected override void DecorateState(AlgorithmStep<KruskalState> step,
        IDictionary<int, NodeBadge> nb,
        IDictionary<int, string?> na,
        IDictionary<(int, int), EdgeBadge> eb)
    {
        var s = step.State;
        foreach (var e in s.AcceptedEdges)
        {
            eb[(e.From, e.To)] = EdgeBadge.Tree;
            nb[e.From] = NodeBadge.InTree;
            nb[e.To] = NodeBadge.InTree;
        }
        foreach (var e in s.RejectedEdges) eb[(e.From, e.To)] = EdgeBadge.Rejected;
        if (s.CurrentEdge != null) eb[(s.CurrentEdge.From, s.CurrentEdge.To)] = EdgeBadge.Active;
    }

    public override TabularData BuildExport()
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var e in _trace.Result.Edges)
            rows.Add(new[] { e.From.ToString(), e.To.ToString(), e.Weight.ToString("0.##") });
        return new TabularData("Kruskal MST",
            new[] { "From", "To", "Weight" }, rows,
            new[]
            {
                $"Total weight: {_trace.Result.TotalWeight:0.##}",
                $"Spanning: {(_trace.Result.IsSpanning ? "yes" : "no (graph disconnected)")}",
                $"Time complexity: {_trace.TimeComplexity}, Space: {_trace.SpaceComplexity}"
            });
    }
}

// ============================================================
// Prim
// ============================================================
internal sealed class PrimAdapter : GraphAlgorithmAdapter<PrimState, MstResult>
{
    public PrimAdapter(GraphTheoryViewModel owner, AlgorithmTrace<PrimState, MstResult> trace) : base(owner, trace) { }

    protected override void DecorateState(AlgorithmStep<PrimState> step,
        IDictionary<int, NodeBadge> nb,
        IDictionary<int, string?> na,
        IDictionary<(int, int), EdgeBadge> eb)
    {
        var s = step.State;
        foreach (var v in s.InTree) nb[v] = NodeBadge.InTree;
        foreach (var e in s.AcceptedEdges) eb[(e.From, e.To)] = EdgeBadge.Tree;
        if (s.CurrentEdge != null) eb[(s.CurrentEdge.From, s.CurrentEdge.To)] = EdgeBadge.Active;
    }

    public override TabularData BuildExport()
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var e in _trace.Result.Edges)
            rows.Add(new[] { e.From.ToString(), e.To.ToString(), e.Weight.ToString("0.##") });
        return new TabularData("Prim MST",
            new[] { "From", "To", "Weight" }, rows,
            new[]
            {
                $"Total weight: {_trace.Result.TotalWeight:0.##}",
                $"Spanning: {(_trace.Result.IsSpanning ? "yes" : "no (graph disconnected)")}",
                $"Time complexity: {_trace.TimeComplexity}, Space: {_trace.SpaceComplexity}"
            });
    }
}
