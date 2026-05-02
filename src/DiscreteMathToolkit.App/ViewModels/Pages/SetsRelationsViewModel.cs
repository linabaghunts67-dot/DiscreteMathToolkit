using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.Sets;
using DiscreteMathToolkit.Infrastructure.Logging;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed class MatrixCell
{
    public bool Value { get; }
    public int Row { get; }
    public int Column { get; }
    public MatrixCell(int row, int col, bool value) { Row = row; Column = col; Value = value; }
}

public sealed class MatrixRow
{
    public string Header { get; }
    public IReadOnlyList<MatrixCell> Cells { get; }
    public MatrixRow(string header, IReadOnlyList<MatrixCell> cells) { Header = header; Cells = cells; }
}

public sealed class PropertyResult
{
    public string Label { get; }
    public bool Holds { get; }
    public string Display => Holds ? "yes" : "no";
    public PropertyResult(string label, bool holds) { Label = label; Holds = holds; }
}

public sealed partial class SetsRelationsViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;

    public string Title => "Sets & Relations";

    // Set operations
    [ObservableProperty] private string _setAText = "1, 2, 3, 4";
    [ObservableProperty] private string _setBText = "3, 4, 5, 6";
    [ObservableProperty] private string _universeText = "1, 2, 3, 4, 5, 6, 7, 8";
    [ObservableProperty] private string _unionResult = string.Empty;
    [ObservableProperty] private string _intersectionResult = string.Empty;
    [ObservableProperty] private string _differenceResult = string.Empty;
    [ObservableProperty] private string _symmetricDifferenceResult = string.Empty;
    [ObservableProperty] private string _complementAResult = string.Empty;
    [ObservableProperty] private string _cartesianProductResult = string.Empty;
    [ObservableProperty] private string _powerSetResult = string.Empty;

    // Relation analysis
    [ObservableProperty] private string _relationBaseText = "1, 2, 3";
    [ObservableProperty] private string _relationPairsText = "(1,1), (2,2), (3,3), (1,2), (2,1)";
    [ObservableProperty] private string _relationVerdict = string.Empty;
    [ObservableProperty] private string _statusLine = "Ready.";

    public ObservableCollection<PropertyResult> Properties { get; } = new();
    public ObservableCollection<string> RelationFailures { get; } = new();
    public ObservableCollection<string> MatrixHeader { get; } = new();
    public ObservableCollection<MatrixRow> MatrixRows { get; } = new();

    public IRelayCommand ComputeSetOpsCommand { get; }
    public IRelayCommand AnalyzeRelationCommand { get; }

    public SetsRelationsViewModel(IAppLogger logger)
    {
        _logger = logger;
        ComputeSetOpsCommand = new RelayCommand(ComputeSetOps);
        AnalyzeRelationCommand = new RelayCommand(AnalyzeRelation);

        ComputeSetOps();
        AnalyzeRelation();
    }

    private void ComputeSetOps()
    {
        try
        {
            var a = ParseInts(SetAText);
            var b = ParseInts(SetBText);
            var u = ParseInts(UniverseText);

            UnionResult = Format(SetOperations.Union(a, b));
            IntersectionResult = Format(SetOperations.Intersection(a, b));
            DifferenceResult = Format(SetOperations.Difference(a, b));
            SymmetricDifferenceResult = Format(SetOperations.SymmetricDifference(a, b));

            try { ComplementAResult = Format(SetOperations.Complement(a, u)); }
            catch (Exception ex) { ComplementAResult = $"⚠ {ex.Message}"; }

            var cart = SetOperations.CartesianProduct(a, b);
            CartesianProductResult = "{" + string.Join(", ", cart.Select(p => $"({p.A},{p.B})")) + "}";

            try
            {
                var ps = SetOperations.PowerSet(a);
                CombinatoricsCalculator_Note(ps.Count);
                PowerSetResult = "{" + string.Join(", ", ps.Select(s => "{" + string.Join(",", s) + "}")) + "}";
            }
            catch (Exception ex) { PowerSetResult = $"⚠ {ex.Message}"; }

            StatusLine = $"|A|={a.Count}, |B|={b.Count}, |U|={u.Count}.";
        }
        catch (Exception ex)
        {
            StatusLine = $"Set parse failed: {ex.Message}";
            _logger.Warn($"Set parse failed: {ex.Message}");
        }
    }

    // Suppress unused-variable warnings for very large powersets:
    private static void CombinatoricsCalculator_Note(int count) { _ = count; }

    private void AnalyzeRelation()
    {
        try
        {
            var baseSet = ParseInts(RelationBaseText);
            var pairs = ParsePairs(RelationPairsText);

            var props = RelationAnalyzer.Analyze(baseSet, pairs);
            Properties.Clear();
            Properties.Add(new PropertyResult("Reflexive", props.Reflexive));
            Properties.Add(new PropertyResult("Irreflexive", props.Irreflexive));
            Properties.Add(new PropertyResult("Symmetric", props.Symmetric));
            Properties.Add(new PropertyResult("Antisymmetric", props.Antisymmetric));
            Properties.Add(new PropertyResult("Transitive", props.Transitive));
            Properties.Add(new PropertyResult("Equivalence", props.IsEquivalence));
            Properties.Add(new PropertyResult("Partial order", props.IsPartialOrder));

            RelationVerdict = props.IsEquivalence
                ? "This is an equivalence relation."
                : props.IsPartialOrder
                    ? "This is a partial order."
                    : "Neither equivalence nor partial order.";

            RelationFailures.Clear();
            foreach (var f in props.Failures) RelationFailures.Add(f);

            // Matrix
            var (order, matrix) = RelationAnalyzer.ToMatrix(baseSet, pairs);
            MatrixHeader.Clear();
            MatrixRows.Clear();
            foreach (var v in order) MatrixHeader.Add(v.ToString());
            for (int i = 0; i < order.Length; i++)
            {
                var cells = new List<MatrixCell>(order.Length);
                for (int j = 0; j < order.Length; j++)
                    cells.Add(new MatrixCell(i, j, matrix[i, j]));
                MatrixRows.Add(new MatrixRow(order[i].ToString(), cells));
            }
            StatusLine = $"Analyzed relation with {pairs.Count} pair(s) over {baseSet.Count} element(s).";
        }
        catch (Exception ex)
        {
            StatusLine = $"Relation analysis failed: {ex.Message}";
            RelationVerdict = string.Empty;
            Properties.Clear();
            RelationFailures.Clear();
            MatrixHeader.Clear();
            MatrixRows.Clear();
            _logger.Warn($"Relation analysis failed: {ex.Message}");
        }
    }

    private static List<int> ParseInts(string text)
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        foreach (var raw in text.Split(new[] { ',', ' ', '{', '}', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                throw new FormatException($"'{raw}' is not an integer.");
            result.Add(v);
        }
        return result;
    }

    private static List<(int, int)> ParsePairs(string text)
    {
        // Accepts "(1,2),(2,3)" or "1 2, 2 3" — anything reasonable
        var result = new List<(int, int)>();
        if (string.IsNullOrWhiteSpace(text)) return result;

        // Strip parens, split on commas and whitespace, then take pairs
        var cleaned = text.Replace("(", " ").Replace(")", " ");
        var tokens = cleaned.Split(new[] { ',', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length % 2 != 0)
            throw new FormatException("Pairs must come in (a, b) form.");
        for (int i = 0; i < tokens.Length; i += 2)
        {
            if (!int.TryParse(tokens[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var a))
                throw new FormatException($"'{tokens[i]}' is not an integer.");
            if (!int.TryParse(tokens[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
                throw new FormatException($"'{tokens[i + 1]}' is not an integer.");
            result.Add((a, b));
        }
        return result;
    }

    private static string Format(IReadOnlySet<int> set) =>
        "{" + string.Join(", ", set.OrderBy(x => x)) + "}";
}
