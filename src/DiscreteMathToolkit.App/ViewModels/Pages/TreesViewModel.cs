using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.Algorithms;
using DiscreteMathToolkit.Core.Trees;
using DiscreteMathToolkit.Infrastructure.Export;
using DiscreteMathToolkit.Infrastructure.Logging;
using DiscreteMathToolkit.Visualization.Layout;
using Microsoft.Win32;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed partial class TreesViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;
    private readonly IExportService _exporter;
    private readonly TreeLayoutEngine _layoutEngine = new();
    private readonly DispatcherTimer _playTimer;

    private BinaryTree _tree = new();
    private TreeLayout? _layout;
    private AlgorithmTrace<TraversalState, IReadOnlyList<int>>? _trace;
    private int _stepIndex;
    private double _availableWidth = 800;

    public string Title => "Trees";

    [ObservableProperty] private TreeRenderState _renderState = TreeRenderState.Empty;
    [ObservableProperty] private string _explanation = "Insert values into a BST and choose a traversal.";
    [ObservableProperty] private string _statusLine = "Ready.";
    [ObservableProperty] private string _resultSummary = string.Empty;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private int _stepDisplayIndex;
    [ObservableProperty] private int _totalSteps;
    [ObservableProperty] private double _playbackSpeed = 1.0;

    [ObservableProperty] private string _bulkInsertText = "4 2 6 1 3 5 7";
    [ObservableProperty] private string _singleInsertValue = string.Empty;
    [ObservableProperty] private string _searchOrDeleteValue = string.Empty;
    [ObservableProperty] private TraversalKind _selectedTraversal = TraversalKind.InOrder;

    [ObservableProperty] private string _reconstructPreOrder = "4 2 1 3 6 5 7";
    [ObservableProperty] private string _reconstructInOrder = "1 2 3 4 5 6 7";
    [ObservableProperty] private string _reconstructPostOrder = "1 3 2 5 7 6 4";

    public ObservableCollection<TraversalKind> Traversals { get; } = new(Enum.GetValues<TraversalKind>());

    public IRelayCommand BuildBstCommand { get; }
    public IRelayCommand InsertOneCommand { get; }
    public IRelayCommand SearchCommand { get; }
    public IRelayCommand DeleteCommand { get; }
    public IRelayCommand ClearTreeCommand { get; }
    public IRelayCommand RunTraversalCommand { get; }
    public IRelayCommand StepNextCommand { get; }
    public IRelayCommand StepPreviousCommand { get; }
    public IRelayCommand TogglePlayCommand { get; }
    public IRelayCommand ResetPlaybackCommand { get; }
    public IRelayCommand ReconstructFromPreInCommand { get; }
    public IRelayCommand ReconstructFromPostInCommand { get; }
    public IRelayCommand ExportTraversalCommand { get; }

    public TreesViewModel(IAppLogger logger, IExportService exporter)
    {
        _logger = logger;
        _exporter = exporter;
        _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        _playTimer.Tick += OnPlayTick;

        BuildBstCommand = new RelayCommand(BuildBst);
        InsertOneCommand = new RelayCommand(InsertOne);
        SearchCommand = new RelayCommand(SearchValue);
        DeleteCommand = new RelayCommand(DeleteValue);
        ClearTreeCommand = new RelayCommand(ClearTree);
        RunTraversalCommand = new RelayCommand(RunTraversal);
        StepNextCommand = new RelayCommand(StepNext, () => _trace != null);
        StepPreviousCommand = new RelayCommand(StepPrevious, () => _trace != null);
        TogglePlayCommand = new RelayCommand(TogglePlay, () => _trace != null);
        ResetPlaybackCommand = new RelayCommand(ResetPlayback, () => _trace != null);
        ReconstructFromPreInCommand = new RelayCommand(ReconstructFromPreIn);
        ReconstructFromPostInCommand = new RelayCommand(ReconstructFromPostIn);
        ExportTraversalCommand = new RelayCommand(ExportTraversal, () => _trace != null);

        BuildBst(); // pre-populate
    }

    public void OnCanvasSizeChanged(double width, double _)
    {
        if (width <= 0) return;
        _availableWidth = width;
        Relayout();
    }

    private void BuildBst()
    {
        try
        {
            var values = ParseInts(BulkInsertText);
            _tree = BinaryTree.BuildBst(values);
            DiscardPlayback();
            Relayout();
            StatusLine = $"Built BST with {_tree.Count} node(s); height {_tree.Height}.";
        }
        catch (Exception ex)
        {
            StatusLine = "Could not parse insert sequence.";
            _logger.Warn($"BuildBst failed: {ex.Message}");
        }
    }

    private void InsertOne()
    {
        if (!int.TryParse(SingleInsertValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            StatusLine = "Insert value must be an integer.";
            return;
        }
        bool added = _tree.Insert(v);
        SingleInsertValue = string.Empty;
        DiscardPlayback();
        Relayout();
        StatusLine = added ? $"Inserted {v}." : $"{v} already exists in the tree.";
    }

    private void SearchValue()
    {
        if (!int.TryParse(SearchOrDeleteValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            StatusLine = "Search value must be an integer.";
            return;
        }
        StatusLine = _tree.Search(v) ? $"Found {v}." : $"{v} not found.";
    }

    private void DeleteValue()
    {
        if (!int.TryParse(SearchOrDeleteValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            StatusLine = "Delete value must be an integer.";
            return;
        }
        bool removed = _tree.Delete(v);
        DiscardPlayback();
        Relayout();
        StatusLine = removed ? $"Deleted {v}." : $"{v} not in tree.";
    }

    private void ClearTree()
    {
        _tree = new BinaryTree();
        DiscardPlayback();
        Relayout();
        StatusLine = "Tree cleared.";
    }

    private void RunTraversal()
    {
        if (_tree.Root == null)
        {
            StatusLine = "Tree is empty.";
            return;
        }
        DiscardPlayback();
        _trace = TreeTraversals.Traverse(_tree, SelectedTraversal);
        TotalSteps = _trace.Steps.Count;
        _stepIndex = 0;
        ApplyStep();
        ResultSummary = $"{SelectedTraversal} sequence: {string.Join(", ", _trace.Result)}";
        StatusLine = $"Loaded {TotalSteps} step(s) for {SelectedTraversal}.";
        NotifyPlaybackCommands();
    }

    private void StepNext()
    {
        if (_trace == null || _stepIndex >= _trace.Steps.Count - 1) return;
        _stepIndex++;
        ApplyStep();
    }

    private void StepPrevious()
    {
        if (_trace == null || _stepIndex == 0) return;
        _stepIndex--;
        ApplyStep();
    }

    private void TogglePlay()
    {
        if (_trace == null) return;
        if (IsPlaying) { _playTimer.Stop(); IsPlaying = false; }
        else
        {
            if (_stepIndex >= _trace.Steps.Count - 1) { _stepIndex = 0; ApplyStep(); }
            _playTimer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(700.0 / Math.Max(0.1, PlaybackSpeed), 50, 5000));
            _playTimer.Start();
            IsPlaying = true;
        }
    }

    partial void OnPlaybackSpeedChanged(double value)
    {
        if (_playTimer.IsEnabled)
            _playTimer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(700.0 / Math.Max(0.1, value), 50, 5000));
    }

    private void OnPlayTick(object? sender, EventArgs e)
    {
        if (_trace == null) return;
        if (_stepIndex >= _trace.Steps.Count - 1)
        {
            _playTimer.Stop();
            IsPlaying = false;
            return;
        }
        _stepIndex++;
        ApplyStep();
    }

    private void ResetPlayback()
    {
        _playTimer.Stop();
        IsPlaying = false;
        if (_trace != null) { _stepIndex = 0; ApplyStep(); }
    }

    private void DiscardPlayback()
    {
        _playTimer.Stop();
        IsPlaying = false;
        _trace = null;
        TotalSteps = 0;
        _stepIndex = 0;
        StepDisplayIndex = 0;
        Explanation = "Insert values into a BST and choose a traversal.";
        NotifyPlaybackCommands();
    }

    private void NotifyPlaybackCommands()
    {
        StepNextCommand.NotifyCanExecuteChanged();
        StepPreviousCommand.NotifyCanExecuteChanged();
        TogglePlayCommand.NotifyCanExecuteChanged();
        ResetPlaybackCommand.NotifyCanExecuteChanged();
        ExportTraversalCommand.NotifyCanExecuteChanged();
    }

    private void ApplyStep()
    {
        if (_trace == null) return;
        var step = _trace.Steps[_stepIndex];
        StepDisplayIndex = step.Index + 1;
        Explanation = step.Description;

        var visitedSet = new HashSet<int>(step.State.Visited);
        int? current = step.State.Current;

        var nodes = new List<RenderableTreeNode>();
        var edges = new List<RenderableTreeEdge>();

        if (_layout?.Root != null)
        {
            foreach (var n in _layout.Root.Flatten())
            {
                TreeNodeBadge badge = n.Node.Value == current ? TreeNodeBadge.Current
                                    : visitedSet.Contains(n.Node.Value) ? TreeNodeBadge.Visited
                                    : TreeNodeBadge.None;
                string? annotation = null;
                int idx = step.State.Visited.ToList().IndexOf(n.Node.Value);
                if (idx >= 0) annotation = $"#{idx + 1}";
                nodes.Add(new RenderableTreeNode(n.Node.Value, n.Position, badge, annotation));
                if (n.Left != null) edges.Add(new RenderableTreeEdge(n.Position, n.Left.Position));
                if (n.Right != null) edges.Add(new RenderableTreeEdge(n.Position, n.Right.Position));
            }
        }
        RenderState = new TreeRenderState(nodes, edges, _layout?.Width ?? 0, _layout?.Height ?? 0);
    }

    private void Relayout()
    {
        _layout = _layoutEngine.Compute(_tree, _availableWidth);
        var nodes = new List<RenderableTreeNode>();
        var edges = new List<RenderableTreeEdge>();
        if (_layout?.Root != null)
        {
            foreach (var n in _layout.Root.Flatten())
            {
                nodes.Add(new RenderableTreeNode(n.Node.Value, n.Position, TreeNodeBadge.None, null));
                if (n.Left != null) edges.Add(new RenderableTreeEdge(n.Position, n.Left.Position));
                if (n.Right != null) edges.Add(new RenderableTreeEdge(n.Position, n.Right.Position));
            }
        }
        RenderState = new TreeRenderState(nodes, edges, _layout?.Width ?? 0, _layout?.Height ?? 0);
    }

    private void ReconstructFromPreIn()
    {
        try
        {
            var pre = ParseInts(ReconstructPreOrder);
            var inO = ParseInts(ReconstructInOrder);
            _tree = TreeReconstruction.FromPreOrderAndInOrder(pre, inO);
            DiscardPlayback();
            Relayout();
            StatusLine = $"Reconstructed tree from Pre+In ({_tree.Count} nodes, height {_tree.Height}).";
        }
        catch (Exception ex)
        {
            StatusLine = $"Reconstruction failed: {ex.Message}";
            _logger.Warn($"Pre+In reconstruction failed: {ex.Message}");
        }
    }

    private void ReconstructFromPostIn()
    {
        try
        {
            var post = ParseInts(ReconstructPostOrder);
            var inO = ParseInts(ReconstructInOrder);
            _tree = TreeReconstruction.FromPostOrderAndInOrder(post, inO);
            DiscardPlayback();
            Relayout();
            StatusLine = $"Reconstructed tree from Post+In ({_tree.Count} nodes, height {_tree.Height}).";
        }
        catch (Exception ex)
        {
            StatusLine = $"Reconstruction failed: {ex.Message}";
            _logger.Warn($"Post+In reconstruction failed: {ex.Message}");
        }
    }

    private void ExportTraversal()
    {
        if (_trace == null) return;
        var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Markdown (*.md)|*.md|HTML (*.html)|*.html",
            FileName = $"{SelectedTraversal.ToString().ToLowerInvariant()}-traversal"
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
            var rows = new List<IReadOnlyList<string>>();
            for (int i = 0; i < _trace.Result.Count; i++)
                rows.Add(new[] { (i + 1).ToString(), _trace.Result[i].ToString() });
            var data = new TabularData($"{SelectedTraversal} Traversal",
                new[] { "Order", "Value" }, rows,
                new[] { $"Tree size: {_tree.Count}; height: {_tree.Height}" });
            _exporter.Export(dlg.FileName, data, format);
            StatusLine = $"Exported traversal to {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Export failed: {ex.Message}";
            _logger.Error("Export traversal failed", ex);
        }
    }

    private static IReadOnlyList<int> ParseInts(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<int>();
        var parts = text.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<int>(parts.Length);
        foreach (var p in parts)
        {
            if (!int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                throw new FormatException($"'{p}' is not an integer.");
            result.Add(v);
        }
        return result;
    }
}
