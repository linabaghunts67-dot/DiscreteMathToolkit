using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Core.Trees;

public enum TraversalKind
{
    PreOrder,
    InOrder,
    PostOrder,
    LevelOrder
}

public sealed class TraversalState
{
    public IReadOnlyList<int> Visited { get; }
    public int? Current { get; }

    public TraversalState(IReadOnlyList<int> visited, int? current)
    {
        Visited = visited;
        Current = current;
    }
}

public static class TreeTraversals
{
    public static AlgorithmTrace<TraversalState, IReadOnlyList<int>> Traverse(BinaryTree tree, TraversalKind kind)
    {
        var visited = new List<int>();
        var steps = new List<AlgorithmStep<TraversalState>>();

        steps.Add(new AlgorithmStep<TraversalState>(0, $"Start {kind} traversal.",
            new TraversalState(new List<int>(visited), null)));

        if (tree.Root != null)
        {
            switch (kind)
            {
                case TraversalKind.PreOrder: PreOrder(tree.Root, visited, steps); break;
                case TraversalKind.InOrder: InOrder(tree.Root, visited, steps); break;
                case TraversalKind.PostOrder: PostOrder(tree.Root, visited, steps); break;
                case TraversalKind.LevelOrder: LevelOrder(tree.Root, visited, steps); break;
            }
        }

        steps.Add(new AlgorithmStep<TraversalState>(steps.Count,
            $"Traversal complete. Sequence: {string.Join(", ", visited)}.",
            new TraversalState(new List<int>(visited), null)));

        return new AlgorithmTrace<TraversalState, IReadOnlyList<int>>(
            steps, visited, "O(n)", "O(h) where h = tree height");
    }

    private static void PreOrder(BinaryTreeNode node, List<int> visited, List<AlgorithmStep<TraversalState>> steps)
    {
        visited.Add(node.Value);
        steps.Add(new AlgorithmStep<TraversalState>(steps.Count, $"Visit {node.Value} (root before subtrees).",
            new TraversalState(new List<int>(visited), node.Value), new[] { node.Value }));
        if (node.Left != null) PreOrder(node.Left, visited, steps);
        if (node.Right != null) PreOrder(node.Right, visited, steps);
    }

    private static void InOrder(BinaryTreeNode node, List<int> visited, List<AlgorithmStep<TraversalState>> steps)
    {
        if (node.Left != null) InOrder(node.Left, visited, steps);
        visited.Add(node.Value);
        steps.Add(new AlgorithmStep<TraversalState>(steps.Count, $"Visit {node.Value} (between subtrees).",
            new TraversalState(new List<int>(visited), node.Value), new[] { node.Value }));
        if (node.Right != null) InOrder(node.Right, visited, steps);
    }

    private static void PostOrder(BinaryTreeNode node, List<int> visited, List<AlgorithmStep<TraversalState>> steps)
    {
        if (node.Left != null) PostOrder(node.Left, visited, steps);
        if (node.Right != null) PostOrder(node.Right, visited, steps);
        visited.Add(node.Value);
        steps.Add(new AlgorithmStep<TraversalState>(steps.Count, $"Visit {node.Value} (root after subtrees).",
            new TraversalState(new List<int>(visited), node.Value), new[] { node.Value }));
    }

    private static void LevelOrder(BinaryTreeNode root, List<int> visited, List<AlgorithmStep<TraversalState>> steps)
    {
        var q = new Queue<BinaryTreeNode>();
        q.Enqueue(root);
        while (q.Count > 0)
        {
            var n = q.Dequeue();
            visited.Add(n.Value);
            steps.Add(new AlgorithmStep<TraversalState>(steps.Count, $"Visit {n.Value} (level order).",
                new TraversalState(new List<int>(visited), n.Value), new[] { n.Value }));
            if (n.Left != null) q.Enqueue(n.Left);
            if (n.Right != null) q.Enqueue(n.Right);
        }
    }
}
