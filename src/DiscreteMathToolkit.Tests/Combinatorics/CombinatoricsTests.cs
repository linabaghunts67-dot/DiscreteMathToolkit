using System.Numerics;
using DiscreteMathToolkit.Core.Combinatorics;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.Combinatorics;

public class CombinatoricsTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    public void Factorial_KnownValues(int n, long expected)
    {
        CombinatoricsCalculator.Factorial(n).Should().Be(new BigInteger(expected));
    }

    [Fact]
    public void Factorial_BigInputDoesNotOverflow()
    {
        // 25! = 15511210043330985984000000 — well beyond long range
        var f = CombinatoricsCalculator.Factorial(25);
        f.Should().Be(BigInteger.Parse("15511210043330985984000000"));
    }

    [Fact]
    public void Factorial_NegativeThrows()
    {
        Assert.Throws<ArgumentException>(() => CombinatoricsCalculator.Factorial(-1));
    }

    [Theory]
    [InlineData(5, 0, 1)]
    [InlineData(5, 1, 5)]
    [InlineData(5, 2, 20)]
    [InlineData(5, 5, 120)]
    [InlineData(10, 3, 720)]
    public void Permutations_KnownValues(int n, int k, long expected)
    {
        CombinatoricsCalculator.Permutations(n, k).Should().Be(new BigInteger(expected));
    }

    [Theory]
    [InlineData(5, 0, 1)]
    [InlineData(5, 2, 10)]
    [InlineData(5, 5, 1)]
    [InlineData(10, 3, 120)]
    [InlineData(52, 5, 2598960)]
    public void Combinations_KnownValues(int n, int k, long expected)
    {
        CombinatoricsCalculator.Combinations(n, k).Should().Be(new BigInteger(expected));
    }

    [Fact]
    public void Combinations_Symmetric()
    {
        CombinatoricsCalculator.Combinations(20, 7)
            .Should().Be(CombinatoricsCalculator.Combinations(20, 13));
    }

    [Fact]
    public void Combinations_KGreaterThanN_Throws()
    {
        Assert.Throws<ArgumentException>(() => CombinatoricsCalculator.Combinations(3, 5));
    }

    [Theory]
    [InlineData(2, 3, 8)]      // 2^3
    [InlineData(10, 4, 10000)]
    [InlineData(5, 0, 1)]
    public void VariationsWithRepetition_KnownValues(int n, int k, long expected)
    {
        CombinatoricsCalculator.VariationsWithRepetition(n, k).Should().Be(new BigInteger(expected));
    }

    [Theory]
    [InlineData(3, 2, 6)]      // C(4,2) = 6
    [InlineData(5, 3, 35)]     // C(7,3) = 35
    [InlineData(10, 0, 1)]
    public void CombinationsWithRepetition_KnownValues(int n, int k, long expected)
    {
        CombinatoricsCalculator.CombinationsWithRepetition(n, k).Should().Be(new BigInteger(expected));
    }

    [Fact]
    public void PascalTriangle_RowSumsArePowersOfTwo()
    {
        var triangle = CombinatoricsCalculator.PascalTriangle(8);
        for (int i = 0; i < triangle.Count; i++)
        {
            BigInteger sum = triangle[i].Aggregate(BigInteger.Zero, (a, b) => a + b);
            sum.Should().Be(BigInteger.Pow(2, i));
        }
    }

    [Fact]
    public void PascalTriangle_EdgesAreOnes()
    {
        var triangle = CombinatoricsCalculator.PascalTriangle(6);
        foreach (var row in triangle)
        {
            row[0].Should().Be(BigInteger.One);
            row[row.Count - 1].Should().Be(BigInteger.One);
        }
    }
}
