using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.Combinatorics;
using DiscreteMathToolkit.Infrastructure.Export;
using DiscreteMathToolkit.Infrastructure.Logging;
using Microsoft.Win32;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed class PascalCell
{
    public int Row { get; }
    public int Column { get; }
    public string Value { get; }
    public bool IsEdge { get; }
    public PascalCell(int row, int col, BigInteger value, bool isEdge)
    {
        Row = row;
        Column = col;
        Value = value.ToString();
        IsEdge = isEdge;
    }
}

public sealed class PascalRow
{
    public IReadOnlyList<PascalCell> Cells { get; }
    public PascalRow(IReadOnlyList<PascalCell> cells) { Cells = cells; }
}

public sealed partial class CombinatoricsViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;
    private readonly IExportService _exporter;

    public string Title => "Combinatorics";

    [ObservableProperty] private string _nText = "10";
    [ObservableProperty] private string _kText = "3";
    [ObservableProperty] private string _factorialResult = string.Empty;
    [ObservableProperty] private string _permutationsResult = string.Empty;
    [ObservableProperty] private string _combinationsResult = string.Empty;
    [ObservableProperty] private string _variationsRepResult = string.Empty;
    [ObservableProperty] private string _combinationsRepResult = string.Empty;
    [ObservableProperty] private string _statusLine = "Ready.";
    [ObservableProperty] private int _pascalRowsCount = 8;

    public ObservableCollection<PascalRow> PascalRows { get; } = new();

    public IRelayCommand ComputeAllCommand { get; }
    public IRelayCommand BuildPascalCommand { get; }
    public IRelayCommand ExportPascalCommand { get; }

    public CombinatoricsViewModel(IAppLogger logger, IExportService exporter)
    {
        _logger = logger;
        _exporter = exporter;
        ComputeAllCommand = new RelayCommand(ComputeAll);
        BuildPascalCommand = new RelayCommand(BuildPascal);
        ExportPascalCommand = new RelayCommand(ExportPascal);

        ComputeAll();
        BuildPascal();
    }

    private void ComputeAll()
    {
        try
        {
            int n = int.Parse(NText, CultureInfo.InvariantCulture);
            int k = int.Parse(KText, CultureInfo.InvariantCulture);

            FactorialResult = TryCompute(() => CombinatoricsCalculator.Factorial(n));
            PermutationsResult = TryCompute(() => CombinatoricsCalculator.Permutations(n, k));
            CombinationsResult = TryCompute(() => CombinatoricsCalculator.Combinations(n, k));
            VariationsRepResult = TryCompute(() => CombinatoricsCalculator.VariationsWithRepetition(n, k));
            CombinationsRepResult = TryCompute(() => CombinatoricsCalculator.CombinationsWithRepetition(n, k));

            StatusLine = $"Computed for n={n}, k={k}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Input error: {ex.Message}";
            _logger.Warn($"Combinatorics input error: {ex.Message}");
        }
    }

    private static string TryCompute(Func<BigInteger> f)
    {
        try { return f().ToString(CultureInfo.InvariantCulture); }
        catch (Exception ex) { return $"⚠ {ex.Message}"; }
    }

    private void BuildPascal()
    {
        try
        {
            int rows = Math.Clamp(PascalRowsCount, 1, 16);
            var triangle = CombinatoricsCalculator.PascalTriangle(rows);
            PascalRows.Clear();
            for (int i = 0; i < triangle.Count; i++)
            {
                var cells = new List<PascalCell>(triangle[i].Count);
                for (int j = 0; j < triangle[i].Count; j++)
                {
                    bool edge = j == 0 || j == triangle[i].Count - 1;
                    cells.Add(new PascalCell(i, j, triangle[i][j], edge));
                }
                PascalRows.Add(new PascalRow(cells));
            }
            StatusLine = $"Built Pascal's triangle with {rows} rows.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Pascal triangle failed: {ex.Message}";
            _logger.Warn($"Pascal triangle failed: {ex.Message}");
        }
    }

    private void ExportPascal()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Markdown (*.md)|*.md|HTML (*.html)|*.html",
            FileName = "pascal-triangle"
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
            int maxCols = PascalRows.Count == 0 ? 0 : PascalRows[^1].Cells.Count;
            var headers = new List<string>(maxCols);
            for (int j = 0; j < maxCols; j++) headers.Add($"C{j}");

            var rows = new List<IReadOnlyList<string>>();
            foreach (var row in PascalRows)
            {
                var cells = new List<string>(maxCols);
                for (int j = 0; j < maxCols; j++)
                    cells.Add(j < row.Cells.Count ? row.Cells[j].Value : string.Empty);
                rows.Add(cells);
            }
            var data = new TabularData("Pascal's Triangle", headers, rows,
                new[] { $"Rows: {PascalRows.Count}" });
            _exporter.Export(dlg.FileName, data, format);
            StatusLine = $"Exported Pascal's triangle to {dlg.FileName}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Export failed: {ex.Message}";
            _logger.Error("Pascal export failed", ex);
        }
    }
}
