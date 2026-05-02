using System.Numerics;

namespace DiscreteMathToolkit.Core.Combinatorics;

/// <summary>
/// Exact combinatorial functions using <see cref="BigInteger"/> so that we never overflow.
/// </summary>
public static class CombinatoricsCalculator
{
    public static BigInteger Factorial(int n)
    {
        if (n < 0) throw new ArgumentException("Factorial is undefined for negative integers.", nameof(n));
        if (n > 10000) throw new ArgumentException("n is too large (max 10000) for this educational tool.", nameof(n));
        BigInteger r = 1;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
    }

    /// <summary>P(n, k) = n! / (n - k)! — number of k-permutations of n.</summary>
    public static BigInteger Permutations(int n, int k)
    {
        Validate(n, k);
        BigInteger r = 1;
        for (int i = 0; i < k; i++) r *= (n - i);
        return r;
    }

    /// <summary>C(n, k) = n! / (k! (n-k)!) — number of k-subsets.</summary>
    public static BigInteger Combinations(int n, int k)
    {
        Validate(n, k);
        if (k > n - k) k = n - k;          // symmetry
        BigInteger r = 1;
        for (int i = 0; i < k; i++)
        {
            r *= (n - i);
            r /= (i + 1);                 // exact division at every step
        }
        return r;
    }

    /// <summary>Variations / arrangements with repetition: n^k.</summary>
    public static BigInteger VariationsWithRepetition(int n, int k)
    {
        if (n < 0 || k < 0) throw new ArgumentException("n and k must be non-negative.");
        return BigInteger.Pow(n, k);
    }

    /// <summary>Number of ways to choose k items from n with repetition allowed: C(n + k - 1, k).</summary>
    public static BigInteger CombinationsWithRepetition(int n, int k)
    {
        if (n < 0 || k < 0) throw new ArgumentException("n and k must be non-negative.");
        if (n == 0) return k == 0 ? 1 : 0;
        return Combinations(n + k - 1, k);
    }

    /// <summary>Generate Pascal's triangle as a list of rows, row i has i+1 entries.</summary>
    public static IReadOnlyList<IReadOnlyList<BigInteger>> PascalTriangle(int rows)
    {
        if (rows < 0) throw new ArgumentException("Row count must be non-negative.", nameof(rows));
        if (rows > 100) throw new ArgumentException("Row count too large for display (max 100).", nameof(rows));
        var triangle = new List<IReadOnlyList<BigInteger>>();
        for (int i = 0; i <= rows; i++)
        {
            var row = new BigInteger[i + 1];
            row[0] = row[i] = 1;
            for (int j = 1; j < i; j++)
            {
                var prev = triangle[i - 1];
                row[j] = prev[j - 1] + prev[j];
            }
            triangle.Add(row);
        }
        return triangle;
    }

    private static void Validate(int n, int k)
    {
        if (n < 0 || k < 0) throw new ArgumentException("n and k must be non-negative.");
        if (k > n) throw new ArgumentException("k must not exceed n.", nameof(k));
        if (n > 10000) throw new ArgumentException("n is too large (max 10000) for this educational tool.", nameof(n));
    }
}
