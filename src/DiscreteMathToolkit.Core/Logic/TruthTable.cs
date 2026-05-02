namespace DiscreteMathToolkit.Core.Logic;

public sealed class TruthTableRow
{
    public IReadOnlyDictionary<string, bool> Assignment { get; }
    public bool Result { get; }

    public TruthTableRow(IReadOnlyDictionary<string, bool> assignment, bool result)
    {
        Assignment = assignment;
        Result = result;
    }
}

public enum LogicClassification { Tautology, Contradiction, Contingency }

public sealed class TruthTable
{
    public IReadOnlyList<string> Variables { get; }
    public IReadOnlyList<TruthTableRow> Rows { get; }
    public LogicClassification Classification { get; }
    public string ExpressionInfix { get; }

    public TruthTable(IReadOnlyList<string> variables, IReadOnlyList<TruthTableRow> rows, LogicClassification classification, string expressionInfix)
    {
        Variables = variables;
        Rows = rows;
        Classification = classification;
        ExpressionInfix = expressionInfix;
    }
}

public static class TruthTableBuilder
{
    public static TruthTable Build(LogicNode expression)
    {
        var variables = expression.Variables().Distinct().OrderBy(v => v, StringComparer.Ordinal).ToList();
        if (variables.Count > 16)
            throw new InvalidOperationException("Too many variables for a truth table (max 16).");

        var rows = new List<TruthTableRow>(1 << variables.Count);
        bool sawTrue = false, sawFalse = false;

        int total = 1 << variables.Count;
        for (int mask = 0; mask < total; mask++)
        {
            var env = new Dictionary<string, bool>(variables.Count);
            for (int i = 0; i < variables.Count; i++)
            {
                // most significant bit = first variable, so the table reads top-to-bottom like a textbook
                bool val = ((mask >> (variables.Count - 1 - i)) & 1) == 1;
                env[variables[i]] = val;
            }
            bool result = expression.Evaluate(env);
            sawTrue |= result;
            sawFalse |= !result;
            rows.Add(new TruthTableRow(env, result));
        }

        var classification =
            sawTrue && sawFalse ? LogicClassification.Contingency :
            sawTrue ? LogicClassification.Tautology :
            LogicClassification.Contradiction;

        return new TruthTable(variables, rows, classification, expression.ToInfix());
    }
}
