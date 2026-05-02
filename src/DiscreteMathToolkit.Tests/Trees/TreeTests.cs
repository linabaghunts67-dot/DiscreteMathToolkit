using DiscreteMathToolkit.Core.Trees;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.Trees;

public class TreeTests
{
    private static BinaryTree BuildSampleTree()
    {
        //        4
        //       / \
        //      2   6
        //     / \   \
        //    1   3   7
        var t = BinaryTree.BuildBst(new[] { 4, 2, 6, 1, 3, 7 });
        return t;
    }

    [Fact]
    public void PreOrder_RootBeforeChildren()
    {
        var t = BuildSampleTree();
        var trace = TreeTraversals.Traverse(t, TraversalKind.PreOrder);
        trace.Result.Should().Equal(4, 2, 1, 3, 6, 7);
    }

    [Fact]
    public void InOrder_OnBst_IsSorted()
    {
        var t = BuildSampleTree();
        var trace = TreeTraversals.Traverse(t, TraversalKind.InOrder);
        trace.Result.Should().Equal(1, 2, 3, 4, 6, 7);
    }

    [Fact]
    public void PostOrder_RootLast()
    {
        var t = BuildSampleTree();
        var trace = TreeTraversals.Traverse(t, TraversalKind.PostOrder);
        trace.Result.Should().Equal(1, 3, 2, 7, 6, 4);
    }

    [Fact]
    public void LevelOrder_TopDown()
    {
        var t = BuildSampleTree();
        var trace = TreeTraversals.Traverse(t, TraversalKind.LevelOrder);
        trace.Result.Should().Equal(4, 2, 6, 1, 3, 7);
    }

    [Fact]
    public void Reconstruction_FromPreAndIn_RestoresStructure()
    {
        var t = BuildSampleTree();
        var pre = TreeTraversals.Traverse(t, TraversalKind.PreOrder).Result;
        var inO = TreeTraversals.Traverse(t, TraversalKind.InOrder).Result;
        var rebuilt = TreeReconstruction.FromPreOrderAndInOrder(pre, inO);

        TreeTraversals.Traverse(rebuilt, TraversalKind.PostOrder).Result
            .Should().Equal(TreeTraversals.Traverse(t, TraversalKind.PostOrder).Result);
    }

    [Fact]
    public void Reconstruction_FromPostAndIn_RestoresStructure()
    {
        var t = BuildSampleTree();
        var post = TreeTraversals.Traverse(t, TraversalKind.PostOrder).Result;
        var inO = TreeTraversals.Traverse(t, TraversalKind.InOrder).Result;
        var rebuilt = TreeReconstruction.FromPostOrderAndInOrder(post, inO);

        TreeTraversals.Traverse(rebuilt, TraversalKind.PreOrder).Result
            .Should().Equal(TreeTraversals.Traverse(t, TraversalKind.PreOrder).Result);
    }

    [Fact]
    public void Reconstruction_RejectsMismatchedSequences()
    {
        Assert.Throws<ArgumentException>(() =>
            TreeReconstruction.FromPreOrderAndInOrder(new[] { 1, 2 }, new[] { 1, 3 }));
    }

    [Fact]
    public void Bst_InsertAndSearch()
    {
        var t = new BinaryTree();
        t.Insert(5); t.Insert(3); t.Insert(7);
        t.Search(5).Should().BeTrue();
        t.Search(3).Should().BeTrue();
        t.Search(7).Should().BeTrue();
        t.Search(42).Should().BeFalse();
    }

    [Fact]
    public void Bst_DeleteRemovesNodeButPreservesOrder()
    {
        var t = BuildSampleTree();
        t.Delete(2);
        TreeTraversals.Traverse(t, TraversalKind.InOrder).Result
            .Should().Equal(1, 3, 4, 6, 7);
        t.Search(2).Should().BeFalse();
    }

    [Fact]
    public void Bst_DeleteMissingValueReturnsFalse()
    {
        var t = BuildSampleTree();
        t.Delete(999).Should().BeFalse();
    }

    [Fact]
    public void Bst_HeightAndCount()
    {
        var t = BuildSampleTree();
        t.Count.Should().Be(6);
        t.Height.Should().Be(2);
    }
}
