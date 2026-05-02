using DiscreteMathToolkit.Core.Sets;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.Sets;

public class SetsAndRelationsTests
{
    [Fact]
    public void Union_CombinesUnique()
    {
        var u = SetOperations.Union(new[] { 1, 2, 3 }, new[] { 3, 4, 5 });
        u.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Intersection_KeepsOnlyShared()
    {
        var i = SetOperations.Intersection(new[] { 1, 2, 3, 4 }, new[] { 3, 4, 5 });
        i.Should().BeEquivalentTo(new[] { 3, 4 });
    }

    [Fact]
    public void Difference_KeepsLeftMinusRight()
    {
        var d = SetOperations.Difference(new[] { 1, 2, 3, 4 }, new[] { 3, 4, 5 });
        d.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void SymmetricDifference_OnlyExclusiveElements()
    {
        var s = SetOperations.SymmetricDifference(new[] { 1, 2, 3 }, new[] { 3, 4, 5 });
        s.Should().BeEquivalentTo(new[] { 1, 2, 4, 5 });
    }

    [Fact]
    public void Complement_RequiresUniverse()
    {
        var c = SetOperations.Complement(new[] { 1, 2 }, new[] { 1, 2, 3, 4 });
        c.Should().BeEquivalentTo(new[] { 3, 4 });
    }

    [Fact]
    public void Complement_RejectsAOutsideUniverse()
    {
        Assert.Throws<InvalidOperationException>(() =>
            SetOperations.Complement(new[] { 1, 99 }, new[] { 1, 2, 3 }));
    }

    [Fact]
    public void CartesianProduct_HasExpectedSize()
    {
        var p = SetOperations.CartesianProduct(new[] { 1, 2 }, new[] { 3, 4, 5 });
        p.Should().HaveCount(6);
        p.Should().Contain((1, 3));
        p.Should().Contain((2, 5));
    }

    [Fact]
    public void PowerSet_Has2ToTheNSubsets()
    {
        var ps = SetOperations.PowerSet(new[] { 1, 2, 3 });
        ps.Should().HaveCount(8);
    }

    [Fact]
    public void RelationAnalyzer_DetectsEquivalenceRelation()
    {
        var baseSet = new[] { 1, 2, 3 };
        var rel = new[]
        {
            (1, 1), (2, 2), (3, 3),
            (1, 2), (2, 1),
            (2, 3), (3, 2),
            (1, 3), (3, 1)
        };
        var props = RelationAnalyzer.Analyze(baseSet, rel);
        props.Reflexive.Should().BeTrue();
        props.Symmetric.Should().BeTrue();
        props.Transitive.Should().BeTrue();
        props.IsEquivalence.Should().BeTrue();
    }

    [Fact]
    public void RelationAnalyzer_DetectsPartialOrder()
    {
        var baseSet = new[] { 1, 2, 3 };
        // ≤ on {1,2,3}
        var rel = new[]
        {
            (1, 1), (2, 2), (3, 3),
            (1, 2), (1, 3), (2, 3)
        };
        var props = RelationAnalyzer.Analyze(baseSet, rel);
        props.Reflexive.Should().BeTrue();
        props.Antisymmetric.Should().BeTrue();
        props.Transitive.Should().BeTrue();
        props.IsPartialOrder.Should().BeTrue();
        props.IsEquivalence.Should().BeFalse();
    }

    [Fact]
    public void RelationAnalyzer_FindsTransitivityFailure()
    {
        var baseSet = new[] { 1, 2, 3 };
        var rel = new[] { (1, 2), (2, 3) };
        var props = RelationAnalyzer.Analyze(baseSet, rel);
        props.Transitive.Should().BeFalse();
        props.Failures.Should().Contain(f => f.StartsWith("Transitivity"));
    }

    [Fact]
    public void RelationAnalyzer_RejectsPairOutsideBaseSet()
    {
        Assert.Throws<InvalidOperationException>(() =>
            RelationAnalyzer.Analyze(new[] { 1, 2 }, new[] { (1, 99) }));
    }

    [Fact]
    public void RelationAnalyzer_BuildsBooleanMatrix()
    {
        var baseSet = new[] { 1, 2 };
        var rel = new[] { (1, 2), (2, 2) };
        var (order, matrix) = RelationAnalyzer.ToMatrix(baseSet, rel);
        order.Should().Equal(1, 2);
        matrix[0, 1].Should().BeTrue();   // (1,2)
        matrix[1, 1].Should().BeTrue();   // (2,2)
        matrix[0, 0].Should().BeFalse();
        matrix[1, 0].Should().BeFalse();
    }
}
