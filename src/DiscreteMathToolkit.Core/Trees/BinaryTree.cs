namespace DiscreteMathToolkit.Core.Trees;

public sealed class BinaryTreeNode
{
    public int Value { get; set; }
    public BinaryTreeNode? Left { get; set; }
    public BinaryTreeNode? Right { get; set; }

    public BinaryTreeNode(int value, BinaryTreeNode? left = null, BinaryTreeNode? right = null)
    {
        Value = value;
        Left = left;
        Right = right;
    }
}

/// <summary>
/// Binary tree operations. The tree is stored as an unbalanced BST when built via <see cref="BuildBst"/>;
/// the operations here are intentionally simple and educational rather than self-balancing.
/// </summary>
public sealed class BinaryTree
{
    public BinaryTreeNode? Root { get; private set; }

    public BinaryTree(BinaryTreeNode? root = null) { Root = root; }

    /// <summary>BST insert. Duplicates are ignored.</summary>
    public bool Insert(int value)
    {
        if (Root is null) { Root = new BinaryTreeNode(value); return true; }
        var cur = Root;
        while (true)
        {
            if (value == cur.Value) return false;
            if (value < cur.Value)
            {
                if (cur.Left is null) { cur.Left = new BinaryTreeNode(value); return true; }
                cur = cur.Left;
            }
            else
            {
                if (cur.Right is null) { cur.Right = new BinaryTreeNode(value); return true; }
                cur = cur.Right;
            }
        }
    }

    public bool Search(int value)
    {
        var cur = Root;
        while (cur != null)
        {
            if (value == cur.Value) return true;
            cur = value < cur.Value ? cur.Left : cur.Right;
        }
        return false;
    }

    /// <summary>Standard BST delete (Hibbard deletion using in-order successor).</summary>
    public bool Delete(int value)
    {
        var (newRoot, deleted) = DeleteRec(Root, value);
        Root = newRoot;
        return deleted;
    }

    private static (BinaryTreeNode? Node, bool Deleted) DeleteRec(BinaryTreeNode? node, int value)
    {
        if (node is null) return (null, false);
        if (value < node.Value)
        {
            var (left, del) = DeleteRec(node.Left, value);
            node.Left = left;
            return (node, del);
        }
        if (value > node.Value)
        {
            var (right, del) = DeleteRec(node.Right, value);
            node.Right = right;
            return (node, del);
        }
        // match
        if (node.Left is null) return (node.Right, true);
        if (node.Right is null) return (node.Left, true);

        // two children: replace with in-order successor
        var succParent = node;
        var succ = node.Right;
        while (succ.Left != null) { succParent = succ; succ = succ.Left; }
        node.Value = succ.Value;
        if (succParent == node) succParent.Right = succ.Right;
        else succParent.Left = succ.Right;
        return (node, true);
    }

    public static BinaryTree BuildBst(IEnumerable<int> values)
    {
        var t = new BinaryTree();
        foreach (var v in values) t.Insert(v);
        return t;
    }

    public int Count => CountRec(Root);
    private static int CountRec(BinaryTreeNode? n) =>
        n is null ? 0 : 1 + CountRec(n.Left) + CountRec(n.Right);

    public int Height => HeightRec(Root);
    private static int HeightRec(BinaryTreeNode? n) =>
        n is null ? -1 : 1 + Math.Max(HeightRec(n.Left), HeightRec(n.Right));
}
