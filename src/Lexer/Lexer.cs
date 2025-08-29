using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NovaLang.Lexer;

/// <summary>
/// Lexical analyzer for NovaLang source code
/// </summary>
public class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line;
    private int _column;
    
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["let"] = TokenType.Let,
        ["const"] = TokenType.Const,
        ["function"] = TokenType.Function,
        ["return"] = TokenType.Return,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["for"] = TokenType.For,
        ["while"] = TokenType.While,
        ["do"] = TokenType.Do,
        ["switch"] = TokenType.Switch,
        ["case"] = TokenType.Case,
        ["default"] = TokenType.Default,
        ["break"] = TokenType.Break,
        ["continue"] = TokenType.Continue,
        ["try"] = TokenType.Try,
        ["catch"] = TokenType.Catch,
        ["finally"] = TokenType.Finally,
        ["throw"] = TokenType.Throw,
        ["import"] = TokenType.Import,
        ["export"] = TokenType.Export,
        ["from"] = TokenType.From,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["null"] = TokenType.Null,
        ["undefined"] = TokenType.Undefined,
    };
    
    public Lexer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _position = 0;
        _line = 1;
        _column = 1;
    }
    
    public IEnumerable<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            var token = NextToken();
            if (token.Type != TokenType.Invalid)
            {
                yield return token;
            }
            
            if (token.Type == TokenType.EOF)
                break;
        }
    }
    
    private Token NextToken()
    {
        SkipWhitespace();
        
        if (IsAtEnd())
            return CreateToken(TokenType.EOF, "", CurrentPosition());
            
        var start = CurrentPosition();
        var ch = Advance();
        
        return ch switch
        {
            // Single character tokens
            '(' => CreateToken(TokenType.LeftParen, "(", start),
            ')' => CreateToken(TokenType.RightParen, ")", start),
            '{' => CreateToken(TokenType.LeftBrace, "{", start),
            '}' => CreateToken(TokenType.RightBrace, "}", start),
            '[' => CreateToken(TokenType.LeftBracket, "[", start),
            ']' => CreateToken(TokenType.RightBracket, "]", start),
            ',' => CreateToken(TokenType.Comma, ",", start),
            ';' => CreateToken(TokenType.Semicolon, ";", start),
            ':' => CreateToken(TokenType.Colon, ":", start),
            '?' => Match('?') ? CreateToken(TokenType.Nullish, "??", start) 
                              : CreateToken(TokenType.Question, "?", start),
            '%' => CreateToken(TokenType.Percent, "%", start),
            '\n' => CreateToken(TokenType.Newline, "\n", start),
            
            // Operators that can be multi-character
            '+' => Match('=') ? CreateToken(TokenType.PlusAssign, "+=", start)
                              : CreateToken(TokenType.Plus, "+", start),
            '-' => Match('=') ? CreateToken(TokenType.MinusAssign, "-=", start)
                              : CreateToken(TokenType.Minus, "-", start),
            '*' => Match('*') ? CreateToken(TokenType.StarStar, "**", start)
                              : CreateToken(TokenType.Star, "*", start),
            '/' => Match('/') ? SkipLineComment() : CreateToken(TokenType.Slash, "/", start),
            '=' => Match('=') ? (Match('=') ? CreateToken(TokenType.StrictEqual, "===", start)
                                            : CreateToken(TokenType.Equal, "==", start))
                              : Match('>') ? CreateToken(TokenType.Arrow, "=>", start)
                                           : CreateToken(TokenType.Assign, "=", start),
            '!' => Match('=') ? (Match('=') ? CreateToken(TokenType.StrictNotEqual, "!==", start)
                                            : CreateToken(TokenType.NotEqual, "!=", start))
                              : CreateToken(TokenType.Not, "!", start),
            '<' => Match('=') ? CreateToken(TokenType.LessEqual, "<=", start)
                              : CreateToken(TokenType.Less, "<", start),
            '>' => Match('=') ? CreateToken(TokenType.GreaterEqual, ">=", start)
                              : CreateToken(TokenType.Greater, ">", start),
            '&' => Match('&') ? CreateToken(TokenType.And, "&&", start)
                              : CreateToken(TokenType.Invalid, "&", start),
            '|' => Match('|') ? CreateToken(TokenType.Or, "||", start)
                              : CreateToken(TokenType.Invalid, "|", start),
            '.' => Match('.') && Match('.') ? CreateToken(TokenType.Spread, "...", start)
                                            : CreateToken(TokenType.Dot, ".", start),
            
            // String literals
            '"' or '\'' => ScanString(ch, start),
            
            // Template literals
            '`' => ScanTemplateString(start),
            
            // Numbers
            var digit when char.IsDigit(digit) => ScanNumber(start),
            
            // Identifiers and keywords
            var letter when IsIdentifierStart(letter) => ScanIdentifier(start),
            
            _ => CreateToken(TokenType.Invalid, ch.ToString(), start)
        };
    }
    
    private Token SkipLineComment()
    {
        while (Peek() != '\n' && !IsAtEnd())
            Advance();
        return NextToken(); // Skip the comment and get the next token
    }
    
    private Token ScanString(char quote, SourcePosition start)
    {
        var value = new StringBuilder();
        
        while (Peek() != quote && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            
            if (Peek() == '\\')
            {
                Advance(); // consume backslash
                var escaped = Advance();
                value.Append(escaped switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '\'' => '\'',
                    '"' => '"',
                    _ => escaped
                });
            }
            else
            {
                value.Append(Advance());
            }
        }
        
        if (IsAtEnd())
        {
            return CreateToken(TokenType.Invalid, "Unterminated string", start);
        }
        
        Advance(); // closing quote
        return CreateToken(TokenType.String, value.ToString(), start);
    }
    
    private Token ScanTemplateString(SourcePosition start)
    {
        var value = new StringBuilder();
        
        while (Peek() != '`' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            
            if (Peek() == '$' && PeekNext() == '{')
            {
                // TODO: Handle template expressions in future
                value.Append(Advance());
            }
            else if (Peek() == '\\')
            {
                Advance(); // consume backslash
                var escaped = Advance();
                value.Append(escaped switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '`' => '`',
                    _ => escaped
                });
            }
            else
            {
                value.Append(Advance());
            }
        }
        
        if (IsAtEnd())
        {
            return CreateToken(TokenType.Invalid, "Unterminated template string", start);
        }
        
        Advance(); // closing backtick
        return CreateToken(TokenType.TemplateString, value.ToString(), start);
    }
    
    private Token ScanNumber(SourcePosition start)
    {
        while (char.IsDigit(Peek()))
            Advance();
            
        // Look for decimal part
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance(); // consume '.'
            while (char.IsDigit(Peek()))
                Advance();
        }
        
        // Look for exponent
        if (char.ToLower(Peek()) == 'e')
        {
            Advance();
            if (Peek() == '+' || Peek() == '-')
                Advance();
            while (char.IsDigit(Peek()))
                Advance();
        }
        
        var value = _source[start.Index.._position];
        return CreateToken(TokenType.Number, value, start);
    }
    
    private Token ScanIdentifier(SourcePosition start)
    {
        while (IsIdentifierContinue(Peek()))
            Advance();
            
        var value = _source[start.Index.._position];
        var tokenType = Keywords.TryGetValue(value, out var keyword) ? keyword : TokenType.Identifier;
        return CreateToken(tokenType, value, start);
    }
    
    private void SkipWhitespace()
    {
        while (!IsAtEnd())
        {
            var ch = Peek();
            if (ch == ' ' || ch == '\r' || ch == '\t')
            {
                Advance();
            }
            else
            {
                break;
            }
        }
    }
    
    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_position] != expected)
            return false;
            
        _position++;
        _column++;
        return true;
    }
    
    private char Advance()
    {
        if (IsAtEnd()) return '\0';
        
        var ch = _source[_position++];
        if (ch == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        return ch;
    }
    
    private char Peek() => IsAtEnd() ? '\0' : _source[_position];
    private char PeekNext() => _position + 1 >= _source.Length ? '\0' : _source[_position + 1];
    
    private bool IsAtEnd() => _position >= _source.Length;
    
    private SourcePosition CurrentPosition() => new(_line, _column, _position);
    
    private Token CreateToken(TokenType type, string value, SourcePosition start)
    {
        var end = new SourcePosition(_line, _column, _position);
        var range = new SourceRange(start, end);
        return new Token(type, value, range);
    }
    
    private static bool IsIdentifierStart(char ch) => 
        char.IsLetter(ch) || ch == '_' || ch == '$';
        
    private static bool IsIdentifierContinue(char ch) => 
        IsIdentifierStart(ch) || char.IsDigit(ch);
}
