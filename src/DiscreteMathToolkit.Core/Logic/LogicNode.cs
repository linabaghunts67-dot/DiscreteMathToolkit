namespace DiscreteMathToolkit.Core.Logic;

public abstract record LogicNode
{
    public abstract bool Evaluate(IReadOnlyDictionary<string, bool> env);
    public abstract string ToInfix();
    public abstract IEnumerable<string> Variables();
}

public sealed record VarNode(string Name) : LogicNode
{
    public override bool Evaluate(IReadOnlyDictionary<string, bool> env) =>
        env.TryGetValue(Name, out var v) ? v : throw new InvalidOperationException($"Variable '{Name}' is not assigned.");
    public override string ToInfix() => Name;
    public override IEnumerable<string> Variables() { yield return Name; }
}

public sealed record ConstNode(bool Value) : LogicNode
{
    public override bool Evaluate(IReadOnlyDictionary<string, bool> env) => Value;
    public override string ToInfix() => Value ? "1" : "0";
    public override IEnumerable<string> Variables() => Array.Empty<string>();
}

public sealed record NotNode(LogicNode Inner) : LogicNode
{
    public override bool Evaluate(IReadOnlyDictionary<string, bool> env) => !Inner.Evaluate(env);
    public override string ToInfix() => $"\u00AC({Inner.ToInfix()})";
    public override IEnumerable<string> Variables() => Inner.Variables();
}

public enum BinOp { And, Or, Xor, Implies, Equiv }

public sealed record BinNode(BinOp Op, LogicNode Left, LogicNode Right) : LogicNode
{
    public override bool Evaluate(IReadOnlyDictionary<string, bool> env)
    {
        bool a = Left.Evaluate(env), b = Right.Evaluate(env);
        return Op switch
        {
            BinOp.And => a && b,
            BinOp.Or => a || b,
            BinOp.Xor => a ^ b,
            BinOp.Implies => !a || b,
            BinOp.Equiv => a == b,
            _ => throw new InvalidOperationException()
        };
    }

    public override string ToInfix()
    {
        string sym = Op switch
        {
            BinOp.And => "\u2227",     // ∧
            BinOp.Or => "\u2228",      // ∨
            BinOp.Xor => "\u2295",     // ⊕
            BinOp.Implies => "\u2192", // →
            BinOp.Equiv => "\u2194",   // ↔
            _ => "?"
        };
        return $"({Left.ToInfix()} {sym} {Right.ToInfix()})";
    }

    public override IEnumerable<string> Variables() =>
        Left.Variables().Concat(Right.Variables());
}
