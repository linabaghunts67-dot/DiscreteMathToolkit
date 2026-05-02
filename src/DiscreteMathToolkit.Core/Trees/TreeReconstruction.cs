namespace DiscreteMathToolkit.Core.Trees;

/// <summary>
/// Reconstructs a unique binary tree from traversal pairs that uniquely determine the structure.
/// Pre+In or Post+In are sufficient (assuming distinct values).
/// </summary>
public static class TreeReconstruction
{
    public static BinaryTree FromPreOrderAndInOrder(IReadOnlyList<int> pre, IReadOnlyList<int> inOrder)
    {
        ValidatePair(pre, inOrder);
        var indexMap = BuildIndex(inOrder);
        int preIdx = 0;
        var root = Build(pre, ref preIdx, 0, inOrder.Count - 1, indexMap);
        return new BinaryTree(root);

        static BinaryTreeNode? Build(IReadOnlyList<int> pre, ref int preIdx, int lo, int hi, Dictionary<int, int> idx)
        {
            if (lo > hi) return null;
            int val = pre[preIdx++];
            var node = new BinaryTreeNode(val);
            int mid = idx[val];
            node.Left = Build(pre, ref preIdx, lo, mid - 1, idx);
            node.Right = Build(pre, ref preIdx, mid + 1, hi, idx);
            return node;
        }
    }

    public static BinaryTree FromPostOrderAndInOrder(IReadOnlyList<int> post, IReadOnlyList<int> inOrder)
    {
        ValidatePair(post, inOrder);
        var indexMap = BuildIndex(inOrder);
        int postIdx = post.Count - 1;
        var root = Build(post, ref postIdx, 0, inOrder.Count - 1, indexMap);
        return new BinaryTree(root);

        static BinaryTreeNode? Build(IReadOnlyList<int> post, ref int postIdx, int lo, int hi, Dictionary<int, int> idx)
        {
            if (lo > hi) return null;
            int val = post[postIdx--];
            var node = new BinaryTreeNode(val);
            int mid = idx[val];
            // build right first because we walk postorder right-to-left
            node.Right = Build(post, ref postIdx, mid + 1, hi, idx);
            node.Left = Build(post, ref postIdx, lo, mid - 1, idx);
            return node;
        }
    }

    private static void ValidatePair(IReadOnlyList<int> a, IReadOnlyList<int> b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (a.Count != b.Count)
            throw new ArgumentException("Traversal sequences must have the same length.");
        if (a.Count == 0) return;
        var setA = new HashSet<int>(a);
        if (setA.Count != a.Count)
            throw new ArgumentException("Reconstruction requires distinct values; duplicates were found.");
        if (!setA.SetEquals(b))
            throw new ArgumentException("Traversal sequences must contain the same set of values.");
    }

    private static Dictionary<int, int> BuildIndex(IReadOnlyList<int> seq)
    {
        var d = new Dictionary<int, int>(seq.Count);
        for (int i = 0; i < seq.Count; i++) d[seq[i]] = i;
        return d;
    }
}
