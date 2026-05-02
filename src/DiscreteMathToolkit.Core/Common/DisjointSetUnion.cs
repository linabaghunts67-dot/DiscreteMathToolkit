namespace DiscreteMathToolkit.Core.Common;

/// <summary>
/// Disjoint-Set Union with path compression and union by rank.
/// Effectively O(alpha(n)) per operation.
/// </summary>
public sealed class DisjointSetUnion
{
    private readonly Dictionary<int, int> _parent = new();
    private readonly Dictionary<int, int> _rank = new();

    public void MakeSet(int x)
    {
        if (_parent.ContainsKey(x)) return;
        _parent[x] = x;
        _rank[x] = 0;
    }

    public int Find(int x)
    {
        if (!_parent.ContainsKey(x))
            throw new InvalidOperationException($"Element {x} is not in any set.");
        int root = x;
        while (_parent[root] != root) root = _parent[root];
        // path compression
        int cur = x;
        while (_parent[cur] != root)
        {
            int next = _parent[cur];
            _parent[cur] = root;
            cur = next;
        }
        return root;
    }

    /// <summary>Returns true if a merge actually happened (i.e. the two sets were distinct).</summary>
    public bool Union(int a, int b)
    {
        int ra = Find(a), rb = Find(b);
        if (ra == rb) return false;
        if (_rank[ra] < _rank[rb])
            (ra, rb) = (rb, ra);
        _parent[rb] = ra;
        if (_rank[ra] == _rank[rb]) _rank[ra]++;
        return true;
    }

    public bool Connected(int a, int b) => Find(a) == Find(b);
}
