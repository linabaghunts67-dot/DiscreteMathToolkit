using System.Text;

namespace DiscreteMathToolkit.Core.Logic;

/// <summary>
/// Tokenizes propositional logic expressions. Accepts symbolic and word forms:
///   NOT: !, ~, ¬, NOT, not
///   AND: &amp;, &amp;&amp;, ∧, AND, and
///   OR : |, ||, ∨, OR, or
///   XOR: ^, ⊕, XOR, xor
///   IMPLIES: -&gt;, =&gt;, →, IMPLIES, implies
///   EQUIV  : &lt;-&gt;, &lt;=&gt;, ↔, ≡, EQUIV, iff
///   Constants: 0, 1, T, F, TRUE, FALSE
///   Variables: identifier ([A-Za-z_][A-Za-z0-9_]*)
/// </summary>
public sealed class LogicTokenizer
{
    private readonly string _src;
    private int _pos;

    public LogicTokenizer(string source) { _src = source ?? throw new ArgumentNullException(nameof(source)); }

    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();
        _pos = 0;
        while (_pos < _src.Length)
        {
            char c = _src[_pos];
            if (char.IsWhiteSpace(c)) { _pos++; continue; }

            if (TryMatchMulti("<->", out var t) || TryMatchMulti("<=>", out t)) { tokens.Add(new Token(TokenKind.Equiv, t!, _pos - 3)); continue; }
            if (TryMatchMulti("->", out t) || TryMatchMulti("=>", out t)) { tokens.Add(new Token(TokenKind.Implies, t!, _pos - 2)); continue; }
            if (TryMatchMulti("&&", out t)) { tokens.Add(new Token(TokenKind.And, t!, _pos - 2)); continue; }
            if (TryMatchMulti("||", out t)) { tokens.Add(new Token(TokenKind.Or, t!, _pos - 2)); continue; }

            switch (c)
            {
                case '(': tokens.Add(new Token(TokenKind.LParen, "(", _pos++)); continue;
                case ')': tokens.Add(new Token(TokenKind.RParen, ")", _pos++)); continue;
                case '!':
                case '~':
                case '\u00AC': // ¬
                    tokens.Add(new Token(TokenKind.Not, c.ToString(), _pos++)); continue;
                case '&':
                case '\u2227': // ∧
                    tokens.Add(new Token(TokenKind.And, c.ToString(), _pos++)); continue;
                case '|':
                case '\u2228': // ∨
                    tokens.Add(new Token(TokenKind.Or, c.ToString(), _pos++)); continue;
                case '^':
                case '\u2295': // ⊕
                    tokens.Add(new Token(TokenKind.Xor, c.ToString(), _pos++)); continue;
                case '\u2192': // →
                    tokens.Add(new Token(TokenKind.Implies, c.ToString(), _pos++)); continue;
                case '\u2194': // ↔
                case '\u2261': // ≡
                    tokens.Add(new Token(TokenKind.Equiv, c.ToString(), _pos++)); continue;
                case '0': tokens.Add(new Token(TokenKind.Constant, "0", _pos++)); continue;
                case '1': tokens.Add(new Token(TokenKind.Constant, "1", _pos++)); continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                int start = _pos;
                var sb = new StringBuilder();
                while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] == '_'))
                    sb.Append(_src[_pos++]);
                string word = sb.ToString();
                tokens.Add(ClassifyWord(word, start));
                continue;
            }

            throw new FormatException($"Unexpected character '{c}' at position {_pos}.");
        }
        tokens.Add(new Token(TokenKind.EndOfInput, "", _pos));
        return tokens;
    }

    private bool TryMatchMulti(string s, out string? matched)
    {
        if (_pos + s.Length <= _src.Length && _src.Substring(_pos, s.Length) == s)
        {
            _pos += s.Length;
            matched = s;
            return true;
        }
        matched = null;
        return false;
    }

    private static Token ClassifyWord(string word, int start)
    {
        return word.ToUpperInvariant() switch
        {
            "NOT" => new Token(TokenKind.Not, word, start),
            "AND" => new Token(TokenKind.And, word, start),
            "OR" => new Token(TokenKind.Or, word, start),
            "XOR" => new Token(TokenKind.Xor, word, start),
            "IMPLIES" => new Token(TokenKind.Implies, word, start),
            "EQUIV" or "IFF" => new Token(TokenKind.Equiv, word, start),
            "TRUE" or "T" => new Token(TokenKind.Constant, "1", start),
            "FALSE" or "F" => new Token(TokenKind.Constant, "0", start),
            _ => new Token(TokenKind.Variable, word, start),
        };
    }
}
