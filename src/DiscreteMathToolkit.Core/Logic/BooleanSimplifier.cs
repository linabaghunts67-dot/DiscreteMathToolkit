namespace DiscreteMathToolkit.Core.Logic;

/// <summary>
/// Conservative boolean simplifier. Applies a small but useful set of rewrite rules
/// repeatedly until a fixed point. Not a full minimizer (Quine-McCluskey is out of scope),
/// but covers identity, dominance, idempotence, double negation, and trivial absorption.
/// </summary>
public static class BooleanSimplifier
{
    public static LogicNode Simplify(LogicNode node)
    {
        var prev = node;
        for (int i = 0; i < 64; i++)
        {
            var next = Step(prev);
            if (next.Equals(prev)) return next;
            prev = next;
        }
        return prev;
    }

    private static LogicNode Step(LogicNode node) => node switch
    {
        VarNode or ConstNode => node,
        NotNode n => SimplifyNot(Step(n.Inner)),
        BinNode b => SimplifyBin(b.Op, Step(b.Left), Step(b.Right)),
        _ => node
    };

    private static LogicNode SimplifyNot(LogicNode inner) => inner switch
    {
        ConstNode c => new ConstNode(!c.Value),
        NotNode n => n.Inner,            // ¬¬x = x
        _ => new NotNode(inner)
    };

    private static LogicNode SimplifyBin(BinOp op, LogicNode l, LogicNode r)
    {
        // both constants → fold
        if (l is ConstNode cl && r is ConstNode cr)
        {
            return op switch
            {
                BinOp.And => new ConstNode(cl.Value && cr.Value),
                BinOp.Or => new ConstNode(cl.Value || cr.Value),
                BinOp.Xor => new ConstNode(cl.Value ^ cr.Value),
                BinOp.Implies => new ConstNode(!cl.Value || cr.Value),
                BinOp.Equiv => new ConstNode(cl.Value == cr.Value),
                _ => new BinNode(op, l, r)
            };
        }

        switch (op)
        {
            case BinOp.And:
                if (l is ConstNode lc) return lc.Value ? r : new ConstNode(false);
                if (r is ConstNode rc) return rc.Value ? l : new ConstNode(false);
                if (l.Equals(r)) return l;                        // idempotent
                if (IsNegationOf(l, r)) return new ConstNode(false); // x ∧ ¬x = 0
                break;
            case BinOp.Or:
                if (l is ConstNode lc2) return lc2.Value ? new ConstNode(true) : r;
                if (r is ConstNode rc2) return rc2.Value ? new ConstNode(true) : l;
                if (l.Equals(r)) return l;
                if (IsNegationOf(l, r)) return new ConstNode(true);
                break;
            case BinOp.Xor:
                if (l is ConstNode lcx)
                    return lcx.Value ? SimplifyNot(r) : r;
                if (r is ConstNode rcx)
                    return rcx.Value ? SimplifyNot(l) : l;
                if (l.Equals(r)) return new ConstNode(false);
                if (IsNegationOf(l, r)) return new ConstNode(true);
                break;
            case BinOp.Implies:
                if (l is ConstNode lci) return lci.Value ? r : new ConstNode(true);
                if (r is ConstNode rci) return rci.Value ? new ConstNode(true) : SimplifyNot(l);
                if (l.Equals(r)) return new ConstNode(true);
                break;
            case BinOp.Equiv:
                if (l is ConstNode lce) return lce.Value ? r : SimplifyNot(r);
                if (r is ConstNode rce) return rce.Value ? l : SimplifyNot(l);
                if (l.Equals(r)) return new ConstNode(true);
                if (IsNegationOf(l, r)) return new ConstNode(false);
                break;
        }
        return new BinNode(op, l, r);
    }

    private static bool IsNegationOf(LogicNode a, LogicNode b) =>
        (a is NotNode na && na.Inner.Equals(b)) ||
        (b is NotNode nb && nb.Inner.Equals(a));
}
