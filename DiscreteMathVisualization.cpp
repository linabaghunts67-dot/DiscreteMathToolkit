using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscreteMathToolkit
{
    public static class DiscreteMathVisualization
    {
        public static void PrintSet(string name, HashSet<int> set)
        {
            Console.WriteLine($"{name} = {{ {string.Join(", ", set)} }}");
        }
        public static void PrintRelation(List<(int, int)> relation)
        {
            Console.WriteLine("Relation:");
            foreach (var (a, b) in relation)
            {
                Console.WriteLine($"({a}, {b})");
            }
        }
        public static void PrintGraph(Dictionary<int, List<int>> graph)
        {
            Console.WriteLine("Graph (Adjacency List):");
            foreach (var node in graph)
            {
                Console.Write($"{node.Key} -> ");
                Console.WriteLine(string.Join(", ", node.Value));
            }
        }
        public static void PrintBFS(Dictionary<int, List<int>> graph, int start)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            queue.Enqueue(start);
            visited.Add(start);

            Console.Write("BFS Order: ");

            while (queue.Count > 0)
            {
                int node = queue.Dequeue();
                Console.Write(node + " ");

                foreach (var neighbor in graph[node])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Console.WriteLine();
        }
        public static void PrintDFS(Dictionary<int, List<int>> graph, int start)
        {
            var visited = new HashSet<int>();
            Console.Write("DFS Order: ");
            DFS(graph, start, visited);
            Console.WriteLine();
        }

        private static void DFS(Dictionary<int, List<int>> graph, int node, HashSet<int> visited)
        {
            visited.Add(node);
            Console.Write(node + " ");

            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                {
                    DFS(graph, neighbor, visited);
                }
            }
        }
        public static void PrintTruthTable()
        {
            Console.WriteLine("Truth Table for P AND Q:");
            Console.WriteLine("P Q | Result");

            bool[] values = { true, false };

            foreach (var p in values)
            {
                foreach (var q in values)
                {
                    Console.WriteLine($"{p} {q} | {p && q}");
                }
            }
        }
    }
}