namespace DiscreteMathToolkit.Core.Logic;

public enum TokenKind
{
    Variable,
    Constant,
    Not,
    And,
    Or,
    Xor,
    Implies,
    Equiv,
    LParen,
    RParen,
    EndOfInput
}

public readonly record struct Token(TokenKind Kind, string Lexeme, int Position);
