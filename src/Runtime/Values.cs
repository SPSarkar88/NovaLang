using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NovaLang.AST;

namespace NovaLang.Runtime;

/// <summary>
/// Represents runtime values in NovaLang
/// </summary>
public abstract class NovaValue
{
    public abstract NovaValueType Type { get; }
    public abstract bool IsTruthy { get; }
    public abstract override string ToString();
    public abstract override bool Equals(object? obj);
    public abstract override int GetHashCode();
    
    public virtual bool IsCallable => false;
    public virtual NovaValue Call(NovaValue[] args, Environment env) => throw new RuntimeException($"Value of type {Type} is not callable");
}

/// <summary>
/// Types of values in NovaLang
/// </summary>
public enum NovaValueType
{
    Number,
    String,
    Boolean,
    Null,
    Undefined,
    Array,
    Object,
    Function,
    NativeFunction
}

/// <summary>
/// Number value (all numbers are doubles in NovaLang)
/// </summary>
public class NumberValue : NovaValue
{
    public double Value { get; }
    
    public NumberValue(double value)
    {
        Value = value;
    }
    
    public override NovaValueType Type => NovaValueType.Number;
    public override bool IsTruthy => Value != 0.0 && !double.IsNaN(Value);
    
    public override string ToString()
    {
        // Format integers without decimal point
        if (Math.Abs(Value % 1) < double.Epsilon && !double.IsInfinity(Value))
        {
            return ((long)Value).ToString();
        }
        return Value.ToString();
    }
    
    public override bool Equals(object? obj) => obj is NumberValue other && Value.Equals(other.Value);
    public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// String value
/// </summary>
public class StringValue : NovaValue
{
    public string Value { get; }
    
    public StringValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public override NovaValueType Type => NovaValueType.String;
    public override bool IsTruthy => !string.IsNullOrEmpty(Value);
    
    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is StringValue other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// Boolean value
/// </summary>
public class BooleanValue : NovaValue
{
    public bool Value { get; }
    
    public BooleanValue(bool value)
    {
        Value = value;
    }
    
    public override NovaValueType Type => NovaValueType.Boolean;
    public override bool IsTruthy => Value;
    
    public override string ToString() => Value ? "true" : "false";
    public override bool Equals(object? obj) => obj is BooleanValue other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    
    public static readonly BooleanValue True = new(true);
    public static readonly BooleanValue False = new(false);
    
    public static BooleanValue From(bool value) => value ? True : False;
}

/// <summary>
/// Null value
/// </summary>
public class NullValue : NovaValue
{
    private NullValue() { }
    
    public static readonly NullValue Instance = new();
    
    public override NovaValueType Type => NovaValueType.Null;
    public override bool IsTruthy => false;
    
    public override string ToString() => "null";
    public override bool Equals(object? obj) => obj is NullValue;
    public override int GetHashCode() => 0;
}

/// <summary>
/// Undefined value
/// </summary>
public class UndefinedValue : NovaValue
{
    private UndefinedValue() { }
    
    public static readonly UndefinedValue Instance = new();
    
    public override NovaValueType Type => NovaValueType.Undefined;
    public override bool IsTruthy => false;
    
    public override string ToString() => "undefined";
    public override bool Equals(object? obj) => obj is UndefinedValue;
    public override int GetHashCode() => 1;
}

/// <summary>
/// Array value
/// </summary>
public class ArrayValue : NovaValue
{
    private readonly List<NovaValue> _elements;
    
    public ArrayValue(IEnumerable<NovaValue>? elements = null)
    {
        _elements = elements?.ToList() ?? new List<NovaValue>();
    }
    
    public IReadOnlyList<NovaValue> Elements => _elements;
    public int Length => _elements.Count;
    
    public NovaValue this[int index]
    {
        get => index >= 0 && index < _elements.Count ? _elements[index] : UndefinedValue.Instance;
        set
        {
            // Extend array if necessary
            while (index >= _elements.Count)
            {
                _elements.Add(UndefinedValue.Instance);
            }
            if (index >= 0)
            {
                _elements[index] = value;
            }
        }
    }
    
    public void Add(NovaValue value) => _elements.Add(value);
    public void RemoveAt(int index) => _elements.RemoveAt(index);
    
    public override NovaValueType Type => NovaValueType.Array;
    public override bool IsTruthy => true; // Arrays are always truthy
    
    public override string ToString()
    {
        var elements = _elements.Select(e => e.ToString());
        return $"[{string.Join(", ", elements)}]";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not ArrayValue other || _elements.Count != other._elements.Count)
            return false;
        
        for (int i = 0; i < _elements.Count; i++)
        {
            if (!_elements[i].Equals(other._elements[i]))
                return false;
        }
        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var element in _elements)
        {
            hash.Add(element);
        }
        return hash.ToHashCode();
    }
}

/// <summary>
/// Object value (key-value pairs)
/// </summary>
public class ObjectValue : NovaValue
{
    private readonly Dictionary<string, NovaValue> _properties;
    
    public ObjectValue(IDictionary<string, NovaValue>? properties = null)
    {
        _properties = properties?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) 
                      ?? new Dictionary<string, NovaValue>();
    }
    
    public IReadOnlyDictionary<string, NovaValue> Properties => _properties;
    
    public NovaValue this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : UndefinedValue.Instance;
        set => _properties[key] = value;
    }
    
    public bool HasProperty(string key) => _properties.ContainsKey(key);
    public void DeleteProperty(string key) => _properties.Remove(key);
    
    public override NovaValueType Type => NovaValueType.Object;
    public override bool IsTruthy => true; // Objects are always truthy
    
    public override string ToString()
    {
        var props = _properties.Select(kvp => $"{kvp.Key}: {kvp.Value}");
        return $"{{ {string.Join(", ", props)} }}";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not ObjectValue other || _properties.Count != other._properties.Count)
            return false;
        
        foreach (var kvp in _properties)
        {
            if (!other._properties.TryGetValue(kvp.Key, out var otherValue) || !kvp.Value.Equals(otherValue))
                return false;
        }
        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in _properties.OrderBy(p => p.Key))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }
}

/// <summary>
/// Function value (user-defined functions with closures)
/// </summary>
public class FunctionValue : NovaValue
{
    public IReadOnlyList<string> Parameters { get; }
    public Statement Body { get; }
    public Environment Closure { get; }
    public string? Name { get; }
    
    public FunctionValue(IReadOnlyList<string> parameters, Statement body, Environment closure, string? name = null)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Closure = closure ?? throw new ArgumentNullException(nameof(closure));
        Name = name;
    }
    
    public override NovaValueType Type => NovaValueType.Function;
    public override bool IsTruthy => true; // Functions are always truthy
    public override bool IsCallable => true;
    
    public override NovaValue Call(NovaValue[] args, Environment env)
    {
        // Create new environment for function execution with closure as parent
        var funcEnv = new Environment(Closure);
        
        // Bind parameters to arguments
        for (int i = 0; i < Parameters.Count; i++)
        {
            var value = i < args.Length ? args[i] : UndefinedValue.Instance;
            funcEnv.Define(Parameters[i], value);
        }
        
        // Execute function body
        try
        {
            var evaluator = new NovaLang.Evaluator.Evaluator(funcEnv);
            Body.Accept(evaluator);
            
            // If no explicit return, return undefined
            return UndefinedValue.Instance;
        }
        catch (ReturnException returnEx)
        {
            return returnEx.Value;
        }
    }
    
    public override string ToString() => Name != null ? $"function {Name}" : "function";
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>
/// Native function value (built-in functions implemented in C#)
/// </summary>
public class NativeFunctionValue : NovaValue
{
    public string Name { get; }
    public Func<NovaValue[], Environment, NovaValue> Implementation { get; }
    
    public NativeFunctionValue(string name, Func<NovaValue[], Environment, NovaValue> implementation)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
    }
    
    public override NovaValueType Type => NovaValueType.NativeFunction;
    public override bool IsTruthy => true;
    public override bool IsCallable => true;
    
    public override NovaValue Call(NovaValue[] args, Environment env) => Implementation(args, env);
    
    public override string ToString() => $"native function {Name}";
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>
/// Utility methods for value operations
/// </summary>
public static class ValueOperations
{
    public static NovaValue Add(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(l.Value + r.Value),
            (StringValue l, _) => new StringValue(l.Value + right.ToString()),
            (_, StringValue r) => new StringValue(left.ToString() + r.Value),
            _ => throw new RuntimeException($"Cannot add {left.Type} and {right.Type}")
        };
    }
    
    public static NovaValue Subtract(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(l.Value - r.Value),
            _ => throw new RuntimeException($"Cannot subtract {right.Type} from {left.Type}")
        };
    }
    
    public static NovaValue Multiply(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(l.Value * r.Value),
            _ => throw new RuntimeException($"Cannot multiply {left.Type} and {right.Type}")
        };
    }
    
    public static NovaValue Divide(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(l.Value / r.Value),
            _ => throw new RuntimeException($"Cannot divide {left.Type} by {right.Type}")
        };
    }
    
    public static NovaValue Modulo(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(l.Value % r.Value),
            _ => throw new RuntimeException($"Cannot get modulo of {left.Type} and {right.Type}")
        };
    }
    
    public static NovaValue Power(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => new NumberValue(Math.Pow(l.Value, r.Value)),
            _ => throw new RuntimeException($"Cannot raise {left.Type} to power of {right.Type}")
        };
    }
    
    public static BooleanValue Equal(NovaValue left, NovaValue right)
    {
        return left.Equals(right) ? BooleanValue.True : BooleanValue.False;
    }
    
    public static BooleanValue NotEqual(NovaValue left, NovaValue right)
    {
        return left.Equals(right) ? BooleanValue.False : BooleanValue.True;
    }
    
    public static BooleanValue LessThan(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => l.Value < r.Value ? BooleanValue.True : BooleanValue.False,
            (StringValue l, StringValue r) => string.Compare(l.Value, r.Value) < 0 ? BooleanValue.True : BooleanValue.False,
            _ => throw new RuntimeException($"Cannot compare {left.Type} and {right.Type}")
        };
    }
    
    public static BooleanValue LessThanOrEqual(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => l.Value <= r.Value ? BooleanValue.True : BooleanValue.False,
            (StringValue l, StringValue r) => string.Compare(l.Value, r.Value) <= 0 ? BooleanValue.True : BooleanValue.False,
            _ => throw new RuntimeException($"Cannot compare {left.Type} and {right.Type}")
        };
    }
    
    public static BooleanValue GreaterThan(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => l.Value > r.Value ? BooleanValue.True : BooleanValue.False,
            (StringValue l, StringValue r) => string.Compare(l.Value, r.Value) > 0 ? BooleanValue.True : BooleanValue.False,
            _ => throw new RuntimeException($"Cannot compare {left.Type} and {right.Type}")
        };
    }
    
    public static BooleanValue GreaterThanOrEqual(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => l.Value >= r.Value ? BooleanValue.True : BooleanValue.False,
            (StringValue l, StringValue r) => string.Compare(l.Value, r.Value) >= 0 ? BooleanValue.True : BooleanValue.False,
            _ => throw new RuntimeException($"Cannot compare {left.Type} and {right.Type}")
        };
    }
    
    // Additional methods needed by the evaluator
    public static bool IsTruthy(NovaValue value)
    {
        return value.IsTruthy;
    }
    
    public static NovaValue Exponentiate(NovaValue left, NovaValue right)
    {
        return Power(left, right);
    }
    
    public static bool IsEqual(NovaValue left, NovaValue right)
    {
        return left.Equals(right);
    }
    
    public static bool IsLess(NovaValue left, NovaValue right)
    {
        return LessThan(left, right).Value;
    }
    
    public static bool IsLessEqual(NovaValue left, NovaValue right)
    {
        return LessThanOrEqual(left, right).Value;
    }
    
    public static bool IsGreater(NovaValue left, NovaValue right)
    {
        return GreaterThan(left, right).Value;
    }
    
    public static bool IsGreaterEqual(NovaValue left, NovaValue right)
    {
        return GreaterThanOrEqual(left, right).Value;
    }
    
    public static NovaValue Negate(NovaValue operand)
    {
        return operand switch
        {
            NumberValue n => new NumberValue(-n.Value),
            _ => throw new RuntimeException($"Cannot negate {operand.Type}")
        };
    }
    
    public static NovaValue UnaryPlus(NovaValue operand)
    {
        return operand switch
        {
            NumberValue n => n,
            _ => throw new RuntimeException($"Cannot apply unary plus to {operand.Type}")
        };
    }
    
    public static NovaValue Call(NovaValue callee, IList<NovaValue> arguments, Environment environment)
    {
        return callee switch
        {
            FunctionValue func => func.Call(arguments.ToArray(), environment),
            NativeFunctionValue nativeFunc => nativeFunc.Call(arguments.ToArray(), environment),
            _ => throw new RuntimeException($"Cannot call {callee.Type}")
        };
    }
    
    public static NovaValue GetProperty(NovaValue obj, NovaValue property)
    {
        return obj switch
        {
            ObjectValue objVal => objVal[property.ToString()],
            ArrayValue arrVal when property is NumberValue numProp => 
                (int)numProp.Value < arrVal.Elements.Count ? arrVal.Elements[(int)numProp.Value] : UndefinedValue.Instance,
            _ => throw new RuntimeException($"Cannot get property from {obj.Type}")
        };
    }
    
    public static void SetProperty(NovaValue obj, NovaValue property, NovaValue value)
    {
        switch (obj)
        {
            case ObjectValue objVal:
                objVal[property.ToString()] = value;
                break;
            default:
                throw new RuntimeException($"Cannot set property on {obj.Type}");
        }
    }
}
