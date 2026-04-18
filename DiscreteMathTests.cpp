using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscreteMathToolkit
{
    public static class DiscreteMathTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("Running Tests...\n");

            TestSetOperations();
            TestRelations();
            TestCombinatorics();

            Console.WriteLine("\nAll tests executed.");
        }

        private static void AssertEqual<T>(T expected, T actual, string testName)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Console.WriteLine($"[FAIL] {testName} | Expected: {expected}, Got: {actual}");
            }
            else
            {
                Console.WriteLine($"[PASS] {testName}");
            }
        }

        private static void TestSetOperations()
        {
            Console.WriteLine("Testing Set Operations...");

            var A = new HashSet<int> { 1, 2, 3 };
            var B = new HashSet<int> { 3, 4 };

            var union = A.Union(B).ToHashSet();
            var expectedUnion = new HashSet<int> { 1, 2, 3, 4 };

            AssertEqual(true, union.SetEquals(expectedUnion), "Union Test");

            var intersection = A.Intersect(B).ToHashSet();
            AssertEqual(true, intersection.SetEquals(new HashSet<int> { 3 }), "Intersection Test");
        }

        private static void TestRelations()
        {
            Console.WriteLine("\nTesting Relations...");

            var relation = new List<(int, int)>
            {
                (1,1), (2,2), (1,2), (2,1)
            };

            bool isSymmetric = relation.All(p => relation.Contains((p.Item2, p.Item1)));

            AssertEqual(true, isSymmetric, "Symmetry Test");
        }

        private static void TestCombinatorics()
        {
            Console.WriteLine("\nTesting Combinatorics...");

            int factorial5 = Factorial(5);
            AssertEqual(120, factorial5, "Factorial Test");

            int comb = Combination(5, 2);
            AssertEqual(10, comb, "Combination Test");
        }

        private static int Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }

        private static int Combination(int n, int r)
        {
            return Factorial(n) / (Factorial(r) * Factorial(n - r));
        }
    }
}