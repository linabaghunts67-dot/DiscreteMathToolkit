namespace DiscreteMathToolkit.Core.Logic;

/// <summary>
/// Recursive-descent parser. Precedence (high → low):
///   NOT  &gt;  AND  &gt;  XOR  &gt;  OR  &gt;  IMPLIES (right-assoc)  &gt;  EQUIV (left-assoc).
/// </summary>
public sealed class LogicParser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _pos;

    public LogicParser(IReadOnlyList<Token> tokens) { _tokens = tokens; }

    public static LogicNode Parse(string source)
    {
        var tokens = new LogicTokenizer(source).Tokenize();
        var parser = new LogicParser(tokens);
        var node = parser.ParseEquiv();
        parser.Expect(TokenKind.EndOfInput);
        return node;
    }

    private Token Peek() => _tokens[_pos];
    private Token Consume() => _tokens[_pos++];
    private bool Match(TokenKind k) { if (Peek().Kind == k) { _pos++; return true; } return false; }
    private void Expect(TokenKind k)
    {
        var t = Peek();
        if (t.Kind != k)
            throw new FormatException($"Expected {k} at position {t.Position} but got {t.Kind} ('{t.Lexeme}').");
        _pos++;
    }

    // equiv := implies (EQUIV implies)*  (left assoc)
    private LogicNode ParseEquiv()
    {
        var left = ParseImplies();
        while (Peek().Kind == TokenKind.Equiv)
        {
            _pos++;
            var right = ParseImplies();
            left = new BinNode(BinOp.Equiv, left, right);
        }
        return left;
    }

    // implies := or (IMPLIES implies)?  (right assoc)
    private LogicNode ParseImplies()
    {
        var left = ParseOr();
        if (Peek().Kind == TokenKind.Implies)
        {
            _pos++;
            var right = ParseImplies();
            return new BinNode(BinOp.Implies, left, right);
        }
        return left;
    }

    private LogicNode ParseOr()
    {
        var left = ParseXor();
        while (Peek().Kind == TokenKind.Or)
        {
            _pos++;
            var right = ParseXor();
            left = new BinNode(BinOp.Or, left, right);
        }
        return left;
    }

    private LogicNode ParseXor()
    {
        var left = ParseAnd();
        while (Peek().Kind == TokenKind.Xor)
        {
            _pos++;
            var right = ParseAnd();
            left = new BinNode(BinOp.Xor, left, right);
        }
        return left;
    }

    private LogicNode ParseAnd()
    {
        var left = ParseNot();
        while (Peek().Kind == TokenKind.And)
        {
            _pos++;
            var right = ParseNot();
            left = new BinNode(BinOp.And, left, right);
        }
        return left;
    }

    private LogicNode ParseNot()
    {
        if (Peek().Kind == TokenKind.Not)
        {
            _pos++;
            return new NotNode(ParseNot());
        }
        return ParsePrimary();
    }

    private LogicNode ParsePrimary()
    {
        var t = Peek();
        switch (t.Kind)
        {
            case TokenKind.LParen:
                _pos++;
                var inside = ParseEquiv();
                Expect(TokenKind.RParen);
                return inside;
            case TokenKind.Variable:
                _pos++;
                return new VarNode(t.Lexeme);
            case TokenKind.Constant:
                _pos++;
                return new ConstNode(t.Lexeme == "1");
            default:
                throw new FormatException($"Unexpected token {t.Kind} ('{t.Lexeme}') at position {t.Position}.");
        }
    }
}
