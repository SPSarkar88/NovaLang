using System;
using System.Collections.Generic;
using System.Linq;
using NovaLang.AST;
using NovaLang.Lexer;

namespace NovaLang.Parser;

/// <summary>
/// Parse error exception
/// </summary>
public class ParseException : Exception
{
    public SourceRange Range { get; }
    
    public ParseException(string message, SourceRange range) : base(message)
    {
        Range = range;
    }
    
    public ParseException(string message, Token token) : base(message)
    {
        Range = token.Range;
    }
}

/// <summary>
/// Recursive descent parser for NovaLang
/// </summary>
public class Parser
{
    private readonly List<Token> _tokens;
    private int _current;
    
    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.Where(t => t.Type != TokenType.Newline).ToList(); // Filter out newlines for now
        _current = 0;
    }
    
    /// <summary>
    /// Parse a complete program
    /// </summary>
    public AstProgram Parse()
    {
        var statements = new List<Statement>();
        var start = CurrentToken.Range.Start;
        
        while (!IsAtEnd())
        {
            try
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    statements.Add(stmt);
            }
            catch (ParseException)
            {
                // Skip to next statement on error
                Synchronize();
            }
        }
        
        var end = _tokens.Count > 0 ? _tokens[^1].Range.End : start;
        return new AstProgram(statements, new SourceRange(start, end));
    }
    
    /// <summary>
    /// Parse a statement
    /// </summary>
    private Statement? ParseStatement()
    {
        switch (CurrentToken.Type)
        {
            case TokenType.Let:
            case TokenType.Const:
                return ParseVariableDeclaration();
            case TokenType.Function:
                return ParseFunctionDeclaration();
            case TokenType.If:
                return ParseIfStatement();
            case TokenType.While:
                return ParseWhileStatement();
            case TokenType.For:
                return ParseForStatement();
            case TokenType.Return:
                return ParseReturnStatement();
            case TokenType.Break:
                return ParseBreakStatement();
            case TokenType.Continue:
                return ParseContinueStatement();
            case TokenType.Switch:
                return ParseSwitchStatement();
            case TokenType.Try:
                return ParseTryStatement();
            case TokenType.Throw:
                return ParseThrowStatement();
            case TokenType.LeftBrace:
                return ParseBlockStatement();
            case TokenType.Semicolon:
                return ParseEmptyStatement();
            default:
                return ParseExpressionStatement();
        }
    }
    
    /// <summary>
    /// Parse empty statement (just semicolon)
    /// </summary>
    private Statement? ParseEmptyStatement()
    {
        Advance(); // consume ';'
        return null; // Return null to skip empty statements
    }

    /// <summary>
    /// Parse variable declaration (let/const)
    /// </summary>
    private VariableDeclaration ParseVariableDeclaration()
    {
        var start = CurrentToken.Range.Start;
        var kind = CurrentToken.Type;
        Advance(); // consume 'let' or 'const'
        
        var declarations = new List<VariableDeclarator>();
        
        do
        {
            Expression pattern;
            
            // Check if it's a destructuring pattern
            if (Check(TokenType.LeftBracket))
            {
                // Array destructuring pattern
                pattern = ParseArrayPattern();
            }
            else if (Check(TokenType.LeftBrace))
            {
                // Object destructuring pattern
                pattern = ParseObjectPattern();
            }
            else
            {
                // Simple identifier
                var name = Consume(TokenType.Identifier, "Expected variable name").Value;
                pattern = new IdentifierExpression(name, PreviousToken.Range);
            }
            
            Expression? init = null;
            
            if (Match(TokenType.Assign))
            {
                init = ParseExpression();
            }
            else if (kind == TokenType.Const)
            {
                throw new ParseException("Missing initializer in const declaration", CurrentToken);
            }
            
            declarations.Add(new VariableDeclarator(pattern, init));
        }
        while (Match(TokenType.Comma));
        
        Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
        
        var end = PreviousToken.Range.End;
        return new VariableDeclaration(kind, declarations, new SourceRange(start, end));
    }
    
    /// <summary>
    /// Parse function declaration
    /// </summary>
    private FunctionDeclaration ParseFunctionDeclaration()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'function'
        
        var name = Consume(TokenType.Identifier, "Expected function name").Value;
        var parameters = ParseParameterList();
        var body = ParseBlockStatement();
        
        var end = body.Range.End;
        return new FunctionDeclaration(name, parameters, body, new SourceRange(start, end));
    }
    
    /// <summary>
    /// Parse parameter list for functions
    /// </summary>
    private List<Parameter> ParseParameterList()
    {
        var parameters = new List<Parameter>();
        
        Consume(TokenType.LeftParen, "Expected '(' before parameters");
        
        if (!Check(TokenType.RightParen))
        {
            do
            {
                var name = Consume(TokenType.Identifier, "Expected parameter name").Value;
                Expression? defaultValue = null;
                
                if (Match(TokenType.Assign))
                {
                    defaultValue = ParseExpression();
                }
                
                parameters.Add(new Parameter(name, defaultValue));
            }
            while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        return parameters;
    }
    
    /// <summary>
    /// Parse block statement
    /// </summary>
    private BlockStatement ParseBlockStatement()
    {
        var start = CurrentToken.Range.Start;
        Consume(TokenType.LeftBrace, "Expected '{'");
        
        var statements = new List<Statement>();
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            var stmt = ParseStatement();
            if (stmt != null)
                statements.Add(stmt);
        }
        
        var endToken = Consume(TokenType.RightBrace, "Expected '}'");
        
        return new BlockStatement(statements, new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse if statement
    /// </summary>
    private IfStatement ParseIfStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'if'
        
        Consume(TokenType.LeftParen, "Expected '(' after 'if'");
        var test = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after if condition");
        
        var consequent = ParseStatement()!;
        Statement? alternate = null;
        
        if (Match(TokenType.Else))
        {
            alternate = ParseStatement();
        }
        
        var end = (alternate ?? consequent).Range.End;
        return new IfStatement(test, consequent, alternate, new SourceRange(start, end));
    }
    
    /// <summary>
    /// Parse while statement
    /// </summary>
    private WhileStatement ParseWhileStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'while'
        
        Consume(TokenType.LeftParen, "Expected '(' after 'while'");
        var test = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after while condition");
        
        var body = ParseStatement()!;
        
        return new WhileStatement(test, body, new SourceRange(start, body.Range.End));
    }
    
    /// <summary>
    /// Parse for statement
    /// </summary>
    private ForStatement ParseForStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'for'
        
        Consume(TokenType.LeftParen, "Expected '(' after 'for'");
        
        // Init
        AstNode? init = null;
        if (Match(TokenType.Semicolon))
        {
            // No init
        }
        else if (Check(TokenType.Let) || Check(TokenType.Const))
        {
            init = ParseVariableDeclaration();
        }
        else
        {
            init = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after for loop initializer");
        }
        
        // Test
        Expression? test = null;
        if (!Check(TokenType.Semicolon))
        {
            test = ParseExpression();
        }
        Consume(TokenType.Semicolon, "Expected ';' after for loop condition");
        
        // Update
        Expression? update = null;
        if (!Check(TokenType.RightParen))
        {
            update = ParseExpression();
        }
        Consume(TokenType.RightParen, "Expected ')' after for clauses");
        
        var body = ParseStatement()!;
        
        return new ForStatement(init, test, update, body, new SourceRange(start, body.Range.End));
    }
    
    /// <summary>
    /// Parse return statement
    /// </summary>
    private ReturnStatement ParseReturnStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'return'
        
        Expression? value = null;
        if (!Check(TokenType.Semicolon))
        {
            value = ParseExpression();
        }
        
        var endToken = Consume(TokenType.Semicolon, "Expected ';' after return value");
        
        return new ReturnStatement(value, new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse break statement
    /// </summary>
    private BreakStatement ParseBreakStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'break'
        
        var endToken = Consume(TokenType.Semicolon, "Expected ';' after 'break'");
        
        return new BreakStatement(new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse continue statement
    /// </summary>
    private ContinueStatement ParseContinueStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'continue'
        
        var endToken = Consume(TokenType.Semicolon, "Expected ';' after 'continue'");
        
        return new ContinueStatement(new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse switch statement - simplified implementation
    /// </summary>
    private SwitchStatement ParseSwitchStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'switch'
        
        Consume(TokenType.LeftParen, "Expected '(' after 'switch'");
        var discriminant = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after switch expression");
        
        Consume(TokenType.LeftBrace, "Expected '{' before switch body");
        
        var cases = new List<SwitchCase>();
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            Expression? test = null;
            
            if (Match(TokenType.Case))
            {
                test = ParseExpression();
                Consume(TokenType.Colon, "Expected ':' after case value");
            }
            else if (Match(TokenType.Default))
            {
                Consume(TokenType.Colon, "Expected ':' after 'default'");
            }
            else
            {
                throw new ParseException("Expected 'case' or 'default' in switch statement", CurrentToken);
            }
            
            var consequent = new List<Statement>();
            while (!Check(TokenType.Case) && !Check(TokenType.Default) && !Check(TokenType.RightBrace) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    consequent.Add(stmt);
            }
            
            cases.Add(new SwitchCase(test, consequent));
        }
        
        var endToken = Consume(TokenType.RightBrace, "Expected '}' after switch body");
        
        return new SwitchStatement(discriminant, cases, new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse try statement - simplified implementation
    /// </summary>
    private TryStatement ParseTryStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'try'
        
        var block = ParseBlockStatement();
        
        CatchClause? handler = null;
        if (Match(TokenType.Catch))
        {
            string? parameter = null;
            
            if (Match(TokenType.LeftParen))
            {
                parameter = Consume(TokenType.Identifier, "Expected catch parameter").Value;
                Consume(TokenType.RightParen, "Expected ')' after catch parameter");
            }
            
            var catchBody = ParseBlockStatement();
            handler = new CatchClause(parameter, catchBody);
        }
        
        BlockStatement? finalizer = null;
        if (Match(TokenType.Finally))
        {
            finalizer = ParseBlockStatement();
        }
        
        if (handler == null && finalizer == null)
        {
            throw new ParseException("Missing catch or finally after try", CurrentToken);
        }
        
        var end = finalizer?.Range.End ?? handler!.Body.Range.End;
        return new TryStatement(block, handler, finalizer, new SourceRange(start, end));
    }
    
    /// <summary>
    /// Parse throw statement
    /// </summary>
    private ThrowStatement ParseThrowStatement()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'throw'
        
        var argument = ParseExpression();
        var endToken = Consume(TokenType.Semicolon, "Expected ';' after throw expression");
        
        return new ThrowStatement(argument, new SourceRange(start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse expression statement
    /// </summary>
    private ExpressionStatement ParseExpressionStatement()
    {
        var expr = ParseExpression();
        var endToken = Consume(TokenType.Semicolon, "Expected ';' after expression");
        
        return new ExpressionStatement(expr, new SourceRange(expr.Range.Start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse expression
    /// </summary>
    private Expression ParseExpression()
    {
        return ParseAssignment();
    }
    
    /// <summary>
    /// Parse assignment expression
    /// </summary>
    private Expression ParseAssignment()
    {
        var expr = ParseTernary();
        
        if (Match(TokenType.Assign, TokenType.PlusAssign, TokenType.MinusAssign))
        {
            var op = PreviousToken.Type;
            var right = ParseAssignment();
            return new AssignmentExpression(expr, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse ternary conditional expression
    /// </summary>
    private Expression ParseTernary()
    {
        var expr = ParseLogicalOr();
        
        if (Match(TokenType.Question))
        {
            var consequent = ParseExpression();
            Consume(TokenType.Colon, "Expected ':' after ternary consequent");
            var alternate = ParseTernary();
            return new ConditionalExpression(expr, consequent, alternate, new SourceRange(expr.Range.Start, alternate.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse logical OR expression
    /// </summary>
    private Expression ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();
        
        while (Match(TokenType.Or))
        {
            var op = PreviousToken.Type;
            var right = ParseLogicalAnd();
            expr = new LogicalExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse logical AND expression
    /// </summary>
    private Expression ParseLogicalAnd()
    {
        var expr = ParseNullish();
        
        while (Match(TokenType.And))
        {
            var op = PreviousToken.Type;
            var right = ParseNullish();
            expr = new LogicalExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse nullish coalescing expression
    /// </summary>
    private Expression ParseNullish()
    {
        var expr = ParseEquality();
        
        while (Match(TokenType.Nullish))
        {
            var op = PreviousToken.Type;
            var right = ParseEquality();
            expr = new LogicalExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse equality expression
    /// </summary>
    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        
        while (Match(TokenType.Equal, TokenType.NotEqual, TokenType.StrictEqual, TokenType.StrictNotEqual))
        {
            var op = PreviousToken.Type;
            var right = ParseComparison();
            expr = new BinaryExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse comparison expression
    /// </summary>
    private Expression ParseComparison()
    {
        var expr = ParseTerm();
        
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = PreviousToken.Type;
            var right = ParseTerm();
            expr = new BinaryExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse term expression (+ -)
    /// </summary>
    private Expression ParseTerm()
    {
        var expr = ParseFactor();
        
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = PreviousToken.Type;
            var right = ParseFactor();
            expr = new BinaryExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse factor expression (* / %)
    /// </summary>
    private Expression ParseFactor()
    {
        var expr = ParseExponentiation();
        
        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
        {
            var op = PreviousToken.Type;
            var right = ParseExponentiation();
            expr = new BinaryExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse exponentiation expression (**)
    /// </summary>
    private Expression ParseExponentiation()
    {
        var expr = ParseUnary();
        
        if (Match(TokenType.StarStar))
        {
            var op = PreviousToken.Type;
            var right = ParseExponentiation(); // Right associative
            expr = new BinaryExpression(expr, op, right, new SourceRange(expr.Range.Start, right.Range.End));
        }
        
        return expr;
    }
    
    /// <summary>
    /// Parse unary expression
    /// </summary>
    private Expression ParseUnary()
    {
        if (Match(TokenType.Not, TokenType.Minus, TokenType.Plus))
        {
            var op = PreviousToken.Type;
            var right = ParseUnary();
            return new UnaryExpression(op, right, new SourceRange(PreviousToken.Range.Start, right.Range.End));
        }
        
        return ParseCall();
    }
    
    /// <summary>
    /// Parse call expression
    /// </summary>
    private Expression ParseCall()
    {
        var expr = ParsePrimary();
        
        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expected property name after '.'").Value;
                var property = new IdentifierExpression(name, PreviousToken.Range);
                expr = new MemberExpression(expr, property, false, new SourceRange(expr.Range.Start, property.Range.End));
            }
            else if (Match(TokenType.LeftBracket))
            {
                var index = ParseExpression();
                var endToken = Consume(TokenType.RightBracket, "Expected ']' after computed property");
                expr = new MemberExpression(expr, index, true, new SourceRange(expr.Range.Start, endToken.Range.End));
            }
            else
            {
                break;
            }
        }
        
        return expr;
    }
    
    /// <summary>
    /// Finish parsing a call expression
    /// </summary>
    private CallExpression FinishCall(Expression callee)
    {
        var arguments = new List<Expression>();
        
        if (!Check(TokenType.RightParen))
        {
            do
            {
                arguments.Add(ParseExpression());
            }
            while (Match(TokenType.Comma));
        }
        
        var endToken = Consume(TokenType.RightParen, "Expected ')' after arguments");
        
        return new CallExpression(callee, arguments, new SourceRange(callee.Range.Start, endToken.Range.End));
    }
    
    /// <summary>
    /// Parse primary expression
    /// </summary>
    private Expression ParsePrimary()
    {
        var token = CurrentToken;
        
        switch (token.Type)
        {
            case TokenType.True:
                Advance();
                return new LiteralExpression(true, TokenType.True, token.Range);
                
            case TokenType.False:
                Advance();
                return new LiteralExpression(false, TokenType.False, token.Range);
                
            case TokenType.Null:
                Advance();
                return new LiteralExpression(null, TokenType.Null, token.Range);
                
            case TokenType.Undefined:
                Advance();
                return new LiteralExpression(null, TokenType.Undefined, token.Range);
                
            case TokenType.Number:
                return ParseNumber();
                
            case TokenType.String:
                return ParseString();
                
            case TokenType.TemplateString:
                return ParseTemplateString();
                
            case TokenType.Identifier:
                return ParseIdentifier();
                
            case TokenType.LeftParen:
                return ParseGrouping();
                
            case TokenType.LeftBracket:
                return ParseArray();
                
            case TokenType.LeftBrace:
                return ParseObject();
                
            case TokenType.Function:
                return ParseFunctionExpression();
                
            default:
                throw new ParseException($"Unexpected token '{token.Value}'", token);
        }
    }
    
    private Expression ParseNumber()
    {
        var token = Advance();
        if (double.TryParse(token.Value, out var value))
        {
            return new LiteralExpression(value, TokenType.Number, token.Range);
        }
        throw new ParseException($"Invalid number '{token.Value}'", token);
    }
    
    private Expression ParseString()
    {
        var token = Advance();
        return new LiteralExpression(token.Value, TokenType.String, token.Range);
    }
    
    private Expression ParseTemplateString()
    {
        var token = Advance();
        return new LiteralExpression(token.Value, TokenType.TemplateString, token.Range);
    }
    
    private Expression ParseIdentifier()
    {
        var token = Advance();
        return new IdentifierExpression(token.Value, token.Range);
    }
    
    private Expression ParseGrouping()
    {
        Advance(); // consume '('
        var expr = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after expression");
        return expr;
    }
    
    private Expression ParseArray()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume '['
        
        var elements = new List<Expression>();
        
        if (!Check(TokenType.RightBracket))
        {
            do
            {
                if (Match(TokenType.Spread))
                {
                    var spreadStart = PreviousToken.Range.Start;
                    var argument = ParseExpression();
                    elements.Add(new SpreadExpression(argument, new SourceRange(spreadStart, argument.Range.End)));
                }
                else
                {
                    elements.Add(ParseExpression());
                }
            }
            while (Match(TokenType.Comma));
        }
        
        var endToken = Consume(TokenType.RightBracket, "Expected ']' after array elements");
        
        return new ArrayExpression(elements, new SourceRange(start, endToken.Range.End));
    }
    
    private Expression ParseObject()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume '{'
        
        var properties = new List<ObjectProperty>();
        
        if (!Check(TokenType.RightBrace))
        {
            do
            {
                // Handle spread syntax in objects
                if (Match(TokenType.Spread))
                {
                    var spreadStart = PreviousToken.Range.Start;
                    var argument = ParseExpression();
                    var spreadExpr = new SpreadExpression(argument, new SourceRange(spreadStart, argument.Range.End));
                    // Use a dummy identifier key for spread expressions - the evaluator checks the Value type
                    var dummyKey = new IdentifierExpression("__spread__", new SourceRange(spreadStart, spreadStart));
                    properties.Add(new ObjectProperty(dummyKey, spreadExpr, false));
                    continue;
                }
                
                Expression key;
                bool isComputed = false;
                
                if (Match(TokenType.LeftBracket))
                {
                    key = ParseExpression();
                    Consume(TokenType.RightBracket, "Expected ']' after computed property key");
                    isComputed = true;
                }
                else if (Check(TokenType.Identifier) || Check(TokenType.String))
                {
                    var token = Advance();
                    key = token.Type == TokenType.String 
                        ? new LiteralExpression(token.Value, TokenType.String, token.Range)
                        : new IdentifierExpression(token.Value, token.Range);
                }
                else
                {
                    throw new ParseException("Expected property name", CurrentToken);
                }
                
                Consume(TokenType.Colon, "Expected ':' after property key");
                var value = ParseExpression();
                
                properties.Add(new ObjectProperty(key, value, isComputed));
            }
            while (Match(TokenType.Comma));
        }
        
        var endToken = Consume(TokenType.RightBrace, "Expected '}' after object properties");
        
        return new ObjectExpression(properties, new SourceRange(start, endToken.Range.End));
    }
    
    private FunctionExpression ParseFunctionExpression()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume 'function'
        
        string? name = null;
        if (Check(TokenType.Identifier))
        {
            name = Advance().Value;
        }
        
        var parameters = ParseParameterList();
        var body = ParseBlockStatement();
        
        return new FunctionExpression(name, parameters, body, new SourceRange(start, body.Range.End));
    }
    
    private ArrayPattern ParseArrayPattern()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume '['
        
        var elements = new List<Expression?>();
        Expression? restElement = null;
        
        if (!Check(TokenType.RightBracket))
        {
            do
            {
                if (Match(TokenType.Spread))
                {
                    // Rest element
                    var restStart = PreviousToken.Range.Start;
                    if (Check(TokenType.Identifier))
                    {
                        var name = Advance().Value;
                        restElement = new IdentifierExpression(name, new SourceRange(restStart, PreviousToken.Range.End));
                    }
                    else
                    {
                        throw new ParseException("Expected identifier after rest operator", CurrentToken);
                    }
                    break; // Rest element must be last
                }
                else if (Check(TokenType.Comma))
                {
                    // Hole in pattern
                    elements.Add(null);
                }
                else if (Check(TokenType.Identifier))
                {
                    var name = Advance().Value;
                    elements.Add(new IdentifierExpression(name, PreviousToken.Range));
                }
                else
                {
                    throw new ParseException("Expected identifier in array pattern", CurrentToken);
                }
            }
            while (Match(TokenType.Comma));
        }
        
        var endToken = Consume(TokenType.RightBracket, "Expected ']' after array pattern");
        return new ArrayPattern(elements, restElement, new SourceRange(start, endToken.Range.End));
    }
    
    private ObjectPattern ParseObjectPattern()
    {
        var start = CurrentToken.Range.Start;
        Advance(); // consume '{'
        
        var properties = new List<PropertyPattern>();
        Expression? restElement = null;
        
        if (!Check(TokenType.RightBrace))
        {
            do
            {
                if (Match(TokenType.Spread))
                {
                    // Rest element
                    var restStart = PreviousToken.Range.Start;
                    if (Check(TokenType.Identifier))
                    {
                        var name = Advance().Value;
                        restElement = new IdentifierExpression(name, new SourceRange(restStart, PreviousToken.Range.End));
                    }
                    else
                    {
                        throw new ParseException("Expected identifier after rest operator", CurrentToken);
                    }
                    break; // Rest element must be last
                }
                
                var key = Consume(TokenType.Identifier, "Expected property name").Value;
                var keyExpr = new IdentifierExpression(key, PreviousToken.Range);
                Expression value = keyExpr; // Default to same name
                bool isShorthand = true;
                
                if (Match(TokenType.Colon))
                {
                    isShorthand = false;
                    if (Check(TokenType.Identifier))
                    {
                        var valueName = Advance().Value;
                        value = new IdentifierExpression(valueName, PreviousToken.Range);
                    }
                    else
                    {
                        throw new ParseException("Expected identifier after ':' in object pattern", CurrentToken);
                    }
                }
                
                properties.Add(new PropertyPattern(keyExpr, value, false, isShorthand));
            }
            while (Match(TokenType.Comma));
        }
        
        var endToken = Consume(TokenType.RightBrace, "Expected '}' after object pattern");
        return new ObjectPattern(properties, restElement, new SourceRange(start, endToken.Range.End));
    }
    
    // Utility methods
    
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }
    
    private bool Check(TokenType type) => !IsAtEnd() && CurrentToken.Type == type;
    
    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return PreviousToken;
    }
    
    private bool IsAtEnd() => _current >= _tokens.Count || CurrentToken.Type == TokenType.EOF;
    
    private Token CurrentToken => _current < _tokens.Count ? _tokens[_current] : new Token(TokenType.EOF, "", new SourceRange(new SourcePosition(1, 1, 0), new SourcePosition(1, 1, 0)));
    
    private Token PreviousToken => _tokens[_current - 1];
    
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        
        throw new ParseException(message, CurrentToken);
    }
    
    private void Synchronize()
    {
        Advance();
        
        while (!IsAtEnd())
        {
            if (PreviousToken.Type == TokenType.Semicolon) return;
            
            if (CurrentToken.Type is TokenType.Function or TokenType.Let or TokenType.Const 
                or TokenType.For or TokenType.If or TokenType.While or TokenType.Return)
            {
                return;
            }
            
            Advance();
        }
    }
}
