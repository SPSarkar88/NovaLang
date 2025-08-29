using System;

namespace NovaLang.Lexer;

/// <summary>
/// Represents the types of tokens in NovaLang
/// </summary>
public enum TokenType
{
    // Literals
    Number,
    String,
    Boolean,
    Null,
    Undefined,
    
    // Identifiers and Keywords
    Identifier,
    
    // Keywords
    Let,
    Const,
    Function,
    Return,
    If,
    Else,
    For,
    While,
    Do,
    Switch,
    Case,
    Default,
    Break,
    Continue,
    Try,
    Catch,
    Finally,
    Throw,
    Import,
    Export,
    From,
    True,
    False,
    
    // Operators
    Plus,           // +
    Minus,          // -
    Star,           // *
    Slash,          // /
    Percent,        // %
    StarStar,       // **
    
    // Comparison
    Equal,          // ==
    NotEqual,       // !=
    StrictEqual,    // ===
    StrictNotEqual, // !==
    Less,           // <
    LessEqual,      // <=
    Greater,        // >
    GreaterEqual,   // >=
    
    // Logical
    And,            // &&
    Or,             // ||
    Not,            // !
    
    // Nullish
    Nullish,        // ??
    
    // Assignment
    Assign,         // =
    PlusAssign,     // +=
    MinusAssign,    // -=
    
    // Punctuation
    LeftParen,      // (
    RightParen,     // )
    LeftBrace,      // {
    RightBrace,     // }
    LeftBracket,    // [
    RightBracket,   // ]
    Comma,          // ,
    Semicolon,      // ;
    Colon,          // :
    Dot,            // .
    Question,       // ?
    Arrow,          // =>
    Spread,         // ...
    
    // Template literals
    TemplateString,
    TemplateStart,
    TemplateMiddle,
    TemplateEnd,
    
    // Special
    Newline,
    EOF,
    Invalid
}

/// <summary>
/// Represents a source position in the code
/// </summary>
public readonly struct SourcePosition
{
    public int Line { get; }
    public int Column { get; }
    public int Index { get; }
    
    public SourcePosition(int line, int column, int index)
    {
        Line = line;
        Column = column;
        Index = index;
    }
    
    public override string ToString() => $"{Line}:{Column}";
}

/// <summary>
/// Represents a source range in the code
/// </summary>
public readonly struct SourceRange
{
    public SourcePosition Start { get; }
    public SourcePosition End { get; }
    
    public SourceRange(SourcePosition start, SourcePosition end)
    {
        Start = start;
        End = end;
    }
    
    public override string ToString() => $"{Start}-{End}";
}

/// <summary>
/// Represents a token in the NovaLang source code
/// </summary>
public readonly struct Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public SourceRange Range { get; }
    
    public Token(TokenType type, string value, SourceRange range)
    {
        Type = type;
        Value = value;
        Range = range;
    }
    
    public Token(TokenType type, string value, SourcePosition position)
        : this(type, value, new SourceRange(position, position))
    {
    }
    
    public bool IsKeyword => Type >= TokenType.Let && Type <= TokenType.False;
    public bool IsOperator => Type >= TokenType.Plus && Type <= TokenType.Nullish;
    public bool IsLiteral => Type >= TokenType.Number && Type <= TokenType.Undefined;
    
    public override string ToString() => $"{Type}({Value}) at {Range}";
}
