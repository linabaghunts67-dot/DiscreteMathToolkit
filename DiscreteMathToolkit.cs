using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
    ============================================================================
    DiscreteMathToolkit.cs
    ----------------------------------------------------------------------------
    A single-file C# project that demonstrates several important areas of
    discrete mathematics in a practical, algorithmic way.

    MODULES INCLUDED
    1. Set operations
    2. Relations on a finite set
    3. Graph theory algorithms
    4. Propositional logic truth tables
    5. Basic combinatorics

    WHY THIS IS A GOOD DISCRETE MATH PROJECT
    - It connects theory with implementation.
    - It includes finite sets, relations, logic, graphs, and counting.
    - It is modular, documented, and can be extended.
    - It can be presented as an educational project for a university course.

    HOW TO RUN
    - Save as DiscreteMathToolkit.cs
    - Compile with:
        csc DiscreteMathToolkit.cs
      or with .NET SDK:
        dotnet new console
        replace Program.cs with this file content
        dotnet run

    NOTE
    This project is intentionally written in one file because the user preferred
    a one-file solution. In real production software, each class would normally
    be placed in its own file.
    ============================================================================
*/

namespace DiscreteMathToolkitProject
{
    public static class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            PrintHeader();

            while (true)
            {
                Console.WriteLine("\nMAIN MENU");
                Console.WriteLine("1. Set Operations");
                Console.WriteLine("2. Relation Analyzer");
                Console.WriteLine("3. Graph Theory Toolkit");
                Console.WriteLine("4. Propositional Logic Truth Table");
                Console.WriteLine("5. Combinatorics Calculator");
                Console.WriteLine("0. Exit");
                Console.Write("Choose a module: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SetModule.Run();
                        break;
                    case "2":
                        RelationModule.Run();
                        break;
                    case "3":
                        GraphModule.Run();
                        break;
                    case "4":
                        LogicModule.Run();
                        break;
                    case "5":
                        CombinatoricsModule.Run();
                        break;
                    case "0":
                        Console.WriteLine("Goodbye.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine("============================================================");
            Console.WriteLine("         DISCRETE MATHEMATICS TOOLKIT IN C#");
            Console.WriteLine("============================================================");
            Console.WriteLine("This project demonstrates how discrete mathematics concepts");
            Console.WriteLine("can be modeled and solved algorithmically.");
            Console.WriteLine("Included topics: sets, relations, graphs, logic, counting.");
            Console.WriteLine("============================================================");
        }
    }

    // ========================================================================
    // 1. SET OPERATIONS MODULE
    // ========================================================================
    public static class SetModule
    {
        public static void Run()
        {
            Console.WriteLine("\n--- SET OPERATIONS MODULE ---");
            Console.WriteLine("Enter elements separated by spaces.");

            HashSet<string> setA = ReadSet("A");
            HashSet<string> setB = ReadSet("B");

            while (true)
            {
                Console.WriteLine("\nSet Menu");
                Console.WriteLine("1. Show sets");
                Console.WriteLine("2. Union A ∪ B");
                Console.WriteLine("3. Intersection A ∩ B");
                Console.WriteLine("4. Difference A - B");
                Console.WriteLine("5. Difference B - A");
                Console.WriteLine("6. Symmetric Difference");
                Console.WriteLine("7. Cartesian Product A × B");
                Console.WriteLine("8. Check subset");
                Console.WriteLine("9. Power set of A");
                Console.WriteLine("0. Back");
                Console.Write("Choose: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.WriteLine($"A = {FormatSet(setA)}");
                        Console.WriteLine($"B = {FormatSet(setB)}");
                        break;
                    case "2":
                        Console.WriteLine($"A ∪ B = {FormatSet(new HashSet<string>(setA.Union(setB)))}");
                        break;
                    case "3":
                        Console.WriteLine($"A ∩ B = {FormatSet(new HashSet<string>(setA.Intersect(setB)))}");
                        break;
                    case "4":
                        Console.WriteLine($"A - B = {FormatSet(new HashSet<string>(setA.Except(setB)))}");
                        break;
                    case "5":
                        Console.WriteLine($"B - A = {FormatSet(new HashSet<string>(setB.Except(setA)))}");
                        break;
                    case "6":
                        Console.WriteLine($"Symmetric Difference = {FormatSet(SymmetricDifference(setA, setB))}");
                        break;
                    case "7":
                        PrintCartesianProduct(setA, setB);
                        break;
                    case "8":
                        Console.WriteLine($"A ⊆ B ? {setA.IsSubsetOf(setB)}");
                        Console.WriteLine($"B ⊆ A ? {setB.IsSubsetOf(setA)}");
                        break;
                    case "9":
                        PrintPowerSet(setA);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static HashSet<string> ReadSet(string name)
        {
            Console.Write($"Enter elements of set {name}: ");
            string input = Console.ReadLine() ?? string.Empty;
            return new HashSet<string>(input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string FormatSet(IEnumerable<string> set)
        {
            return "{" + string.Join(", ", set.OrderBy(x => x)) + "}";
        }

        private static HashSet<string> SymmetricDifference(HashSet<string> a, HashSet<string> b)
        {
            HashSet<string> result = new HashSet<string>(a);
            result.SymmetricExceptWith(b);
            return result;
        }

        private static void PrintCartesianProduct(HashSet<string> a, HashSet<string> b)
        {
            List<string> pairs = new List<string>();
            foreach (string x in a.OrderBy(x => x))
            {
                foreach (string y in b.OrderBy(y => y))
                {
                    pairs.Add($"({x}, {y})");
                }
            }

            Console.WriteLine("A × B = {" + string.Join(", ", pairs) + "}");
            Console.WriteLine($"|A × B| = {a.Count * b.Count}");
        }

        private static void PrintPowerSet(HashSet<string> set)
        {
            List<string> items = set.OrderBy(x => x).ToList();
            int n = items.Count;
            int subsetCount = 1 << n;

            Console.WriteLine("Power set P(A):");
            for (int mask = 0; mask < subsetCount; mask++)
            {
                List<string> subset = new List<string>();
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        subset.Add(items[i]);
                    }
                }
                Console.WriteLine("{" + string.Join(", ", subset) + "}");
            }

            Console.WriteLine($"|P(A)| = 2^{n} = {subsetCount}");
        }
    }

    // ========================================================================
    // 2. RELATION ANALYZER MODULE
    // ========================================================================
    public static class RelationModule
    {
        public static void Run()
        {
            Console.WriteLine("\n--- RELATION ANALYZER MODULE ---");
            Console.WriteLine("We work with a finite set A and a relation R on A.");
            Console.WriteLine("Example set input: 1 2 3");
            Console.WriteLine("Example pair input: 1,1 1,2 2,3");

            List<string> elements = ReadElements();
            HashSet<(string, string)> relation = ReadRelationPairs(elements);

            while (true)
            {
                Console.WriteLine("\nRelation Menu");
                Console.WriteLine("1. Show relation");
                Console.WriteLine("2. Show adjacency matrix");
                Console.WriteLine("3. Check reflexive");
                Console.WriteLine("4. Check symmetric");
                Console.WriteLine("5. Check antisymmetric");
                Console.WriteLine("6. Check transitive");
                Console.WriteLine("7. Full classification");
                Console.WriteLine("8. Show transitive closure (Warshall)");
                Console.WriteLine("0. Back");
                Console.Write("Choose: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        PrintRelation(relation);
                        break;
                    case "2":
                        PrintMatrix(elements, relation);
                        break;
                    case "3":
                        Console.WriteLine($"Reflexive: {IsReflexive(elements, relation)}");
                        break;
                    case "4":
                        Console.WriteLine($"Symmetric: {IsSymmetric(relation)}");
                        break;
                    case "5":
                        Console.WriteLine($"Antisymmetric: {IsAntisymmetric(relation)}");
                        break;
                    case "6":
                        Console.WriteLine($"Transitive: {IsTransitive(elements, relation)}");
                        break;
                    case "7":
                        FullClassification(elements, relation);
                        break;
                    case "8":
                        PrintTransitiveClosure(elements, relation);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static List<string> ReadElements()
        {
            Console.Write("Enter elements of the base set: ");
            return (Console.ReadLine() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private static HashSet<(string, string)> ReadRelationPairs(List<string> elements)
        {
            HashSet<string> allowed = new HashSet<string>(elements);
            HashSet<(string, string)> relation = new HashSet<(string, string)>();

            Console.Write("Enter ordered pairs a,b separated by spaces: ");
            string input = Console.ReadLine() ?? string.Empty;
            string[] pairs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs)
            {
                string[] parts = pair.Split(',');
                if (parts.Length == 2 && allowed.Contains(parts[0]) && allowed.Contains(parts[1]))
                {
                    relation.Add((parts[0], parts[1]));
                }
            }

            return relation;
        }

        private static void PrintRelation(HashSet<(string, string)> relation)
        {
            string formatted = string.Join(", ", relation.OrderBy(p => p.Item1).ThenBy(p => p.Item2)
                .Select(p => $"({p.Item1}, {p.Item2})"));
            Console.WriteLine("R = {" + formatted + "}");
        }

        private static void PrintMatrix(List<string> elements, HashSet<(string, string)> relation)
        {
            Console.Write("    ");
            foreach (string e in elements)
                Console.Write($"{e,4}");
            Console.WriteLine();

            foreach (string row in elements)
            {
                Console.Write($"{row,4}");
                foreach (string col in elements)
                {
                    int value = relation.Contains((row, col)) ? 1 : 0;
                    Console.Write($"{value,4}");
                }
                Console.WriteLine();
            }
        }

        private static bool IsReflexive(List<string> elements, HashSet<(string, string)> relation)
        {
            return elements.All(x => relation.Contains((x, x)));
        }

        private static bool IsSymmetric(HashSet<(string, string)> relation)
        {
            return relation.All(p => relation.Contains((p.Item2, p.Item1)));
        }

        private static bool IsAntisymmetric(HashSet<(string, string)> relation)
        {
            foreach (var (a, b) in relation)
            {
                if (a != b && relation.Contains((b, a)))
                    return false;
            }
            return true;
        }

        private static bool IsTransitive(List<string> elements, HashSet<(string, string)> relation)
        {
            foreach (string a in elements)
            {
                foreach (string b in elements)
                {
                    foreach (string c in elements)
                    {
                        if (relation.Contains((a, b)) && relation.Contains((b, c)) && !relation.Contains((a, c)))
                            return false;
                    }
                }
            }
            return true;
        }

        private static void FullClassification(List<string> elements, HashSet<(string, string)> relation)
        {
            bool reflexive = IsReflexive(elements, relation);
            bool symmetric = IsSymmetric(relation);
            bool antisymmetric = IsAntisymmetric(relation);
            bool transitive = IsTransitive(elements, relation);

            Console.WriteLine($"Reflexive: {reflexive}");
            Console.WriteLine($"Symmetric: {symmetric}");
            Console.WriteLine($"Antisymmetric: {antisymmetric}");
            Console.WriteLine($"Transitive: {transitive}");
            Console.WriteLine($"Equivalence relation: {reflexive && symmetric && transitive}");
            Console.WriteLine($"Partial order: {reflexive && antisymmetric && transitive}");
        }

        private static void PrintTransitiveClosure(List<string> elements, HashSet<(string, string)> relation)
        {
            int n = elements.Count;
            Dictionary<string, int> index = elements.Select((value, i) => new { value, i })
                                                   .ToDictionary(x => x.value, x => x.i);

            bool[,] matrix = new bool[n, n];
            foreach (var (a, b) in relation)
                matrix[index[a], index[b]] = true;

            // Warshall algorithm:
            // If i can reach k and k can reach j, then i can reach j.
            for (int k = 0; k < n; k++)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        matrix[i, j] = matrix[i, j] || (matrix[i, k] && matrix[k, j]);

            HashSet<(string, string)> closure = new HashSet<(string, string)>();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (matrix[i, j])
                        closure.Add((elements[i], elements[j]));

            Console.WriteLine("Transitive closure R+:");
            PrintRelation(closure);
            PrintMatrix(elements, closure);
        }
    }

    // ========================================================================
    // 3. GRAPH THEORY MODULE
    // ========================================================================
    public static class GraphModule
    {
        public static void Run()
        {
            Console.WriteLine("\n--- GRAPH THEORY TOOLKIT ---");
            Console.WriteLine("Vertices are strings. Example: A B C D");
            Console.WriteLine("Edges for undirected graph example: A,B A,C B,D");
            Console.WriteLine("Edges for directed graph example: A,B B,C C,D");

            List<string> vertices = ReadVertices();
            bool directed = ReadYesNo("Is the graph directed? (y/n): ");
            Graph graph = new Graph(vertices, directed);
            ReadEdges(graph, vertices);

            while (true)
            {
                Console.WriteLine("\nGraph Menu");
                Console.WriteLine("1. Show adjacency list");
                Console.WriteLine("2. Show adjacency matrix");
                Console.WriteLine("3. BFS traversal");
                Console.WriteLine("4. DFS traversal");
                Console.WriteLine("5. Connected components (undirected)");
                Console.WriteLine("6. Shortest path (unweighted)");
                Console.WriteLine("7. Detect cycle");
                Console.WriteLine("8. Topological sort (directed DAG only)");
                Console.WriteLine("9. Degree information");
                Console.WriteLine("0. Back");
                Console.Write("Choose: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        graph.PrintAdjacencyList();
                        break;
                    case "2":
                        graph.PrintAdjacencyMatrix();
                        break;
                    case "3":
                        Console.Write("Start vertex: ");
                        Console.WriteLine("BFS: " + string.Join(" -> ", graph.Bfs(Console.ReadLine() ?? string.Empty)));
                        break;
                    case "4":
                        Console.Write("Start vertex: ");
                        Console.WriteLine("DFS: " + string.Join(" -> ", graph.Dfs(Console.ReadLine() ?? string.Empty)));
                        break;
                    case "5":
                        if (graph.IsDirected)
                            Console.WriteLine("Connected components here are implemented for undirected graphs.");
                        else
                            graph.PrintConnectedComponents();
                        break;
                    case "6":
                        Console.Write("Start vertex: ");
                        string start = Console.ReadLine() ?? string.Empty;
                        Console.Write("End vertex: ");
                        string end = Console.ReadLine() ?? string.Empty;
                        graph.PrintShortestPath(start, end);
                        break;
                    case "7":
                        Console.WriteLine($"Contains cycle: {graph.HasCycle()}");
                        break;
                    case "8":
                        graph.PrintTopologicalSort();
                        break;
                    case "9":
                        graph.PrintDegreeInfo();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static List<string> ReadVertices()
        {
            Console.Write("Enter vertices: ");
            return (Console.ReadLine() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private static bool ReadYesNo(string prompt)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
            return input == "y" || input == "yes";
        }

        private static void ReadEdges(Graph graph, List<string> vertices)
        {
            HashSet<string> allowed = new HashSet<string>(vertices);
            Console.Write("Enter edges u,v separated by spaces: ");
            string input = Console.ReadLine() ?? string.Empty;
            string[] edges = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string edge in edges)
            {
                string[] parts = edge.Split(',');
                if (parts.Length == 2 && allowed.Contains(parts[0]) && allowed.Contains(parts[1]))
                    graph.AddEdge(parts[0], parts[1]);
            }
        }
    }

    public class Graph
    {
        private readonly Dictionary<string, List<string>> adjacency;
        public bool IsDirected { get; }

        public Graph(IEnumerable<string> vertices, bool isDirected)
        {
            IsDirected = isDirected;
            adjacency = new Dictionary<string, List<string>>();

            foreach (string v in vertices)
                adjacency[v] = new List<string>();
        }

        public void AddEdge(string u, string v)
        {
            if (!adjacency.ContainsKey(u) || !adjacency.ContainsKey(v))
                return;

            adjacency[u].Add(v);
            adjacency[u] = adjacency[u].Distinct().OrderBy(x => x).ToList();

            if (!IsDirected)
            {
                adjacency[v].Add(u);
                adjacency[v] = adjacency[v].Distinct().OrderBy(x => x).ToList();
            }
        }

        public void PrintAdjacencyList()
        {
            Console.WriteLine("Adjacency List:");
            foreach (var kvp in adjacency.OrderBy(x => x.Key))
            {
                Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
            }
        }

        public void PrintAdjacencyMatrix()
        {
            List<string> vertices = adjacency.Keys.OrderBy(x => x).ToList();
            Console.Write("    ");
            foreach (string v in vertices)
                Console.Write($"{v,4}");
            Console.WriteLine();

            foreach (string row in vertices)
            {
                Console.Write($"{row,4}");
                foreach (string col in vertices)
                {
                    int value = adjacency[row].Contains(col) ? 1 : 0;
                    Console.Write($"{value,4}");
                }
                Console.WriteLine();
            }
        }

        public List<string> Bfs(string start)
        {
            if (!adjacency.ContainsKey(start)) return new List<string>();

            List<string> order = new List<string>();
            HashSet<string> visited = new HashSet<string>();
            Queue<string> queue = new Queue<string>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                order.Add(current);

                foreach (string neighbor in adjacency[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return order;
        }

        public List<string> Dfs(string start)
        {
            if (!adjacency.ContainsKey(start)) return new List<string>();

            List<string> order = new List<string>();
            HashSet<string> visited = new HashSet<string>();
            DfsRecursive(start, visited, order);
            return order;
        }

        private void DfsRecursive(string current, HashSet<string> visited, List<string> order)
        {
            visited.Add(current);
            order.Add(current);

            foreach (string neighbor in adjacency[current])
            {
                if (!visited.Contains(neighbor))
                    DfsRecursive(neighbor, visited, order);
            }
        }

        public void PrintConnectedComponents()
        {
            HashSet<string> visited = new HashSet<string>();
            int componentNumber = 1;

            foreach (string vertex in adjacency.Keys.OrderBy(x => x))
            {
                if (!visited.Contains(vertex))
                {
                    List<string> component = new List<string>();
                    ExploreComponent(vertex, visited, component);
                    Console.WriteLine($"Component {componentNumber}: {string.Join(", ", component)}");
                    componentNumber++;
                }
            }
        }

        private void ExploreComponent(string start, HashSet<string> visited, List<string> component)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                component.Add(current);

                foreach (string neighbor in adjacency[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        public void PrintShortestPath(string start, string end)
        {
            if (!adjacency.ContainsKey(start) || !adjacency.ContainsKey(end))
            {
                Console.WriteLine("Invalid vertices.");
                return;
            }

            Dictionary<string, string?> parent = new Dictionary<string, string?>();
            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            visited.Add(start);
            parent[start] = null;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                if (current == end) break;

                foreach (string neighbor in adjacency[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (!visited.Contains(end))
            {
                Console.WriteLine("No path exists.");
                return;
            }

            List<string> path = new List<string>();
            string? step = end;
            while (step != null)
            {
                path.Add(step);
                step = parent[step];
            }
            path.Reverse();

            Console.WriteLine("Shortest path: " + string.Join(" -> ", path));
            Console.WriteLine($"Path length: {path.Count - 1}");
        }

        public bool HasCycle()
        {
            return IsDirected ? HasCycleDirected() : HasCycleUndirected();
        }

        private bool HasCycleUndirected()
        {
            HashSet<string> visited = new HashSet<string>();
            foreach (string vertex in adjacency.Keys)
            {
                if (!visited.Contains(vertex) && HasCycleUndirectedDfs(vertex, null, visited))
                    return true;
            }
            return false;
        }

        private bool HasCycleUndirectedDfs(string current, string? parent, HashSet<string> visited)
        {
            visited.Add(current);

            foreach (string neighbor in adjacency[current])
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasCycleUndirectedDfs(neighbor, current, visited))
                        return true;
                }
                else if (neighbor != parent)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasCycleDirected()
        {
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> recursionStack = new HashSet<string>();

            foreach (string vertex in adjacency.Keys)
            {
                if (HasCycleDirectedDfs(vertex, visited, recursionStack))
                    return true;
            }
            return false;
        }

        private bool HasCycleDirectedDfs(string current, HashSet<string> visited, HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(current)) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            recursionStack.Add(current);

            foreach (string neighbor in adjacency[current])
            {
                if (HasCycleDirectedDfs(neighbor, visited, recursionStack))
                    return true;
            }

            recursionStack.Remove(current);
            return false;
        }

        public void PrintTopologicalSort()
        {
            if (!IsDirected)
            {
                Console.WriteLine("Topological sorting is defined for directed graphs.");
                return;
            }

            Dictionary<string, int> indegree = adjacency.Keys.ToDictionary(v => v, v => 0);
            foreach (var kvp in adjacency)
            {
                foreach (string neighbor in kvp.Value)
                    indegree[neighbor]++;
            }

            Queue<string> queue = new Queue<string>(indegree.Where(x => x.Value == 0)
                                                           .Select(x => x.Key)
                                                           .OrderBy(x => x));
            List<string> result = new List<string>();

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                result.Add(current);

                foreach (string neighbor in adjacency[current])
                {
                    indegree[neighbor]--;
                    if (indegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            if (result.Count != adjacency.Count)
            {
                Console.WriteLine("Topological sort not possible: graph contains a cycle.");
                return;
            }

            Console.WriteLine("Topological order: " + string.Join(" -> ", result));
        }

        public void PrintDegreeInfo()
        {
            if (!IsDirected)
            {
                foreach (var kvp in adjacency.OrderBy(x => x.Key))
                    Console.WriteLine($"deg({kvp.Key}) = {kvp.Value.Count}");
            }
            else
            {
                Dictionary<string, int> indegree = adjacency.Keys.ToDictionary(v => v, v => 0);
                foreach (var kvp in adjacency)
                    foreach (string neighbor in kvp.Value)
                        indegree[neighbor]++;

                foreach (string v in adjacency.Keys.OrderBy(x => x))
                {
                    Console.WriteLine($"Vertex {v}: out-degree = {adjacency[v].Count}, in-degree = {indegree[v]}");
                }
            }
        }
    }

    // ========================================================================
    // 4. PROPOSITIONAL LOGIC MODULE
    // ========================================================================
    public static class LogicModule
    {
        public static void Run()
        {
            Console.WriteLine("\n--- PROPOSITIONAL LOGIC TRUTH TABLE MODULE ---");
            Console.WriteLine("Supported operators:");
            Console.WriteLine("!   negation");
            Console.WriteLine("&   conjunction");
            Console.WriteLine("|   disjunction");
            Console.WriteLine("^   exclusive OR");
            Console.WriteLine("->  implication");
            Console.WriteLine("<-> biconditional");
            Console.WriteLine("Example: (p -> q) & (!q -> !p)");

            Console.Write("Enter logical expression: ");
            string expression = Console.ReadLine() ?? string.Empty;

            try
            {
                TruthTable table = new TruthTable(expression);
                table.Print();
                table.PrintClassification();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    public class TruthTable
    {
        private readonly string originalExpression;
        private readonly List<string> variables;
        private readonly List<string> postfix;
        private readonly List<(Dictionary<string, bool> Assignment, bool Result)> rows;

        public TruthTable(string expression)
        {
            originalExpression = expression;
            List<string> tokens = Tokenize(expression);
            variables = ExtractVariables(tokens);
            postfix = ToPostfix(tokens);
            rows = BuildRows();
        }

        public void Print()
        {
            Console.WriteLine("\nTruth Table:");
            foreach (string variable in variables)
                Console.Write($"{variable,6}");
            Console.Write($"{originalExpression,15}");
            Console.WriteLine();

            foreach (var row in rows)
            {
                foreach (string variable in variables)
                    Console.Write($"{(row.Assignment[variable] ? 'T' : 'F'),6}");
                Console.Write($"{(row.Result ? 'T' : 'F'),15}");
                Console.WriteLine();
            }
        }

        public void PrintClassification()
        {
            bool allTrue = rows.All(r => r.Result);
            bool allFalse = rows.All(r => !r.Result);

            if (allTrue)
                Console.WriteLine("\nClassification: Tautology");
            else if (allFalse)
                Console.WriteLine("\nClassification: Contradiction");
            else
                Console.WriteLine("\nClassification: Contingency");
        }

        private List<(Dictionary<string, bool> Assignment, bool Result)> BuildRows()
        {
            List<(Dictionary<string, bool> Assignment, bool Result)> result = new List<(Dictionary<string, bool> Assignment, bool Result)>();
            int n = variables.Count;
            int total = 1 << n;

            for (int mask = 0; mask < total; mask++)
            {
                Dictionary<string, bool> assignment = new Dictionary<string, bool>();
                for (int i = 0; i < n; i++)
                {
                    bool value = ((mask >> (n - i - 1)) & 1) == 1;
                    assignment[variables[i]] = value;
                }

                bool formulaValue = EvaluatePostfix(postfix, assignment);
                result.Add((assignment, formulaValue));
            }

            return result;
        }

        private static List<string> Tokenize(string expression)
        {
            List<string> tokens = new List<string>();
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                    continue;

                if (char.IsLetter(c))
                {
                    tokens.Add(c.ToString());
                }
                else if (c == '(' || c == ')' || c == '!' || c == '&' || c == '|' || c == '^')
                {
                    tokens.Add(c.ToString());
                }
                else if (c == '-' && i + 1 < expression.Length && expression[i + 1] == '>')
                {
                    tokens.Add("->");
                    i++;
                }
                else if (c == '<' && i + 2 < expression.Length && expression[i + 1] == '-' && expression[i + 2] == '>')
                {
                    tokens.Add("<->");
                    i += 2;
                }
                else
                {
                    throw new ArgumentException($"Invalid character in expression: '{c}'");
                }
            }

            return tokens;
        }

        private static List<string> ExtractVariables(List<string> tokens)
        {
            return tokens.Where(t => t.Length == 1 && char.IsLetter(t[0]))
                         .Distinct()
                         .OrderBy(t => t)
                         .ToList();
        }

        private static int Precedence(string op)
        {
            return op switch
            {
                "!" => 5,
                "&" => 4,
                "|" => 3,
                "^" => 3,
                "->" => 2,
                "<->" => 1,
                _ => 0
            };
        }

        private static bool IsRightAssociative(string op)
        {
            return op == "!" || op == "->";
        }

        private static bool IsOperator(string token)
        {
            return token is "!" or "&" or "|" or "^" or "->" or "<->";
        }

        private static List<string> ToPostfix(List<string> tokens)
        {
            List<string> output = new List<string>();
            Stack<string> operators = new Stack<string>();

            foreach (string token in tokens)
            {
                if (token.Length == 1 && char.IsLetter(token[0]))
                {
                    output.Add(token);
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 && IsOperator(operators.Peek()))
                    {
                        string top = operators.Peek();
                        bool shouldPop = IsRightAssociative(token)
                            ? Precedence(token) < Precedence(top)
                            : Precedence(token) <= Precedence(top);

                        if (!shouldPop) break;
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                        output.Add(operators.Pop());

                    if (operators.Count == 0 || operators.Peek() != "(")
                        throw new ArgumentException("Mismatched parentheses.");

                    operators.Pop();
                }
            }

            while (operators.Count > 0)
            {
                string op = operators.Pop();
                if (op == "(" || op == ")")
                    throw new ArgumentException("Mismatched parentheses.");
                output.Add(op);
            }

            return output;
        }

        private static bool EvaluatePostfix(List<string> postfix, Dictionary<string, bool> assignment)
        {
            Stack<bool> stack = new Stack<bool>();

            foreach (string token in postfix)
            {
                if (token.Length == 1 && char.IsLetter(token[0]))
                {
                    stack.Push(assignment[token]);
                }
                else if (token == "!")
                {
                    if (stack.Count < 1) throw new ArgumentException("Invalid expression.");
                    stack.Push(!stack.Pop());
                }
                else
                {
                    if (stack.Count < 2) throw new ArgumentException("Invalid expression.");
                    bool right = stack.Pop();
                    bool left = stack.Pop();

                    bool value = token switch
                    {
                        "&" => left && right,
                        "|" => left || right,
                        "^" => left ^ right,
                        "->" => !left || right,
                        "<->" => left == right,
                        _ => throw new ArgumentException("Unknown operator.")
                    };

                    stack.Push(value);
                }
            }

            if (stack.Count != 1)
                throw new ArgumentException("Invalid expression.");

            return stack.Pop();
        }
    }

    // ========================================================================
    // 5. COMBINATORICS MODULE
    // ========================================================================
    public static class CombinatoricsModule
    {
        public static void Run()
        {
            while (true)
            {
                Console.WriteLine("\n--- COMBINATORICS CALCULATOR ---");
                Console.WriteLine("1. n!");
                Console.WriteLine("2. Permutations P(n, r)");
                Console.WriteLine("3. Combinations C(n, r)");
                Console.WriteLine("4. Binomial expansion coefficients for (a+b)^n");
                Console.WriteLine("0. Back");
                Console.Write("Choose: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Write("Enter n: ");
                        int n1 = ReadNonNegativeInt();
                        Console.WriteLine($"{n1}! = {Factorial(n1)}");
                        break;
                    case "2":
                        Console.Write("Enter n: ");
                        int n2 = ReadNonNegativeInt();
                        Console.Write("Enter r: ");
                        int r2 = ReadNonNegativeInt();
                        Console.WriteLine($"P({n2}, {r2}) = {Permutation(n2, r2)}");
                        break;
                    case "3":
                        Console.Write("Enter n: ");
                        int n3 = ReadNonNegativeInt();
                        Console.Write("Enter r: ");
                        int r3 = ReadNonNegativeInt();
                        Console.WriteLine($"C({n3}, {r3}) = {Combination(n3, r3)}");
                        break;
                    case "4":
                        Console.Write("Enter n: ");
                        int n4 = ReadNonNegativeInt();
                        PrintBinomialCoefficients(n4);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static int ReadNonNegativeInt()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int value) && value >= 0)
                    return value;
                Console.Write("Please enter a non-negative integer: ");
            }
        }

        private static long Factorial(int n)
        {
            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        private static long Permutation(int n, int r)
        {
            if (r > n) return 0;
            long result = 1;
            for (int i = 0; i < r; i++)
                result *= (n - i);
            return result;
        }

        private static long Combination(int n, int r)
        {
            if (r > n) return 0;
            r = Math.Min(r, n - r);
            long result = 1;
            for (int i = 1; i <= r; i++)
            {
                result = result * (n - r + i) / i;
            }
            return result;
        }

        private static void PrintBinomialCoefficients(int n)
        {
            Console.WriteLine($"Coefficients of (a + b)^{n}:");
            List<long> coefficients = new List<long>();

            for (int k = 0; k <= n; k++)
                coefficients.Add(Combination(n, k));

            Console.WriteLine(string.Join(" ", coefficients));
            Console.WriteLine("Expansion:");

            List<string> terms = new List<string>();
            for (int k = 0; k <= n; k++)
            {
                long coeff = Combination(n, k);
                int powerA = n - k;
                int powerB = k;

                string term = coeff == 1 ? "" : coeff.ToString();
                if (powerA > 0)
                    term += powerA == 1 ? "a" : $"a^{powerA}";
                if (powerB > 0)
                {
                    if (powerA > 0) term += "b";
                    else term += powerB == 1 ? "b" : $"b^{powerB}";
                }
                if (powerA == 0 && powerB == 0)
                    term = coeff.ToString();
                if (powerA > 0 && powerB > 1)
                    term = (coeff == 1 ? "" : coeff.ToString()) + (powerA == 1 ? "a" : $"a^{powerA}") + $"b^{powerB}";

                terms.Add(term);
            }

            Console.WriteLine(string.Join(" + ", terms));
        }
    }
}
