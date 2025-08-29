using System;
using System.Collections.Generic;
using NovaLang.Lexer;

namespace NovaLang.AST;

/// <summary>
/// Expression statement (expression used as statement)
/// </summary>
public class ExpressionStatement : Statement
{
    public Expression Expression { get; }
    
    public ExpressionStatement(Expression expression, SourceRange range) : base(range)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Variable declarator (single variable in declaration)
/// </summary>
public class VariableDeclarator
{
    public string? Name { get; } // For simple identifiers
    public Expression? Pattern { get; } // For destructuring patterns
    public Expression? Init { get; }
    
    // Constructor for simple variable declarations
    public VariableDeclarator(string name, Expression? init = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Pattern = null;
        Init = init;
    }
    
    // Constructor for destructuring declarations
    public VariableDeclarator(Expression pattern, Expression? init = null)
    {
        Name = null;
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Init = init;
    }
    
    public bool IsDestructuring => Pattern != null;
}

/// <summary>
/// Variable declaration statement (let/const declarations)
/// </summary>
public class VariableDeclaration : Statement
{
    public TokenType Kind { get; } // Let or Const
    public IReadOnlyList<VariableDeclarator> Declarations { get; }
    
    public VariableDeclaration(TokenType kind, IReadOnlyList<VariableDeclarator> declarations, SourceRange range) : base(range)
    {
        Kind = kind;
        Declarations = declarations ?? throw new ArgumentNullException(nameof(declarations));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Function declaration statement
/// </summary>
public class FunctionDeclaration : Statement
{
    public string Name { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public BlockStatement Body { get; }
    
    public FunctionDeclaration(string name, IReadOnlyList<Parameter> parameters, BlockStatement body, SourceRange range) : base(range)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Block statement ({ ... })
/// </summary>
public class BlockStatement : Statement
{
    public IReadOnlyList<Statement> Body { get; }
    
    public BlockStatement(IReadOnlyList<Statement> body, SourceRange range) : base(range)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// If statement (if/else)
/// </summary>
public class IfStatement : Statement
{
    public Expression Test { get; }
    public Statement Consequent { get; }
    public Statement? Alternate { get; }
    
    public IfStatement(Expression test, Statement consequent, Statement? alternate, SourceRange range) : base(range)
    {
        Test = test ?? throw new ArgumentNullException(nameof(test));
        Consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
        Alternate = alternate;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// While loop statement
/// </summary>
public class WhileStatement : Statement
{
    public Expression Test { get; }
    public Statement Body { get; }
    
    public WhileStatement(Expression test, Statement body, SourceRange range) : base(range)
    {
        Test = test ?? throw new ArgumentNullException(nameof(test));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// For loop statement
/// </summary>
public class ForStatement : Statement
{
    public AstNode? Init { get; } // VariableDeclaration or Expression
    public Expression? Test { get; }
    public Expression? Update { get; }
    public Statement Body { get; }
    
    public ForStatement(AstNode? init, Expression? test, Expression? update, Statement body, SourceRange range) : base(range)
    {
        Init = init;
        Test = test;
        Update = update;
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Return statement
/// </summary>
public class ReturnStatement : Statement
{
    public Expression? Argument { get; }
    
    public ReturnStatement(Expression? argument, SourceRange range) : base(range)
    {
        Argument = argument;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Break statement
/// </summary>
public class BreakStatement : Statement
{
    public BreakStatement(SourceRange range) : base(range) { }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Continue statement
/// </summary>
public class ContinueStatement : Statement
{
    public ContinueStatement(SourceRange range) : base(range) { }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Switch case clause
/// </summary>
public class SwitchCase
{
    public Expression? Test { get; } // null for default case
    public IReadOnlyList<Statement> Consequent { get; }
    
    public SwitchCase(Expression? test, IReadOnlyList<Statement> consequent)
    {
        Test = test;
        Consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
    }
}

/// <summary>
/// Switch statement
/// </summary>
public class SwitchStatement : Statement
{
    public Expression Discriminant { get; }
    public IReadOnlyList<SwitchCase> Cases { get; }
    
    public SwitchStatement(Expression discriminant, IReadOnlyList<SwitchCase> cases, SourceRange range) : base(range)
    {
        Discriminant = discriminant ?? throw new ArgumentNullException(nameof(discriminant));
        Cases = cases ?? throw new ArgumentNullException(nameof(cases));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Catch clause in try statement
/// </summary>
public class CatchClause
{
    public string? Parameter { get; } // Exception variable name
    public BlockStatement Body { get; }
    
    public CatchClause(string? parameter, BlockStatement body)
    {
        Parameter = parameter;
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}

/// <summary>
/// Try statement (try/catch/finally)
/// </summary>
public class TryStatement : Statement
{
    public BlockStatement Block { get; }
    public CatchClause? Handler { get; }
    public BlockStatement? Finalizer { get; }
    
    public TryStatement(BlockStatement block, CatchClause? handler, BlockStatement? finalizer, SourceRange range) : base(range)
    {
        Block = block ?? throw new ArgumentNullException(nameof(block));
        Handler = handler;
        Finalizer = finalizer;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Throw statement
/// </summary>
public class ThrowStatement : Statement
{
    public Expression Argument { get; }
    
    public ThrowStatement(Expression argument, SourceRange range) : base(range)
    {
        Argument = argument ?? throw new ArgumentNullException(nameof(argument));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}
