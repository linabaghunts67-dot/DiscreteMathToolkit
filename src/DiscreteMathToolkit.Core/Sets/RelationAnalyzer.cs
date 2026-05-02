namespace DiscreteMathToolkit.Core.Sets;

public sealed class RelationProperties
{
    public bool Reflexive { get; }
    public bool Irreflexive { get; }
    public bool Symmetric { get; }
    public bool Antisymmetric { get; }
    public bool Transitive { get; }
    public bool IsEquivalence => Reflexive && Symmetric && Transitive;
    public bool IsPartialOrder => Reflexive && Antisymmetric && Transitive;

    public IReadOnlyList<string> Failures { get; }

    public RelationProperties(bool reflexive, bool irreflexive, bool symmetric, bool antisymmetric, bool transitive, IReadOnlyList<string> failures)
    {
        Reflexive = reflexive;
        Irreflexive = irreflexive;
        Symmetric = symmetric;
        Antisymmetric = antisymmetric;
        Transitive = transitive;
        Failures = failures;
    }
}

/// <summary>
/// Analyzes a binary relation R ⊆ A×A. The set A is supplied separately so that we can
/// correctly detect failures of reflexivity even when the relation is empty.
/// </summary>
public static class RelationAnalyzer
{
    public static RelationProperties Analyze(IEnumerable<int> baseSet, IEnumerable<(int A, int B)> relation)
    {
        var elems = new HashSet<int>(baseSet);
        var rel = new HashSet<(int, int)>(relation);
        // sanity: ensure the relation is on the base set
        foreach (var (a, b) in rel)
        {
            if (!elems.Contains(a) || !elems.Contains(b))
                throw new InvalidOperationException($"Pair ({a},{b}) references an element outside the base set.");
        }

        var failures = new List<string>();

        bool reflexive = true, irreflexive = true;
        foreach (var x in elems)
        {
            bool selfLoop = rel.Contains((x, x));
            if (!selfLoop) { reflexive = false; failures.Add($"Reflexivity fails: ({x},{x}) is missing."); }
            if (selfLoop) irreflexive = false;
        }
        // dedupe-ish: keep at most a few example failures per property
        TrimFailures(failures, "Reflexivity fails");

        bool symmetric = true;
        foreach (var (a, b) in rel)
        {
            if (!rel.Contains((b, a)))
            {
                symmetric = false;
                failures.Add($"Symmetry fails: ({a},{b}) ∈ R but ({b},{a}) ∉ R.");
            }
        }
        TrimFailures(failures, "Symmetry fails");

        bool antisymmetric = true;
        foreach (var (a, b) in rel)
        {
            if (a != b && rel.Contains((b, a)))
            {
                antisymmetric = false;
                failures.Add($"Antisymmetry fails: ({a},{b}) and ({b},{a}) both in R with {a} ≠ {b}.");
            }
        }
        TrimFailures(failures, "Antisymmetry fails");

        // transitivity: for each (a,b), (b,c) check (a,c)
        bool transitive = true;
        // index by first element for speed
        var byFirst = rel.GroupBy(p => p.Item1).ToDictionary(g => g.Key, g => g.Select(p => p.Item2).ToHashSet());
        foreach (var (a, b) in rel)
        {
            if (!byFirst.TryGetValue(b, out var seconds)) continue;
            foreach (var c in seconds)
            {
                if (!rel.Contains((a, c)))
                {
                    transitive = false;
                    failures.Add($"Transitivity fails: ({a},{b}) and ({b},{c}) ∈ R but ({a},{c}) ∉ R.");
                }
            }
        }
        TrimFailures(failures, "Transitivity fails");

        return new RelationProperties(reflexive, irreflexive, symmetric, antisymmetric, transitive, failures);
    }

    private static void TrimFailures(List<string> failures, string prefix)
    {
        const int maxPerKind = 3;
        var matching = failures.Where(f => f.StartsWith(prefix)).ToList();
        if (matching.Count <= maxPerKind) return;
        // keep first maxPerKind, drop the rest
        int keep = 0;
        for (int i = failures.Count - 1; i >= 0; i--)
        {
            if (!failures[i].StartsWith(prefix)) continue;
            keep++;
            if (keep > maxPerKind) failures.RemoveAt(i);
        }
        failures.Add($"… and {matching.Count - maxPerKind} more failures of {prefix.ToLowerInvariant()}.");
    }

    /// <summary>Build the boolean matrix representation in the natural sorted order of the base set.</summary>
    public static (int[] Order, bool[,] Matrix) ToMatrix(IEnumerable<int> baseSet, IEnumerable<(int, int)> relation)
    {
        var order = baseSet.Distinct().OrderBy(x => x).ToArray();
        var index = new Dictionary<int, int>();
        for (int i = 0; i < order.Length; i++) index[order[i]] = i;
        var m = new bool[order.Length, order.Length];
        foreach (var (a, b) in relation)
        {
            if (index.TryGetValue(a, out var i) && index.TryGetValue(b, out var j))
                m[i, j] = true;
        }
        return (order, m);
    }
}
