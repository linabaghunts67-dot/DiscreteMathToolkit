using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.Algorithms;
using DiscreteMathToolkit.Core.Graphs;
using DiscreteMathToolkit.Core.Graphs.Algorithms;
using DiscreteMathToolkit.Infrastructure.Export;
using DiscreteMathToolkit.Infrastructure.Logging;
using DiscreteMathToolkit.Infrastructure.Persistence;
using DiscreteMathToolkit.Visualization.Layout;
using DiscreteMathToolkit.Visualization.Playback;
using Microsoft.Win32;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public enum GraphAlgorithmKind { Bfs, Dfs, Dijkstra, Kruskal, Prim }

public sealed partial class GraphTheoryViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;
    private readonly IGraphRepository _graphRepo;
    private readonly IExportService _exporter;
    private readonly IGraphLayoutEngine _layoutEngine;
    private readonly DispatcherTimer _playTimer;

    // Holds positions across algorithm runs — independent from the algorithm trace state
    private Dictionary<int, Point2D> _nodePositions = new();
    private double _canvasWidth = 800;
    private double _canvasHeight = 540;

    // Active algorithm playback state (one of these is non-null at a time)
    private IPlaybackAdapter? _adapter;

    public string Title => "Graph Theory";

    [ObservableProperty] private Graph _graph = new(directed: false, weighted: true);
    [ObservableProperty] private GraphRenderState _renderState = GraphRenderState.Empty;
    [ObservableProperty] private string _explanation = "Build a graph in the editor on the right, choose an algorithm, then press Run.";
    [ObservableProperty] private string _statusLine = "Ready.";
    [ObservableProperty] private string _resultSummary = string.Empty;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private int _stepIndex;
    [ObservableProperty] private int _totalSteps;
    [ObservableProperty] private double _playbackSpeed = 1.0;

    // Algorithm selection
    public ObservableCollection<GraphAlgorithmKind> Algorithms { get; } =
        new(Enum.GetValues<GraphAlgorithmKind>());

    [ObservableProperty] private GraphAlgorithmKind _selectedAlgorithm = GraphAlgorithmKind.Bfs;
    [ObservableProperty] private string _startNodeText = "0";

    // Graph editing
    [ObservableProperty] private bool _isDirected;
    [ObservableProperty] private bool _isWeighted;
    [ObservableProperty] private string _newNodeLabel = string.Empty;
    [ObservableProperty] private string _newEdgeFrom = string.Empty;
    [ObservableProperty] private string _newEdgeTo = string.Empty;
    [ObservableProperty] private string _newEdgeWeight = "1";

    public IRelayCommand AddNodeCommand { get; }
    public IRelayCommand AddEdgeCommand { get; }
    public IRelayCommand ClearGraphCommand { get; }
    public IRelayCommand LoadSampleCommand { get; }
    public IRelayCommand RunAlgorithmCommand { get; }
    public IRelayCommand StepNextCommand { get; }
    public IRelayCommand StepPreviousCommand { get; }
    public IRelayCommand TogglePlayCommand { get; }
    public IRelayCommand ResetPlaybackCommand { get; }
    public IRelayCommand SaveGraphCommand { get; }
    public IRelayCommand LoadGraphCommand { get; }
    public IRelayCommand ExportResultCommand { get; }

    public GraphTheoryViewModel(
        IAppLogger logger,
        IGraphRepository graphRepo,
        IExportService exporter)
    {
        _logger = logger;
        _graphRepo = graphRepo;
        _exporter = exporter;
        _layoutEngine = new ForceDirectedLayoutEngine();
        _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        _playTimer.Tick += OnPlayTimerTick;

        _graph = BuildSampleGraph();
        LayoutGraph();
        RebuildRenderState();

        AddNodeCommand = new RelayCommand(AddNode);
        AddEdgeCommand = new RelayCommand(AddEdge);
        ClearGraphCommand = new RelayCommand(ClearGraph);
        LoadSampleCommand = new RelayCommand(LoadSampleGraph);
        RunAlgorithmCommand = new RelayCommand(RunAlgorithm);
        StepNextCommand = new RelayCommand(() => _adapter?.Next(), () => _adapter != null);
        StepPreviousCommand = new RelayCommand(() => _adapter?.Previous(), () => _adapter != null);
        TogglePlayCommand = new RelayCommand(TogglePlay, () => _adapter != null);
        ResetPlaybackCommand = new RelayCommand(ResetPlayback, () => _adapter != null);
        SaveGraphCommand = new RelayCommand(SaveGraph);
        LoadGraphCommand = new RelayCommand(LoadGraph);
        ExportResultCommand = new RelayCommand(ExportResult, () => _adapter != null);
    }

    // ============================================================
    // Graph editing
    // ============================================================
    private static Graph BuildSampleGraph()
    {
        // pleasant 5-node weighted demo
        var g = new Graph(directed: false, weighted: true);
        for (int i = 0; i < 5; i++) g.AddNode(i, ((char)('A' + i)).ToString());
        g.AddEdge(0, 1, 2);
        g.AddEdge(0, 2, 5);
        g.AddEdge(1, 2, 1);
        g.AddEdge(1, 3, 3);
        g.AddEdge(2, 3, 2);
        g.AddEdge(3, 4, 1);
        g.AddEdge(2, 4, 5);
        return g;
    }

    partial void OnIsDirectedChanged(bool value)
    {
        if (value == Graph.IsDirected) return;
        Graph = RebuildGraphWithFlags(value, IsWeighted);
        LayoutGraph();
        RebuildRenderState();
    }

    partial void OnIsWeightedChanged(bool value)
    {
        if (value == Graph.IsWeighted) return;
        Graph = RebuildGraphWithFlags(IsDirected, value);
        LayoutGraph();
        RebuildRenderState();
    }

    private Graph RebuildGraphWithFlags(bool directed, bool weighted)
    {
        var ng = new Graph(directed, weighted);
        foreach (var n in Graph.Nodes.Values) ng.AddNode(n.Id, n.Label);
        foreach (var e in Graph.Edges()) ng.AddEdge(e.From, e.To, e.Weight);
        return ng;
    }

    private void AddNode()
    {
        try
        {
            int nextId = Graph.Nodes.Count == 0 ? 0 : Graph.Nodes.Keys.Max() + 1;
            string label = string.IsNullOrWhiteSpace(NewNodeLabel) ? nextId.ToString(CultureInfo.InvariantCulture) : NewNodeLabel.Trim();
            Graph.AddNode(nextId, label);
            NewNodeLabel = string.Empty;
            DiscardPlayback();
            LayoutGraph();
            RebuildRenderState();
            StatusLine = $"Added node '{label}' (id {nextId}).";
        }
        catch (Exception ex)
        {
            StatusLine = $"Error: {ex.Message}";
            _logger.Warn($"AddNode failed: {ex.Message}");
        }
    }

    private void AddEdge()
    {
        try
        {
            int from = ResolveId(NewEdgeFrom);
            int to = ResolveId(NewEdgeTo);
            double weight = 1.0;
            if (IsWeighted)
            {
                if (!double.TryParse(NewEdgeWeight, NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
                    throw new FormatException("Edge weight must be a number.");
            }
            Graph.AddEdge(from, to, weight);
            DiscardPlayback();
            RebuildRenderState();
            StatusLine = $"Added edge {Graph.Nodes[from].Label} → {Graph.Nodes[to].Label}{(IsWeighted ? $" (w={weight})" : "")}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Error: {ex.Message}";
            _logger.Warn($"AddEdge failed: {ex.Message}");
        }
    }

    private int ResolveId(string text)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && Graph.Nodes.ContainsKey(id))
            return id;
        // try by label
        var match = Graph.Nodes.Values.FirstOrDefault(n => string.Equals(n.Label, text, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match.Id;
        throw new FormatException($"Node '{text}' not found.");
    }

    private void ClearGraph()
    {
        Graph = new Graph(IsDirected, IsWeighted);
        DiscardPlayback();
        LayoutGraph();
        RebuildRenderState();
        StatusLine = "Graph cleared.";
    }

    private void LoadSampleGraph()
    {
        Graph = BuildSampleGraph();
        IsDirected = false;
        IsWeighted = true;
        DiscardPlayback();
        LayoutGraph();
        RebuildRenderState();
        StatusLine = "Loaded sample weighted graph (A–E, 7 edges).";
    }

    private void LayoutGraph()
    {
        var layout = _layoutEngine.Compute(Graph, _canvasWidth, _canvasHeight);
        _nodePositions = new Dictionary<int, Point2D>(layout.NodePositions);
    }

    /// <summary>
    /// Called by the View when the canvas is resized so layout can be recomputed.
    /// </summary>
    public void OnCanvasSizeChanged(double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        _canvasWidth = width;
        _canvasHeight = height;
        if (Graph.Nodes.Count > 0)
        {
            LayoutGraph();
            RebuildRenderState();
        }
    }

    // ============================================================
    // Algorithm execution
    // ============================================================
    private void RunAlgorithm()
    {
        DiscardPlayback();
        ResultSummary = string.Empty;
        try
        {
            switch (SelectedAlgorithm)
            {
                case GraphAlgorithmKind.Bfs:
                {
                    int start = ResolveId(StartNodeText);
                    var trace = BreadthFirstSearch.Run(Graph, start);
                    _adapter = new BfsAdapter(this, trace);
                    ResultSummary = $"BFS from {Graph.Nodes[start].Label}: visited {trace.Result.Order.Count} node(s); furthest distance = {(trace.Result.Distance.Count == 0 ? 0 : trace.Result.Distance.Values.Max())}.";
                    break;
                }
                case GraphAlgorithmKind.Dfs:
                {
                    int start = ResolveId(StartNodeText);
                    var trace = DepthFirstSearch.Run(Graph, start);
                    _adapter = new DfsAdapter(this, trace);
                    ResultSummary = $"DFS from {Graph.Nodes[start].Label}: visited {trace.Result.Order.Count} node(s) in order [{string.Join(", ", trace.Result.Order.Select(id => Graph.Nodes[id].Label))}].";
                    break;
                }
                case GraphAlgorithmKind.Dijkstra:
                {
                    int start = ResolveId(StartNodeText);
                    var trace = Dijkstra.Run(Graph, start);
                    _adapter = new DijkstraAdapter(this, trace);
                    int reachable = trace.Result.Distance.Values.Count(d => !double.IsPositiveInfinity(d));
                    ResultSummary = $"Dijkstra from {Graph.Nodes[start].Label}: shortest paths to {reachable} node(s) computed.";
                    break;
                }
                case GraphAlgorithmKind.Kruskal:
                {
                    var trace = Kruskal.Run(Graph);
                    _adapter = new KruskalAdapter(this, trace);
                    ResultSummary = trace.Result.IsSpanning
                        ? $"Kruskal: MST has {trace.Result.Edges.Count} edges, total weight {trace.Result.TotalWeight:0.##}."
                        : $"Kruskal: graph is disconnected; spanning forest has {trace.Result.Edges.Count} edges, weight {trace.Result.TotalWeight:0.##}.";
                    break;
                }
                case GraphAlgorithmKind.Prim:
                {
                    int? start = int.TryParse(StartNodeText, out var s) && Graph.Nodes.ContainsKey(s) ? s : (int?)null;
                    var trace = Prim.Run(Graph, start);
                    _adapter = new PrimAdapter(this, trace);
                    ResultSummary = trace.Result.IsSpanning
                        ? $"Prim: MST has {trace.Result.Edges.Count} edges, total weight {trace.Result.TotalWeight:0.##}."
                        : $"Prim: stopped early ({trace.Result.Edges.Count} edge(s)); graph not fully connected.";
                    break;
                }
            }

            TotalSteps = _adapter!.TotalSteps;
            _adapter.Reset();
            StatusLine = $"Loaded {TotalSteps} step(s) for {SelectedAlgorithm}. Use Next or Play to step through.";
            _logger.Info($"Ran {SelectedAlgorithm} on graph with {Graph.NodeCount} nodes.");
            NotifyPlaybackCommands();
        }
        catch (Exception ex)
        {
            StatusLine = $"Error: {ex.Message}";
            _logger.Warn($"{SelectedAlgorithm} failed: {ex.Message}");
        }
    }

    // ============================================================
    // Playback
    // ============================================================
    private void OnPlayTimerTick(object? sender, EventArgs e)
    {
        if (_adapter == null) return;
        if (!_adapter.Next())
        {
            _playTimer.Stop();
            IsPlaying = false;
        }
    }

    private void TogglePlay()
    {
        if (_adapter == null) return;
        if (IsPlaying) { _playTimer.Stop(); IsPlaying = false; }
        else
        {
            if (_adapter.AtEnd) _adapter.Reset();
            _playTimer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(700.0 / Math.Max(0.1, PlaybackSpeed), 50, 5000));
            _playTimer.Start();
            IsPlaying = true;
        }
    }

    partial void OnPlaybackSpeedChanged(double value)
    {
        if (_playTimer.IsEnabled)
        {
            _playTimer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(700.0 / Math.Max(0.1, value), 50, 5000));
        }
    }

    private void ResetPlayback()
    {
        _playTimer.Stop();
        IsPlaying = false;
        _adapter?.Reset();
    }

    private void DiscardPlayback()
    {
        _playTimer.Stop();
        IsPlaying = false;
        _adapter = null;
        TotalSteps = 0;
        StepIndex = 0;
        Explanation = "Build a graph in the editor on the right, choose an algorithm, then press Run.";
        NotifyPlaybackCommands();
    }

    private void NotifyPlaybackCommands()
    {
        StepNextCommand.NotifyCanExecuteChanged();
        StepPreviousCommand.NotifyCanExecuteChanged();
        TogglePlayCommand.NotifyCanExecuteChanged();
        ResetPlaybackCommand.NotifyCanExecuteChanged();
        ExportResultCommand.NotifyCanExecuteChanged();
    }

    // ============================================================
    // Persistence and export
    // ============================================================
    private void SaveGraph()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Graph JSON (*.json)|*.json",
            FileName = "graph.json"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var doc = _graphRepo.FromGraph(Graph, _nodePositions.ToDictionary(p => p.Key, p => (p.Value.X, p.Value.Y)));
            _graphRepo.Save(dlg.FileName, doc);
            StatusLine = $"Saved graph to {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Save failed: {ex.Message}";
            _logger.Error("Save graph failed", ex);
        }
    }

    private void LoadGraph()
    {
        var dlg = new OpenFileDialog { Filter = "Graph JSON (*.json)|*.json" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var doc = _graphRepo.Load(dlg.FileName);
            Graph = _graphRepo.ToGraph(doc);
            IsDirected = doc.Directed;
            IsWeighted = doc.Weighted;
            _nodePositions = doc.Nodes.ToDictionary(n => n.Id, n => new Point2D(n.X, n.Y));
            // if all positions are at the origin, run layout
            if (_nodePositions.Values.All(p => p.X == 0 && p.Y == 0)) LayoutGraph();
            DiscardPlayback();
            RebuildRenderState();
            StatusLine = $"Loaded graph from {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Load failed: {ex.Message}";
            _logger.Error("Load graph failed", ex);
        }
    }

    private void ExportResult()
    {
        if (_adapter == null) return;
        var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Markdown (*.md)|*.md|HTML (*.html)|*.html",
            FileName = $"{SelectedAlgorithm.ToString().ToLowerInvariant()}-result"
        };
        if (dlg.ShowDialog() != true) return;
        var format = dlg.FilterIndex switch
        {
            2 => ExportFormat.Markdown,
            3 => ExportFormat.Html,
            _ => ExportFormat.Csv
        };
        try
        {
            var data = _adapter.BuildExport();
            _exporter.Export(dlg.FileName, data, format);
            StatusLine = $"Exported result to {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Export failed: {ex.Message}";
            _logger.Error("Export failed", ex);
        }
    }

    // ============================================================
    // Render-state assembly
    // ============================================================
    private void RebuildRenderState()
    {
        var nodes = new List<RenderableNode>();
        foreach (var n in Graph.Nodes.Values)
        {
            if (!_nodePositions.TryGetValue(n.Id, out var pos))
                pos = new Point2D(_canvasWidth / 2, _canvasHeight / 2);
            nodes.Add(new RenderableNode(n.Id, n.Label, pos, NodeBadge.None, null));
        }
        var edges = new List<RenderableEdge>();
        foreach (var e in Graph.Edges())
        {
            edges.Add(new RenderableEdge(
                e.From, e.To,
                IsWeighted ? e.Weight.ToString("0.##", CultureInfo.InvariantCulture) : null,
                EdgeBadge.None,
                IsDirected));
        }
        RenderState = new GraphRenderState(nodes, edges);
    }

    /// <summary>
    /// Called by an adapter on every step change so the view rerenders the canvas.
    /// </summary>
    internal void ApplyStepRender(
        int stepIndex,
        string description,
        IReadOnlyDictionary<int, NodeBadge> nodeBadges,
        IReadOnlyDictionary<int, string?> nodeAnnotations,
        IReadOnlyDictionary<(int, int), EdgeBadge> edgeBadges)
    {
        StepIndex = stepIndex;
        Explanation = description;

        var nodes = new List<RenderableNode>(Graph.NodeCount);
        foreach (var n in Graph.Nodes.Values)
        {
            var pos = _nodePositions.TryGetValue(n.Id, out var p) ? p : new Point2D(_canvasWidth / 2, _canvasHeight / 2);
            var badge = nodeBadges.TryGetValue(n.Id, out var b) ? b : NodeBadge.None;
            var ann = nodeAnnotations.TryGetValue(n.Id, out var a) ? a : null;
            nodes.Add(new RenderableNode(n.Id, n.Label, pos, badge, ann));
        }

        var edges = new List<RenderableEdge>();
        foreach (var e in Graph.Edges())
        {
            EdgeBadge b = EdgeBadge.None;
            if (edgeBadges.TryGetValue((e.From, e.To), out var found)) b = found;
            else if (!IsDirected && edgeBadges.TryGetValue((e.To, e.From), out var rfound)) b = rfound;
            edges.Add(new RenderableEdge(
                e.From, e.To,
                IsWeighted ? e.Weight.ToString("0.##", CultureInfo.InvariantCulture) : null,
                b,
                IsDirected));
        }
        RenderState = new GraphRenderState(nodes, edges);
    }

    // ============================================================
    // Adapter abstraction — wraps each algorithm's Step so the VM is uniform
    // ============================================================
    internal interface IPlaybackAdapter
    {
        int TotalSteps { get; }
        int Index { get; }
        bool AtStart { get; }
        bool AtEnd { get; }
        void Reset();
        bool Next();
        bool Previous();
        TabularData BuildExport();
    }
}
