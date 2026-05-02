using DiscreteMathToolkit.Core.Graphs;
using DiscreteMathToolkit.Core.Graphs.Algorithms;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.Graphs;

public class GraphAlgorithmsTests
{
    private static Graph BuildSampleUndirected()
    {
        // 0 - 1 - 2
        //  \  |  /
        //    3
        var g = new Graph(directed: false, weighted: false);
        for (int i = 0; i < 4; i++) g.AddNode(i);
        g.AddEdge(0, 1);
        g.AddEdge(1, 2);
        g.AddEdge(0, 3);
        g.AddEdge(1, 3);
        g.AddEdge(2, 3);
        return g;
    }

    private static Graph BuildWeightedGraph()
    {
        // classic small Dijkstra example
        var g = new Graph(directed: false, weighted: true);
        for (int i = 0; i < 5; i++) g.AddNode(i);
        g.AddEdge(0, 1, 2);
        g.AddEdge(0, 2, 5);
        g.AddEdge(1, 2, 1);
        g.AddEdge(1, 3, 3);
        g.AddEdge(2, 3, 2);
        g.AddEdge(3, 4, 1);
        g.AddEdge(2, 4, 5);
        return g;
    }

    [Fact]
    public void Bfs_VisitOrderIsLevelByLevel()
    {
        var g = BuildSampleUndirected();
        var trace = BreadthFirstSearch.Run(g, 0);
        trace.Result.Order[0].Should().Be(0);
        trace.Result.Distance[0].Should().Be(0);
        trace.Result.Distance[1].Should().Be(1);
        trace.Result.Distance[2].Should().Be(2);
        trace.Result.Distance[3].Should().Be(1);
    }

    [Fact]
    public void Bfs_RecordsAtLeastOneStepPerNode()
    {
        var g = BuildSampleUndirected();
        var trace = BreadthFirstSearch.Run(g, 0);
        trace.Steps.Count.Should().BeGreaterThan(g.NodeCount);
    }

    [Fact]
    public void Bfs_OnUnknownStartNode_Throws()
    {
        var g = BuildSampleUndirected();
        Assert.Throws<ArgumentException>(() => BreadthFirstSearch.Run(g, 99));
    }

    [Fact]
    public void Dfs_VisitsEveryReachableNode()
    {
        var g = BuildSampleUndirected();
        var trace = DepthFirstSearch.Run(g, 0);
        trace.Result.Order.Should().BeEquivalentTo(new[] { 0, 1, 2, 3 });
    }

    [Fact]
    public void Dfs_StartsAtTheGivenNode()
    {
        var g = BuildSampleUndirected();
        var trace = DepthFirstSearch.Run(g, 2);
        trace.Result.Order[0].Should().Be(2);
    }

    [Fact]
    public void Dijkstra_FindsCorrectShortestDistances()
    {
        var g = BuildWeightedGraph();
        var trace = Dijkstra.Run(g, 0);
        trace.Result.Distance[0].Should().Be(0);
        trace.Result.Distance[1].Should().Be(2);
        trace.Result.Distance[2].Should().Be(3);   // 0->1->2
        trace.Result.Distance[3].Should().Be(5);   // 0->1->2->3
        trace.Result.Distance[4].Should().Be(6);   // 0->1->2->3->4
    }

    [Fact]
    public void Dijkstra_ReconstructsPath()
    {
        var g = BuildWeightedGraph();
        var trace = Dijkstra.Run(g, 0);
        var path = trace.Result.ReconstructPath(4);

        // There are two valid shortest paths (each of total weight 6):
        //   0 -> 1 -> 2 -> 3 -> 4   (2 + 1 + 2 + 1)
        //   0 -> 1 -> 3 -> 4        (2 + 3 + 1)
        // Verify that the returned path starts at 0, ends at 4, and has total weight 6.
        path[0].Should().Be(0);
        path[^1].Should().Be(4);

        double totalWeight = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var edge = g.Neighbors(path[i]).First(e => e.To == path[i + 1]);
            totalWeight += edge.Weight;
        }
        totalWeight.Should().Be(6);
    }

    [Fact]
    public void Dijkstra_RejectsNegativeWeights()
    {
        var g = new Graph(directed: false, weighted: true);
        g.AddNode(0); g.AddNode(1);
        g.AddEdge(0, 1, -1);
        Assert.Throws<InvalidOperationException>(() => Dijkstra.Run(g, 0));
    }

    [Fact]
    public void Kruskal_FindsCorrectMstWeight()
    {
        var g = BuildWeightedGraph();
        var trace = Kruskal.Run(g);
        trace.Result.IsSpanning.Should().BeTrue();
        // MST edges: (1,2)=1, (3,4)=1, (0,1)=2, (2,3)=2  ⇒  total 6.
        trace.Result.TotalWeight.Should().Be(6);
    }

    [Fact]
    public void Kruskal_TotalEdgeCountIsNMinusOne_OnConnectedGraph()
    {
        var g = BuildWeightedGraph();
        var trace = Kruskal.Run(g);
        trace.Result.Edges.Count.Should().Be(g.NodeCount - 1);
    }

    [Fact]
    public void Prim_AndKruskal_ProduceSameWeight()
    {
        var g = BuildWeightedGraph();
        var k = Kruskal.Run(g).Result;
        var p = Prim.Run(g).Result;
        p.TotalWeight.Should().Be(k.TotalWeight);
        p.IsSpanning.Should().BeTrue();
    }

    [Fact]
    public void Graph_AdjacencyMatrix_ZerosDiagonalAndInfWhereNoEdge()
    {
        var g = BuildSampleUndirected();
        var m = g.ToAdjacencyMatrix(out var ids);
        for (int i = 0; i < ids.Length; i++)
            m[i, i].Should().Be(0);
        // there's no direct edge 0-2 in this graph
        int idx0 = Array.IndexOf(ids, 0);
        int idx2 = Array.IndexOf(ids, 2);
        m[idx0, idx2].Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void Graph_RemoveNode_RemovesIncidentEdges()
    {
        var g = BuildSampleUndirected();
        int before = g.EdgeCount;
        g.RemoveNode(3);
        g.NodeCount.Should().Be(3);
        g.EdgeCount.Should().BeLessThan(before);
    }
}
