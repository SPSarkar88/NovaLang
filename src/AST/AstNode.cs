using System;
using System.Collections.Generic;
using NovaLang.Lexer;

namespace NovaLang.AST;

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class AstNode
{
    public SourceRange Range { get; }
    
    protected AstNode(SourceRange range)
    {
        Range = range;
    }
    
    public abstract void Accept(IAstVisitor visitor);
    public abstract T Accept<T>(IAstVisitor<T> visitor);
}

/// <summary>
/// Base class for expressions
/// </summary>
public abstract class Expression : AstNode
{
    protected Expression(SourceRange range) : base(range) { }
}

/// <summary>
/// Function parameter
/// </summary>
public class Parameter
{
    public string Name { get; }
    public Expression? DefaultValue { get; }
    
    public Parameter(string name, Expression? defaultValue = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DefaultValue = defaultValue;
    }
}

/// <summary>
/// Base class for statements
/// </summary>
public abstract class Statement : AstNode
{
    protected Statement(SourceRange range) : base(range) { }
}

/// <summary>
/// Visitor interface for AST traversal
/// </summary>
public interface IAstVisitor
{
    void Visit(AstProgram program);
    void Visit(LiteralExpression literal);
    void Visit(IdentifierExpression identifier);
    void Visit(BinaryExpression binary);
    void Visit(UnaryExpression unary);
    void Visit(CallExpression call);
    void Visit(MemberExpression member);
    void Visit(ArrayExpression array);
    void Visit(ObjectExpression obj);
    void Visit(FunctionExpression function);
    void Visit(ArrowFunctionExpression arrowFunction);
    void Visit(ConditionalExpression conditional);
    void Visit(AssignmentExpression assignment);
    void Visit(LogicalExpression logical);
    void Visit(SpreadExpression spread);
    void Visit(TemplateLiteralExpression templateLiteral);
    void Visit(ArrayPattern arrayPattern);
    void Visit(ObjectPattern objectPattern);
    
    void Visit(ExpressionStatement expression);
    void Visit(VariableDeclaration variable);
    void Visit(FunctionDeclaration function);
    void Visit(BlockStatement block);
    void Visit(IfStatement ifStmt);
    void Visit(WhileStatement whileStmt);
    void Visit(ForStatement forStmt);
    void Visit(ReturnStatement returnStmt);
    void Visit(BreakStatement breakStmt);
    void Visit(ContinueStatement continueStmt);
    void Visit(SwitchStatement switchStmt);
    void Visit(TryStatement tryStmt);
    void Visit(ThrowStatement throwStmt);
}

/// <summary>
/// Generic visitor interface for AST traversal with return values
/// </summary>
public interface IAstVisitor<T>
{
    T Visit(AstProgram program);
    T Visit(LiteralExpression literal);
    T Visit(IdentifierExpression identifier);
    T Visit(BinaryExpression binary);
    T Visit(UnaryExpression unary);
    T Visit(CallExpression call);
    T Visit(MemberExpression member);
    T Visit(ArrayExpression array);
    T Visit(ObjectExpression obj);
    T Visit(FunctionExpression function);
    T Visit(ArrowFunctionExpression arrowFunction);
    T Visit(ConditionalExpression conditional);
    T Visit(AssignmentExpression assignment);
    T Visit(LogicalExpression logical);
    T Visit(SpreadExpression spread);
    T Visit(TemplateLiteralExpression templateLiteral);
    T Visit(ArrayPattern arrayPattern);
    T Visit(ObjectPattern objectPattern);
    
    T Visit(ExpressionStatement expression);
    T Visit(VariableDeclaration variable);
    T Visit(FunctionDeclaration function);
    T Visit(BlockStatement block);
    T Visit(IfStatement ifStmt);
    T Visit(WhileStatement whileStmt);
    T Visit(ForStatement forStmt);
    T Visit(ReturnStatement returnStmt);
    T Visit(BreakStatement breakStmt);
    T Visit(ContinueStatement continueStmt);
    T Visit(SwitchStatement switchStmt);
    T Visit(TryStatement tryStmt);
    T Visit(ThrowStatement throwStmt);
}

/// <summary>
/// Root node of the AST representing a program/module
/// </summary>
public class AstProgram : AstNode
{
    public IReadOnlyList<Statement> Body { get; }
    
    public AstProgram(IReadOnlyList<Statement> body, SourceRange range) : base(range)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}
