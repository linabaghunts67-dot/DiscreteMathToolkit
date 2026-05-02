namespace DiscreteMathToolkit.Core.Sets;

/// <summary>
/// Pure-functional set operations on <see cref="int"/> sets. The "universe" is optional;
/// if supplied it is required for <see cref="Complement"/>.
/// </summary>
public static class SetOperations
{
    public static IReadOnlySet<int> Union(IEnumerable<int> a, IEnumerable<int> b)
    {
        var s = new HashSet<int>(a);
        s.UnionWith(b);
        return s;
    }

    public static IReadOnlySet<int> Intersection(IEnumerable<int> a, IEnumerable<int> b)
    {
        var s = new HashSet<int>(a);
        s.IntersectWith(b);
        return s;
    }

    public static IReadOnlySet<int> Difference(IEnumerable<int> a, IEnumerable<int> b)
    {
        var s = new HashSet<int>(a);
        s.ExceptWith(b);
        return s;
    }

    public static IReadOnlySet<int> SymmetricDifference(IEnumerable<int> a, IEnumerable<int> b)
    {
        var s = new HashSet<int>(a);
        s.SymmetricExceptWith(b);
        return s;
    }

    public static IReadOnlySet<int> Complement(IEnumerable<int> a, IEnumerable<int> universe)
    {
        var u = new HashSet<int>(universe);
        var s = new HashSet<int>(a);
        if (!s.IsSubsetOf(u))
            throw new InvalidOperationException("Cannot take a complement: A is not a subset of the universe.");
        u.ExceptWith(s);
        return u;
    }

    public static IReadOnlyList<(int A, int B)> CartesianProduct(IEnumerable<int> a, IEnumerable<int> b)
    {
        var bList = b as IList<int> ?? b.ToList();
        var result = new List<(int, int)>();
        foreach (var x in a)
            foreach (var y in bList)
                result.Add((x, y));
        return result;
    }

    public static IReadOnlyList<IReadOnlySet<int>> PowerSet(IEnumerable<int> a)
    {
        var arr = a.Distinct().OrderBy(x => x).ToArray();
        if (arr.Length > 20) throw new InvalidOperationException("PowerSet: refusing to enumerate beyond 20 elements (2^20 subsets).");
        int n = 1 << arr.Length;
        var result = new List<IReadOnlySet<int>>(n);
        for (int mask = 0; mask < n; mask++)
        {
            var sub = new HashSet<int>();
            for (int i = 0; i < arr.Length; i++)
                if (((mask >> i) & 1) == 1) sub.Add(arr[i]);
            result.Add(sub);
        }
        return result;
    }
}
