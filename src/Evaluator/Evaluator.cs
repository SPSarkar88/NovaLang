using NovaLang.AST;
using NovaLang.Runtime;
using System.Text;
using NovaLang.Lexer;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NovaLang.Evaluator;

/// <summary>
/// Evaluator for the NovaLang AST
/// </summary>
public class Evaluator : IAstVisitor<NovaValue>
{
    private Runtime.Environment _environment;

    public Evaluator(Runtime.Environment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    // Program visitor
    public NovaValue Visit(AstProgram program)
    {
        NovaValue result = UndefinedValue.Instance;
        
        foreach (var statement in program.Body)
        {
            result = statement.Accept(this);
        }
        
        return result;
    }

    // Expression visitors
    public NovaValue Visit(LiteralExpression literal)
    {
        return literal.Value switch
        {
            null => NullValue.Instance,
            bool b => new BooleanValue(b),
            double d => new NumberValue(d),
            string s when literal.LiteralType == TokenType.TemplateString => 
                new StringValue(EvaluateTemplateString(s)),
            string s => new StringValue(s),
            _ => throw new RuntimeException($"Unsupported literal type: {literal.Value.GetType().Name}")
        };
    }

    public NovaValue Visit(IdentifierExpression identifier)
    {
        return _environment.Get(identifier.Name);
    }

    public NovaValue Visit(BinaryExpression binary)
    {
        var left = binary.Left.Accept(this);
        
        // Handle short-circuit evaluation for logical operators
        if (binary.Operator == TokenType.And)
        {
            if (!IsTruthy(left))
                return left;
            return binary.Right.Accept(this);
        }
        
        if (binary.Operator == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
            return binary.Right.Accept(this);
        }
        
        var right = binary.Right.Accept(this);
        
        return binary.Operator switch
        {
            TokenType.Plus => Add(left, right),
            TokenType.Minus => Subtract(left, right),
            TokenType.Star => Multiply(left, right),
            TokenType.Slash => Divide(left, right),
            TokenType.Percent => Modulo(left, right),
            TokenType.Equal => new BooleanValue(IsEqual(left, right)),
            TokenType.NotEqual => new BooleanValue(!IsEqual(left, right)),
            TokenType.Less => new BooleanValue(Compare(left, right) < 0),
            TokenType.LessEqual => new BooleanValue(Compare(left, right) <= 0),
            TokenType.Greater => new BooleanValue(Compare(left, right) > 0),
            TokenType.GreaterEqual => new BooleanValue(Compare(left, right) >= 0),
            _ => throw new RuntimeException($"Unknown binary operator: {binary.Operator}")
        };
    }

    public NovaValue Visit(UnaryExpression unary)
    {
        var operand = unary.Operand.Accept(this);
        
        return unary.Operator switch
        {
            TokenType.Minus => Negate(operand),
            TokenType.Not => new BooleanValue(!IsTruthy(operand)),
            TokenType.Plus => Plus(operand),
            _ => throw new RuntimeException($"Unknown unary operator: {unary.Operator}")
        };
    }

    public NovaValue Visit(CallExpression call)
    {
        var callee = call.Callee.Accept(this);
        
        if (!callee.IsCallable)
        {
            throw new RuntimeException($"'{callee}' is not a function");
        }
        
        var arguments = call.Arguments.Select(arg => arg.Accept(this)).ToArray();
        
        return callee.Call(arguments, _environment);
    }

    public NovaValue Visit(MemberExpression member)
    {
        var obj = member.Object.Accept(this);
        
        if (member.IsComputed)
        {
            // Handle computed member access: obj[prop]
            var prop = member.Property.Accept(this);
            return GetProperty(obj, prop);
        }
        else
        {
            // Handle dot notation: obj.prop
            if (member.Property is not IdentifierExpression identifier)
            {
                throw new RuntimeException("Property must be an identifier for dot notation");
            }
            
            return GetProperty(obj, new StringValue(identifier.Name));
        }
    }

    public NovaValue Visit(ArrayExpression array)
    {
        var elements = new List<NovaValue>();
        
        foreach (var element in array.Elements)
        {
            if (element is SpreadExpression spread)
            {
                // Handle spread syntax: ...iterable
                var value = spread.Argument.Accept(this);
                if (value is ArrayValue spreadArray)
                {
                    elements.AddRange(spreadArray.Elements);
                }
                else
                {
                    throw new RuntimeException("Spread syntax can only be applied to arrays");
                }
            }
            else
            {
                elements.Add(element.Accept(this));
            }
        }
        
        return new ArrayValue(elements);
    }

    public NovaValue Visit(ObjectExpression obj)
    {
        var properties = new Dictionary<string, NovaValue>();
        
        foreach (var property in obj.Properties)
        {
            // Check if this is a spread expression
            if (property.Value is SpreadExpression spread)
            {
                // Handle spread syntax: {...other}
                var value = spread.Argument.Accept(this);
                if (value is ObjectValue spreadObject)
                {
                    // Copy all properties from the spread object
                    foreach (var kvp in spreadObject.Properties)
                    {
                        properties[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    throw new RuntimeException("Spread syntax can only be applied to objects in object literals");
                }
            }
            else
            {
                // Regular property
                string key;
                if (property.Key is IdentifierExpression identifier)
                {
                    key = identifier.Name;
                }
                else if (property.Key is LiteralExpression literal && literal.Value is string str)
                {
                    key = str;
                }
                else
                {
                    throw new RuntimeException("Object keys must be identifiers or string literals");
                }
                
                var value = property.Value.Accept(this);
                properties[key] = value;
            }
        }
        
        return new ObjectValue(properties);
    }

    public NovaValue Visit(FunctionExpression function)
    {
        var parameters = function.Parameters.Select(p => p.Name).ToList();
        return new FunctionValue(parameters, function.Body, _environment);
    }

    public NovaValue Visit(ArrowFunctionExpression arrowFunction)
    {
        var parameters = arrowFunction.Parameters.Select(p => p.Name).ToList();
        
        // Handle arrow function body - can be expression or block statement
        if (arrowFunction.Body is Expression expr)
        {
            // Single expression arrow function
            var blockBody = new BlockStatement(new List<Statement> 
            { 
                new ReturnStatement(expr, expr.Range) 
            }, arrowFunction.Body.Range);
            return new FunctionValue(parameters, blockBody, _environment);
        }
        else if (arrowFunction.Body is BlockStatement blockStmt)
        {
            return new FunctionValue(parameters, blockStmt, _environment);
        }
        else
        {
            throw new RuntimeException("Invalid arrow function body type");
        }
    }

    public NovaValue Visit(ConditionalExpression conditional)
    {
        var test = conditional.Test.Accept(this);
        
        if (IsTruthy(test))
        {
            return conditional.Consequent.Accept(this);
        }
        else
        {
            return conditional.Alternate.Accept(this);
        }
    }

    public NovaValue Visit(AssignmentExpression assignment)
    {
        var value = assignment.Right.Accept(this);
        
        if (assignment.Left is IdentifierExpression identifier)
        {
            _environment.Assign(identifier.Name, value);
        }
        else if (assignment.Left is MemberExpression member)
        {
            var obj = member.Object.Accept(this);
            
            if (member.IsComputed)
            {
                var prop = member.Property.Accept(this);
                SetProperty(obj, prop, value);
            }
            else
            {
                if (member.Property is not IdentifierExpression propId)
                {
                    throw new RuntimeException("Property must be an identifier for dot notation");
                }
                SetProperty(obj, new StringValue(propId.Name), value);
            }
        }
        else
        {
            // Handle destructuring assignment
            DestructureAssign(assignment.Left, value);
        }
        
        return value;
    }

    public NovaValue Visit(LogicalExpression logical)
    {
        var left = logical.Left.Accept(this);
        
        if (logical.Operator == TokenType.And)
        {
            if (!IsTruthy(left))
                return left;
            return logical.Right.Accept(this);
        }
        else if (logical.Operator == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
            return logical.Right.Accept(this);
        }
        
        throw new RuntimeException($"Unknown logical operator: {logical.Operator}");
    }

    public NovaValue Visit(SpreadExpression spread)
    {
        // Spread expressions are handled in context (array/object expressions)
        throw new RuntimeException("Spread expressions cannot be evaluated directly");
    }

    public NovaValue Visit(TemplateLiteralExpression templateLiteral)
    {
        var sb = new StringBuilder();
        
        // Template literals alternate between string parts (quasis) and expressions
        for (int i = 0; i < templateLiteral.Quasis.Count; i++)
        {
            sb.Append(templateLiteral.Quasis[i]);
            
            if (i < templateLiteral.Expressions.Count)
            {
                var exprResult = templateLiteral.Expressions[i].Accept(this);
                sb.Append(exprResult.ToString());
            }
        }
        
        return new StringValue(sb.ToString());
    }

    // Add template string evaluation method
    private string EvaluateTemplateString(string template)
    {
        var result = new StringBuilder();
        var pattern = @"\$\{([^}]+)\}";
        var lastEnd = 0;
        
        foreach (Match match in Regex.Matches(template, pattern))
        {
            // Add the literal text before this interpolation
            result.Append(template.Substring(lastEnd, match.Index - lastEnd));
            
            // Extract and evaluate the expression
            var expression = match.Groups[1].Value.Trim();
            
            // Simple variable lookup for now
            try
            {
                var value = _environment.Get(expression);
                result.Append(FormatValue(value));
            }
            catch
            {
                // If variable doesn't exist, just use the literal text
                result.Append(match.Value);
            }
            
            lastEnd = match.Index + match.Length;
        }
        
        // Add any remaining literal text
        result.Append(template.Substring(lastEnd));
        
        return result.ToString();
    }

    private string FormatValue(NovaValue value)
    {
        return value switch
        {
            StringValue s => s.Value,
            NumberValue n => n.Value.ToString(CultureInfo.InvariantCulture),
            BooleanValue b => b.Value.ToString().ToLower(),
            NullValue => "null",
            UndefinedValue => "undefined",
            ArrayValue a => $"[{string.Join(", ", a.Elements.Select(FormatValue))}]",
            ObjectValue o => $"{{ {string.Join(", ", o.Properties.Select(p => $"{p.Key}: {FormatValue(p.Value)}"))} }}",
            _ => value.ToString()
        };
    }

    public NovaValue Visit(ArrayPattern arrayPattern)
    {
        // Array patterns are used for destructuring, not direct evaluation
        throw new RuntimeException("Array pattern cannot be evaluated directly");
    }

    public NovaValue Visit(ObjectPattern objectPattern)
    {
        // Object patterns are used for destructuring, not direct evaluation
        throw new RuntimeException("Object pattern cannot be evaluated directly");
    }

    // Statement visitors
    public NovaValue Visit(ExpressionStatement expression)
    {
        return expression.Expression.Accept(this);
    }

    public NovaValue Visit(VariableDeclaration variable)
    {
        NovaValue result = UndefinedValue.Instance;
        
        foreach (var declarator in variable.Declarations)
        {
            var value = declarator.Init?.Accept(this) ?? UndefinedValue.Instance;
            
            if (declarator.Name != null)
            {
                // Simple variable declaration
                _environment.Define(declarator.Name, value);
            }
            else if (declarator.Pattern != null)
            {
                // Destructuring declaration
                DestructureAssign(declarator.Pattern, value);
            }
            
            result = value;
        }
        
        return result;
    }

    public NovaValue Visit(FunctionDeclaration function)
    {
        var parameters = function.Parameters.Select(p => p.Name).ToList();
        var functionValue = new FunctionValue(parameters, function.Body, _environment);
        _environment.Define(function.Name, functionValue);
        return functionValue;
    }

    public NovaValue Visit(BlockStatement block)
    {
        var previous = _environment;
        _environment = new Runtime.Environment(previous);
        
        try
        {
            NovaValue result = UndefinedValue.Instance;
            
            foreach (var statement in block.Body)
            {
                result = statement.Accept(this);
            }
            
            return result;
        }
        finally
        {
            _environment = previous;
        }
    }

    public NovaValue Visit(IfStatement ifStatement)
    {
        var test = ifStatement.Test.Accept(this);
        
        if (IsTruthy(test))
        {
            return ifStatement.Consequent.Accept(this);
        }
        else if (ifStatement.Alternate != null)
        {
            return ifStatement.Alternate.Accept(this);
        }
        
        return UndefinedValue.Instance;
    }

    public NovaValue Visit(WhileStatement whileStatement)
    {
        NovaValue result = UndefinedValue.Instance;
        
        while (true)
        {
            var test = whileStatement.Test.Accept(this);
            if (!IsTruthy(test))
                break;
                
            try
            {
                result = whileStatement.Body.Accept(this);
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                continue;
            }
        }
        
        return result;
    }

    public NovaValue Visit(ForStatement forStatement)
    {
        var previous = _environment;
        _environment = new Runtime.Environment(previous);
        
        try
        {
            NovaValue result = UndefinedValue.Instance;
            
            // Initialize
            if (forStatement.Init != null)
            {
                forStatement.Init.Accept(this);
            }
            
            while (true)
            {
                // Test condition
                if (forStatement.Test != null)
                {
                    var test = forStatement.Test.Accept(this);
                    if (!IsTruthy(test))
                        break;
                }
                
                // Execute body
                try
                {
                    result = forStatement.Body.Accept(this);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    // Continue to update section
                }
                
                // Update
                if (forStatement.Update != null)
                {
                    forStatement.Update.Accept(this);
                }
            }
            
            return result;
        }
        finally
        {
            _environment = previous;
        }
    }

    public NovaValue Visit(ReturnStatement returnStatement)
    {
        var value = returnStatement.Argument?.Accept(this) ?? UndefinedValue.Instance;
        throw new ReturnException(value);
    }

    public NovaValue Visit(BreakStatement breakStatement)
    {
        throw BreakException.Instance;
    }

    public NovaValue Visit(ContinueStatement continueStatement)
    {
        throw ContinueException.Instance;
    }

    public NovaValue Visit(SwitchStatement switchStatement)
    {
        var discriminant = switchStatement.Discriminant.Accept(this);
        NovaValue result = UndefinedValue.Instance;
        bool matched = false;
        bool fallthrough = false;
        
        try
        {
            foreach (var switchCase in switchStatement.Cases)
            {
                if (switchCase.Test == null) // default case
                {
                    if (!matched && !fallthrough)
                        continue;
                    matched = true;
                    fallthrough = true;
                }
                else if (!matched && !fallthrough)
                {
                    var testValue = switchCase.Test.Accept(this);
                    if (!IsEqual(discriminant, testValue))
                        continue;
                    matched = true;
                    fallthrough = true;
                }
                
                if (matched || fallthrough)
                {
                    foreach (var statement in switchCase.Consequent)
                    {
                        result = statement.Accept(this);
                    }
                }
            }
        }
        catch (BreakException)
        {
            // Normal switch break
        }
        
        return result;
    }

    public NovaValue Visit(TryStatement tryStatement)
    {
        try
        {
            return tryStatement.Block.Accept(this);
        }
        catch (RuntimeException ex) when (tryStatement.Handler != null)
        {
            var previous = _environment;
            _environment = new Runtime.Environment(previous);
            
            try
            {
                if (tryStatement.Handler.Parameter != null)
                {
                    _environment.Define(tryStatement.Handler.Parameter, new StringValue(ex.Message));
                }
                
                return tryStatement.Handler.Body.Accept(this);
            }
            finally
            {
                _environment = previous;
            }
        }
        finally
        {
            if (tryStatement.Finalizer != null)
            {
                tryStatement.Finalizer.Accept(this);
            }
        }
    }

    public NovaValue Visit(ThrowStatement throwStatement)
    {
        var value = throwStatement.Argument.Accept(this);
        throw new RuntimeException(value.ToString());
    }

    // Helper methods
    private bool IsTruthy(NovaValue value)
    {
        return value switch
        {
            BooleanValue b => b.Value,
            NullValue => false,
            UndefinedValue => false,
            NumberValue n => n.Value != 0 && !double.IsNaN(n.Value),
            StringValue s => !string.IsNullOrEmpty(s.Value),
            _ => true
        };
    }

    private NovaValue Add(NovaValue left, NovaValue right)
    {
        // String concatenation
        if (left is StringValue || right is StringValue)
        {
            return new StringValue(left.ToString() + right.ToString());
        }
        
        // Numeric addition
        if (left is NumberValue ln && right is NumberValue rn)
        {
            return new NumberValue(ln.Value + rn.Value);
        }
        
        throw new RuntimeException($"Cannot add {left.GetType().Name} and {right.GetType().Name}");
    }

    private NovaValue Subtract(NovaValue left, NovaValue right)
    {
        if (left is NumberValue ln && right is NumberValue rn)
        {
            return new NumberValue(ln.Value - rn.Value);
        }
        
        throw new RuntimeException($"Cannot subtract {right.GetType().Name} from {left.GetType().Name}");
    }

    private NovaValue Multiply(NovaValue left, NovaValue right)
    {
        if (left is NumberValue ln && right is NumberValue rn)
        {
            return new NumberValue(ln.Value * rn.Value);
        }
        
        throw new RuntimeException($"Cannot multiply {left.GetType().Name} and {right.GetType().Name}");
    }

    private NovaValue Divide(NovaValue left, NovaValue right)
    {
        if (left is NumberValue ln && right is NumberValue rn)
        {
            if (rn.Value == 0)
            {
                return new NumberValue(double.PositiveInfinity);
            }
            return new NumberValue(ln.Value / rn.Value);
        }
        
        throw new RuntimeException($"Cannot divide {left.GetType().Name} by {right.GetType().Name}");
    }

    private NovaValue Modulo(NovaValue left, NovaValue right)
    {
        if (left is NumberValue ln && right is NumberValue rn)
        {
            return new NumberValue(ln.Value % rn.Value);
        }
        
        throw new RuntimeException($"Cannot modulo {left.GetType().Name} by {right.GetType().Name}");
    }

    private NovaValue Negate(NovaValue operand)
    {
        if (operand is NumberValue n)
        {
            return new NumberValue(-n.Value);
        }
        
        throw new RuntimeException($"Cannot negate {operand.GetType().Name}");
    }

    private NovaValue Plus(NovaValue operand)
    {
        if (operand is NumberValue n)
        {
            return n;
        }
        
        if (operand is StringValue s && double.TryParse(s.Value, out var result))
        {
            return new NumberValue(result);
        }
        
        return new NumberValue(double.NaN);
    }

    private bool IsEqual(NovaValue left, NovaValue right)
    {
        if (left.GetType() != right.GetType())
            return false;
            
        return left switch
        {
            BooleanValue lb when right is BooleanValue rb => lb.Value == rb.Value,
            NumberValue ln when right is NumberValue rn => ln.Value == rn.Value,
            StringValue ls when right is StringValue rs => ls.Value == rs.Value,
            NullValue when right is NullValue => true,
            UndefinedValue when right is UndefinedValue => true,
            _ => false
        };
    }

    private int Compare(NovaValue left, NovaValue right)
    {
        if (left is NumberValue ln && right is NumberValue rn)
        {
            return ln.Value.CompareTo(rn.Value);
        }
        
        if (left is StringValue ls && right is StringValue rs)
        {
            return string.Compare(ls.Value, rs.Value, StringComparison.Ordinal);
        }
        
        throw new RuntimeException($"Cannot compare {left.GetType().Name} and {right.GetType().Name}");
    }

    private NovaValue GetProperty(NovaValue obj, NovaValue prop)
    {
        if (obj is ObjectValue objectValue)
        {
            var key = prop.ToString();
            return objectValue[key];
        }
        
        if (obj is ArrayValue arrayValue && prop is NumberValue index)
        {
            var idx = (int)index.Value;
            if (idx >= 0 && idx < arrayValue.Elements.Count)
            {
                return arrayValue.Elements[idx];
            }
            return UndefinedValue.Instance;
        }
        
        if (obj is StringValue stringValue && prop is NumberValue stringIndex)
        {
            var idx = (int)stringIndex.Value;
            if (idx >= 0 && idx < stringValue.Value.Length)
            {
                return new StringValue(stringValue.Value[idx].ToString());
            }
            return UndefinedValue.Instance;
        }
        
        return UndefinedValue.Instance;
    }

    private void SetProperty(NovaValue obj, NovaValue prop, NovaValue value)
    {
        if (obj is ObjectValue objectValue)
        {
            var key = prop.ToString();
            objectValue[key] = value;
            return;
        }
        
        if (obj is ArrayValue arrayValue && prop is NumberValue index)
        {
            var idx = (int)index.Value;
            if (idx >= 0)
            {
                // Extend array if necessary
                while (arrayValue.Elements.Count <= idx)
                {
                    arrayValue.Add(UndefinedValue.Instance);
                }
                arrayValue[idx] = value;
            }
            return;
        }
        
        throw new RuntimeException($"Cannot set property on {obj.GetType().Name}");
    }

    private void DestructureAssign(Expression pattern, NovaValue value)
    {
        switch (pattern)
        {
            case ArrayPattern arrayPattern:
                if (value is not ArrayValue arrayValue)
                {
                    throw new RuntimeException("Cannot destructure non-array value");
                }
                
                for (int i = 0; i < arrayPattern.Elements.Count; i++)
                {
                    var element = arrayPattern.Elements[i];
                    if (element == null) continue; // Handle holes
                    
                    var elementValue = i < arrayValue.Elements.Count ? arrayValue.Elements[i] : UndefinedValue.Instance;
                    
                    if (element is IdentifierExpression identifier)
                    {
                        _environment.Define(identifier.Name, elementValue);
                    }
                    else
                    {
                        DestructureAssign(element, elementValue);
                    }
                }
                
                // Handle rest element if present
                if (arrayPattern.RestElement is IdentifierExpression restIdentifier)
                {
                    // Collect remaining elements starting from where we left off
                    var remainingElements = new List<NovaValue>();
                    for (int i = arrayPattern.Elements.Count; i < arrayValue.Elements.Count; i++)
                    {
                        remainingElements.Add(arrayValue.Elements[i]);
                    }
                    var restArray = new ArrayValue(remainingElements);
                    _environment.Define(restIdentifier.Name, restArray);
                }
                break;
                
            case ObjectPattern objectPattern:
                if (value is not ObjectValue objectValue)
                {
                    throw new RuntimeException("Cannot destructure non-object value");
                }
                
                foreach (var property in objectPattern.Properties)
                {
                    string key;
                    if (property.Key is IdentifierExpression identifier)
                    {
                        key = identifier.Name;
                    }
                    else if (property.Key is LiteralExpression literal && literal.Value is string str)
                    {
                        key = str;
                    }
                    else
                    {
                        throw new RuntimeException("Object destructuring keys must be identifiers or string literals");
                    }
                    
                    var propertyValue = objectValue[key];
                    
                    if (property.Value is IdentifierExpression valueIdentifier)
                    {
                        _environment.Define(valueIdentifier.Name, propertyValue);
                    }
                    else
                    {
                        DestructureAssign(property.Value, propertyValue);
                    }
                }
                break;
                
            case IdentifierExpression identifier:
                _environment.Define(identifier.Name, value);
                break;
                
            default:
                throw new RuntimeException($"Unsupported destructuring pattern: {pattern.GetType().Name}");
        }
    }
}
