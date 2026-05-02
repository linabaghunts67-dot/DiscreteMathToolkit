using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.Logic;
using DiscreteMathToolkit.Infrastructure.Export;
using DiscreteMathToolkit.Infrastructure.Logging;
using Microsoft.Win32;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

/// <summary>
/// One row in the rendered truth table view-model.
/// Renamed to avoid conflict with Core.Logic.TruthTableRow.
/// </summary>
public sealed class TruthTableRowVm
{
    public IReadOnlyList<string> Cells { get; }
    public bool Result { get; }
    public TruthTableRowVm(IReadOnlyList<string> cells, bool result)
    {
        Cells = cells;
        Result = result;
    }
}

public sealed partial class LogicViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;
    private readonly IExportService _exporter;
    private TruthTable? _table;

    public string Title => "Logic & Truth Tables";

    [ObservableProperty] private string _expression = "(p AND q) OR (!p AND r)";
    [ObservableProperty] private string _statusLine = "Ready.";
    [ObservableProperty] private string _classification = string.Empty;
    [ObservableProperty] private string _simplifiedForm = string.Empty;
    [ObservableProperty] private string _parseTreePreview = string.Empty;

    public ObservableCollection<string> ColumnHeaders { get; } = new();
    public ObservableCollection<TruthTableRowVm> Rows { get; } = new();

    public ObservableCollection<string> SampleExpressions { get; } = new(new[]
    {
        "p OR !p",
        "p AND !p",
        "!(p AND q) <-> (!p OR !q)",
        "(p -> q) <-> (!p OR q)",
        "(p AND q) OR (q AND r) OR (p AND r)",
        "(p -> q) AND (q -> r) -> (p -> r)",
    });

    public IRelayCommand BuildTableCommand { get; }
    public IRelayCommand SimplifyCommand { get; }
    public IRelayCommand<string> LoadSampleCommand { get; }
    public IRelayCommand ExportTableCommand { get; }

    public LogicViewModel(IAppLogger logger, IExportService exporter)
    {
        _logger = logger;
        _exporter = exporter;
        BuildTableCommand = new RelayCommand(BuildTable);
        SimplifyCommand = new RelayCommand(Simplify);
        LoadSampleCommand = new RelayCommand<string>(s =>
        {
            if (!string.IsNullOrEmpty(s)) Expression = s;
        });
        ExportTableCommand = new RelayCommand(ExportTable, () => _table != null);

        // populate on startup
        BuildTable();
    }

    private void BuildTable()
    {
        try
        {
            var node = LogicParser.Parse(Expression);
            ParseTreePreview = NodePreview(node);
            _table = TruthTableBuilder.Build(node);

            Rows.Clear();
            ColumnHeaders.Clear();
            foreach (var v in _table.Variables) ColumnHeaders.Add(v);
            ColumnHeaders.Add("⊨");

            foreach (var row in _table.Rows)
            {
                var cells = new List<string>(_table.Variables.Count + 1);
                foreach (var v in _table.Variables)
                    cells.Add(row.Assignment[v] ? "1" : "0");
                cells.Add(row.Result ? "1" : "0");
                Rows.Add(new TruthTableRowVm(cells, row.Result));
            }

            int trueCount = _table.Rows.Count(r => r.Result);
            Classification = _table.Classification switch
            {
                LogicClassification.Tautology => "Tautology — the expression is always true.",
                LogicClassification.Contradiction => "Contradiction — the expression is always false.",
                _ => $"Contingency — true in {trueCount} of {_table.Rows.Count} rows."
            };
            StatusLine = $"Built truth table with {_table.Variables.Count} variable(s) and {_table.Rows.Count} row(s).";
            _logger.Info($"Built truth table for expression: {Expression}");
        }
        catch (Exception ex)
        {
            StatusLine = $"Parse error: {ex.Message}";
            Classification = string.Empty;
            ParseTreePreview = string.Empty;
            Rows.Clear();
            ColumnHeaders.Clear();
            _table = null;
            _logger.Warn($"Logic parse failed: {ex.Message}");
        }
        finally
        {
            ExportTableCommand.NotifyCanExecuteChanged();
        }
    }

    private void Simplify()
    {
        try
        {
            var node = LogicParser.Parse(Expression);
            var simplified = BooleanSimplifier.Simplify(node);
            SimplifiedForm = simplified.ToInfix();
            StatusLine = $"Simplified: {SimplifiedForm}";
        }
        catch (Exception ex)
        {
            StatusLine = $"Simplify failed: {ex.Message}";
            SimplifiedForm = string.Empty;
        }
    }

    private static string NodePreview(LogicNode node)
    {
        var sb = new StringBuilder();
        Walk(node, sb, 0);
        return sb.ToString();
    }

    private static void Walk(LogicNode n, StringBuilder sb, int depth)
    {
        sb.Append(' ', depth * 2);
        sb.AppendLine(n switch
        {
            VarNode v => $"VAR {v.Name}",
            ConstNode c => $"CONST {(c.Value ? "T" : "F")}",
            NotNode => "NOT",
            BinNode b => b.Op.ToString(),
            _ => "?"
        });
        switch (n)
        {
            case NotNode not: Walk(not.Inner, sb, depth + 1); break;
            case BinNode bin:
                Walk(bin.Left, sb, depth + 1);
                Walk(bin.Right, sb, depth + 1);
                break;
        }
    }

    private void ExportTable()
    {
        if (_table == null) return;
        var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Markdown (*.md)|*.md|HTML (*.html)|*.html",
            FileName = "truth-table"
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
            var headers = new List<string>(_table.Variables) { "Result" };
            var rows = new List<IReadOnlyList<string>>();
            foreach (var r in _table.Rows)
            {
                var cells = new List<string>();
                foreach (var v in _table.Variables) cells.Add(r.Assignment[v] ? "1" : "0");
                cells.Add(r.Result ? "1" : "0");
                rows.Add(cells);
            }
            var data = new TabularData(
                $"Truth table: {Expression}",
                headers, rows,
                new[] { Classification });
            _exporter.Export(dlg.FileName, data, format);
            StatusLine = $"Exported truth table to {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Export failed: {ex.Message}";
            _logger.Error("Truth table export failed", ex);
        }
    }
}
