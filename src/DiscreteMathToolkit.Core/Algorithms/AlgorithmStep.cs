namespace DiscreteMathToolkit.Core.Algorithms;

/// <summary>
/// A single, replayable step in the execution of a visualizable algorithm.
/// Algorithms record steps; the visualization layer plays them back.
/// </summary>
/// <typeparam name="TState">Snapshot of relevant state at this step (e.g. distances, visited set).</typeparam>
public sealed class AlgorithmStep<TState>
{
    /// <summary>Zero-based index in the recorded sequence.</summary>
    public int Index { get; }

    /// <summary>Human-friendly description (shown in the explanation panel).</summary>
    public string Description { get; }

    /// <summary>Deep snapshot of the algorithm state at this step.</summary>
    public TState State { get; }

    /// <summary>Optional ids of nodes/edges to highlight in the UI.</summary>
    public IReadOnlyCollection<int> HighlightedNodes { get; }
    public IReadOnlyCollection<(int From, int To)> HighlightedEdges { get; }

    public AlgorithmStep(
        int index,
        string description,
        TState state,
        IReadOnlyCollection<int>? highlightedNodes = null,
        IReadOnlyCollection<(int, int)>? highlightedEdges = null)
    {
        Index = index;
        Description = description;
        State = state;
        HighlightedNodes = highlightedNodes ?? Array.Empty<int>();
        HighlightedEdges = highlightedEdges ?? Array.Empty<(int, int)>();
    }
}

/// <summary>
/// Result returned by every visualizable algorithm: the ordered steps plus any final answer payload.
/// </summary>
public sealed class AlgorithmTrace<TState, TResult>
{
    public IReadOnlyList<AlgorithmStep<TState>> Steps { get; }
    public TResult Result { get; }
    public string TimeComplexity { get; }
    public string SpaceComplexity { get; }

    public AlgorithmTrace(
        IReadOnlyList<AlgorithmStep<TState>> steps,
        TResult result,
        string timeComplexity,
        string spaceComplexity)
    {
        Steps = steps;
        Result = result;
        TimeComplexity = timeComplexity;
        SpaceComplexity = spaceComplexity;
    }
}
