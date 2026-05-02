using DiscreteMathToolkit.Core.Trees;

namespace DiscreteMathToolkit.Visualization.Layout;

public sealed class TreeNodeLayout
{
    public BinaryTreeNode Node { get; }
    public Point2D Position { get; }
    public TreeNodeLayout? Left { get; }
    public TreeNodeLayout? Right { get; }

    public TreeNodeLayout(BinaryTreeNode node, Point2D pos, TreeNodeLayout? left, TreeNodeLayout? right)
    {
        Node = node;
        Position = pos;
        Left = left;
        Right = right;
    }

    public IEnumerable<TreeNodeLayout> Flatten()
    {
        yield return this;
        if (Left != null) foreach (var n in Left.Flatten()) yield return n;
        if (Right != null) foreach (var n in Right.Flatten()) yield return n;
    }
}

public sealed class TreeLayout
{
    public TreeNodeLayout? Root { get; }
    public double Width { get; }
    public double Height { get; }

    public TreeLayout(TreeNodeLayout? root, double width, double height)
    {
        Root = root;
        Width = width;
        Height = height;
    }
}

/// <summary>
/// Two-pass tree layout. First pass assigns "leaf indices" via in-order numbering;
/// second pass computes coordinates. This produces a clean layered look without
/// the full Reingold–Tilford bookkeeping, and is more than enough for trees up
/// to a few hundred nodes which is all we need pedagogically.
/// </summary>
public sealed class TreeLayoutEngine
{
    public double LevelHeight { get; init; } = 80;
    public double NodeSpacing { get; init; } = 60;
    public double Padding { get; init; } = 40;

    public TreeLayout Compute(BinaryTree tree, double availableWidth)
    {
        if (tree.Root == null) return new TreeLayout(null, availableWidth, 0);

        // Pass 1: assign in-order index to every node
        var inOrderIndex = new Dictionary<BinaryTreeNode, int>();
        int next = 0;
        AssignInOrder(tree.Root, inOrderIndex, ref next);

        int totalLeaves = inOrderIndex.Count;
        int treeHeight = HeightOf(tree.Root);

        double totalWidth = Math.Max(availableWidth, NodeSpacing * (totalLeaves + 1) + 2 * Padding);
        double totalHeight = LevelHeight * (treeHeight + 1) + 2 * Padding;

        // Pass 2: compute coordinates
        var rootLayout = Build(tree.Root, 0, inOrderIndex, totalWidth, totalLeaves);
        return new TreeLayout(rootLayout, totalWidth, totalHeight);
    }

    private static void AssignInOrder(BinaryTreeNode node, Dictionary<BinaryTreeNode, int> map, ref int counter)
    {
        if (node.Left != null) AssignInOrder(node.Left, map, ref counter);
        map[node] = counter++;
        if (node.Right != null) AssignInOrder(node.Right, map, ref counter);
    }

    private static int HeightOf(BinaryTreeNode? n) =>
        n is null ? -1 : 1 + Math.Max(HeightOf(n.Left), HeightOf(n.Right));

    private TreeNodeLayout Build(BinaryTreeNode node, int depth, Dictionary<BinaryTreeNode, int> idx, double totalWidth, int totalLeaves)
    {
        // distribute nodes evenly across the available width by in-order index
        double slot = (totalWidth - 2 * Padding) / Math.Max(1, totalLeaves);
        double x = Padding + (idx[node] + 0.5) * slot;
        double y = Padding + depth * LevelHeight + LevelHeight / 2.0;
        var pos = new Point2D(x, y);

        var left = node.Left != null ? Build(node.Left, depth + 1, idx, totalWidth, totalLeaves) : null;
        var right = node.Right != null ? Build(node.Right, depth + 1, idx, totalWidth, totalLeaves) : null;
        return new TreeNodeLayout(node, pos, left, right);
    }
}
