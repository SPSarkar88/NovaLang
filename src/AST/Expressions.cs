using System;
using System.Collections.Generic;
using NovaLang.Lexer;

namespace NovaLang.AST;

/// <summary>
/// Literal value expression (number, string, boolean, null, undefined)
/// </summary>
public class LiteralExpression : Expression
{
    public object? Value { get; }
    public TokenType LiteralType { get; }
    
    public LiteralExpression(object? value, TokenType literalType, SourceRange range) : base(range)
    {
        Value = value;
        LiteralType = literalType;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Identifier expression (variable reference)
/// </summary>
public class IdentifierExpression : Expression
{
    public string Name { get; }
    
    public IdentifierExpression(string name, SourceRange range) : base(range)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Binary operation expression (a + b, a == b, etc.)
/// </summary>
public class BinaryExpression : Expression
{
    public Expression Left { get; }
    public TokenType Operator { get; }
    public Expression Right { get; }
    
    public BinaryExpression(Expression left, TokenType op, Expression right, SourceRange range) : base(range)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operator = op;
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Unary operation expression (!a, -a, etc.)
/// </summary>
public class UnaryExpression : Expression
{
    public TokenType Operator { get; }
    public Expression Operand { get; }
    
    public UnaryExpression(TokenType op, Expression operand, SourceRange range) : base(range)
    {
        Operator = op;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Function call expression (fn(a, b))
/// </summary>
public class CallExpression : Expression
{
    public Expression Callee { get; }
    public IReadOnlyList<Expression> Arguments { get; }
    
    public CallExpression(Expression callee, IReadOnlyList<Expression> arguments, SourceRange range) : base(range)
    {
        Callee = callee ?? throw new ArgumentNullException(nameof(callee));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Member access expression (obj.prop, obj[key])
/// </summary>
public class MemberExpression : Expression
{
    public Expression Object { get; }
    public Expression Property { get; }
    public bool IsComputed { get; } // true for obj[key], false for obj.prop
    
    public MemberExpression(Expression obj, Expression property, bool isComputed, SourceRange range) : base(range)
    {
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
        Property = property ?? throw new ArgumentNullException(nameof(property));
        IsComputed = isComputed;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Array literal expression ([1, 2, 3])
/// </summary>
public class ArrayExpression : Expression
{
    public IReadOnlyList<Expression> Elements { get; }
    
    public ArrayExpression(IReadOnlyList<Expression> elements, SourceRange range) : base(range)
    {
        Elements = elements ?? throw new ArgumentNullException(nameof(elements));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Object property in object literal
/// </summary>
public class ObjectProperty
{
    public Expression Key { get; }
    public Expression Value { get; }
    public bool IsComputed { get; } // true for [key]: value, false for key: value
    
    public ObjectProperty(Expression key, Expression value, bool isComputed = false)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        IsComputed = isComputed;
    }
}

/// <summary>
/// Object literal expression ({a: 1, b: 2})
/// </summary>
public class ObjectExpression : Expression
{
    public IReadOnlyList<ObjectProperty> Properties { get; }
    
    public ObjectExpression(IReadOnlyList<ObjectProperty> properties, SourceRange range) : base(range)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Function expression (function(x, y) { ... })
/// </summary>
public class FunctionExpression : Expression
{
    public string? Name { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public BlockStatement Body { get; }
    
    public FunctionExpression(string? name, IReadOnlyList<Parameter> parameters, BlockStatement body, SourceRange range) : base(range)
    {
        Name = name;
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Arrow function expression ((x, y) => x + y)
/// </summary>
public class ArrowFunctionExpression : Expression
{
    public IReadOnlyList<Parameter> Parameters { get; }
    public AstNode Body { get; } // Can be Expression or BlockStatement
    public bool IsAsync { get; }
    
    public ArrowFunctionExpression(IReadOnlyList<Parameter> parameters, AstNode body, bool isAsync, SourceRange range) : base(range)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        IsAsync = isAsync;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Conditional (ternary) expression (cond ? a : b)
/// </summary>
public class ConditionalExpression : Expression
{
    public Expression Test { get; }
    public Expression Consequent { get; }
    public Expression Alternate { get; }
    
    public ConditionalExpression(Expression test, Expression consequent, Expression alternate, SourceRange range) : base(range)
    {
        Test = test ?? throw new ArgumentNullException(nameof(test));
        Consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
        Alternate = alternate ?? throw new ArgumentNullException(nameof(alternate));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Assignment expression (a = b)
/// </summary>
public class AssignmentExpression : Expression
{
    public Expression Left { get; }
    public Expression Right { get; }
    
    public AssignmentExpression(Expression left, Expression right, SourceRange range) : base(range)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Logical expression (&& ||)
/// </summary>
public class LogicalExpression : Expression
{
    public Expression Left { get; }
    public TokenType Operator { get; } // And, Or, Nullish
    public Expression Right { get; }
    
    public LogicalExpression(Expression left, TokenType op, Expression right, SourceRange range) : base(range)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operator = op;
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Spread expression (...expr)
/// </summary>
public class SpreadExpression : Expression
{
    public Expression Argument { get; }
    
    public SpreadExpression(Expression argument, SourceRange range) : base(range)
    {
        Argument = argument ?? throw new ArgumentNullException(nameof(argument));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Template literal expression with interpolation
/// </summary>
public class TemplateLiteralExpression : Expression
{
    public IReadOnlyList<string> Quasis { get; } // String parts
    public IReadOnlyList<Expression> Expressions { get; } // Interpolated expressions
    
    public TemplateLiteralExpression(IReadOnlyList<string> quasis, IReadOnlyList<Expression> expressions, SourceRange range) : base(range)
    {
        Quasis = quasis ?? throw new ArgumentNullException(nameof(quasis));
        Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Array pattern for destructuring
/// </summary>
public class ArrayPattern : Expression
{
    public IReadOnlyList<Expression?> Elements { get; } // null for holes
    public Expression? RestElement { get; } // Rest element (...rest)
    
    public ArrayPattern(IReadOnlyList<Expression?> elements, Expression? restElement, SourceRange range) : base(range)
    {
        Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        RestElement = restElement;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Object pattern for destructuring
/// </summary>
public class ObjectPattern : Expression
{
    public IReadOnlyList<PropertyPattern> Properties { get; }
    public Expression? RestElement { get; } // Rest element (...rest)
    
    public ObjectPattern(IReadOnlyList<PropertyPattern> properties, Expression? restElement, SourceRange range) : base(range)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        RestElement = restElement;
    }
    
    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Property pattern in object destructuring
/// </summary>
public class PropertyPattern
{
    public Expression Key { get; }
    public Expression Value { get; } // Can be identifier or nested pattern
    public bool IsComputed { get; }
    public bool IsShorthand { get; } // For {x} instead of {x: x}
    
    public PropertyPattern(Expression key, Expression value, bool isComputed = false, bool isShorthand = false)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        IsComputed = isComputed;
        IsShorthand = isShorthand;
    }
}
