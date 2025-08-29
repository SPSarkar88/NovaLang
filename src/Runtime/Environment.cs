using System;
using System.Collections.Generic;

namespace NovaLang.Runtime;

/// <summary>
/// Represents a lexical environment/scope for variable bindings
/// </summary>
public class Environment
{
    private readonly Dictionary<string, NovaValue> _bindings = new();
    private readonly Environment? _parent;
    
    public Environment(Environment? parent = null)
    {
        _parent = parent;
    }
    
    /// <summary>
    /// Defines a new variable in this environment
    /// </summary>
    public void Define(string name, NovaValue value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        
        _bindings[name] = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    /// <summary>
    /// Gets a variable value from this environment or parent environments
    /// </summary>
    public NovaValue Get(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        
        if (_bindings.TryGetValue(name, out var value))
            return value;
        
        if (_parent != null)
            return _parent.Get(name);
        
        throw new RuntimeException($"Undefined variable '{name}'");
    }
    
    /// <summary>
    /// Assigns a value to an existing variable in this environment or parent environments
    /// </summary>
    public void Assign(string name, NovaValue value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        
        if (_bindings.ContainsKey(name))
        {
            _bindings[name] = value ?? throw new ArgumentNullException(nameof(value));
            return;
        }
        
        if (_parent != null)
        {
            _parent.Assign(name, value);
            return;
        }
        
        throw new RuntimeException($"Undefined variable '{name}'");
    }
    
    /// <summary>
    /// Checks if a variable is defined in this environment or parent environments
    /// </summary>
    public bool IsDefined(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        
        return _bindings.ContainsKey(name) || (_parent?.IsDefined(name) ?? false);
    }
    
    /// <summary>
    /// Creates a new child environment
    /// </summary>
    public Environment CreateChild()
    {
        return new Environment(this);
    }
    
    /// <summary>
    /// Gets all variable names defined in this environment (not including parents)
    /// </summary>
    public IEnumerable<string> GetLocalNames()
    {
        return _bindings.Keys;
    }
    
    /// <summary>
    /// Creates a global environment with built-in functions
    /// </summary>
    public static Environment CreateGlobal()
    {
        var global = new Environment();
        
        // Built-in console functions
        global.Define("console", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["log"] = new NativeFunctionValue("log", (args, env) =>
            {
                var output = string.Join(" ", Array.ConvertAll(args, arg => arg.ToString()));
                Console.WriteLine(output);
                return UndefinedValue.Instance;
            }),
            ["error"] = new NativeFunctionValue("error", (args, env) =>
            {
                var output = string.Join(" ", Array.ConvertAll(args, arg => arg.ToString()));
                Console.Error.WriteLine(output);
                return UndefinedValue.Instance;
            }),
            ["warn"] = new NativeFunctionValue("warn", (args, env) =>
            {
                var output = string.Join(" ", Array.ConvertAll(args, arg => arg.ToString()));
                Console.WriteLine($"WARNING: {output}");
                return UndefinedValue.Instance;
            })
        }));
        
        // Built-in print function (shorthand for console.log)
        global.Define("print", new NativeFunctionValue("print", (args, env) =>
        {
            var output = string.Join(" ", Array.ConvertAll(args, arg => arg.ToString()));
            Console.WriteLine(output);
            return UndefinedValue.Instance;
        }));
        
        // Built-in input function for reading user input
        global.Define("input", new NativeFunctionValue("input", (args, env) =>
        {
            // Optional prompt message
            if (args.Length > 0)
            {
                var prompt = string.Join(" ", Array.ConvertAll(args, arg => arg.ToString()));
                Console.Write(prompt);
            }
            
            var userInput = Console.ReadLine();
            return new StringValue(userInput ?? "");
        }));
        
        // Built-in Math functions
        global.Define("Math", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["abs"] = new NativeFunctionValue("abs", (args, env) =>
            {
                if (args.Length < 1) return new NumberValue(double.NaN);
                if (args[0] is NumberValue num) return new NumberValue(Math.Abs(num.Value));
                throw new RuntimeException("Math.abs expects a number");
            }),
            ["floor"] = new NativeFunctionValue("floor", (args, env) =>
            {
                if (args.Length < 1) return new NumberValue(double.NaN);
                if (args[0] is NumberValue num) return new NumberValue(Math.Floor(num.Value));
                throw new RuntimeException("Math.floor expects a number");
            }),
            ["ceil"] = new NativeFunctionValue("ceil", (args, env) =>
            {
                if (args.Length < 1) return new NumberValue(double.NaN);
                if (args[0] is NumberValue num) return new NumberValue(Math.Ceiling(num.Value));
                throw new RuntimeException("Math.ceil expects a number");
            }),
            ["round"] = new NativeFunctionValue("round", (args, env) =>
            {
                if (args.Length < 1) return new NumberValue(double.NaN);
                if (args[0] is NumberValue num) return new NumberValue(Math.Round(num.Value));
                throw new RuntimeException("Math.round expects a number");
            }),
            ["sqrt"] = new NativeFunctionValue("sqrt", (args, env) =>
            {
                if (args.Length < 1) return new NumberValue(double.NaN);
                if (args[0] is NumberValue num) return new NumberValue(Math.Sqrt(num.Value));
                throw new RuntimeException("Math.sqrt expects a number");
            }),
            ["pow"] = new NativeFunctionValue("pow", (args, env) =>
            {
                if (args.Length < 2) return new NumberValue(double.NaN);
                if (args[0] is NumberValue base_ && args[1] is NumberValue exp)
                    return new NumberValue(Math.Pow(base_.Value, exp.Value));
                throw new RuntimeException("Math.pow expects two numbers");
            }),
            ["min"] = new NativeFunctionValue("min", (args, env) =>
            {
                if (args.Length == 0) return new NumberValue(double.PositiveInfinity);
                var min = double.MaxValue;
                foreach (var arg in args)
                {
                    if (arg is NumberValue num && num.Value < min)
                        min = num.Value;
                    else if (arg is not NumberValue)
                        throw new RuntimeException("Math.min expects numbers");
                }
                return new NumberValue(min);
            }),
            ["max"] = new NativeFunctionValue("max", (args, env) =>
            {
                if (args.Length == 0) return new NumberValue(double.NegativeInfinity);
                var max = double.MinValue;
                foreach (var arg in args)
                {
                    if (arg is NumberValue num && num.Value > max)
                        max = num.Value;
                    else if (arg is not NumberValue)
                        throw new RuntimeException("Math.max expects numbers");
                }
                return new NumberValue(max);
            }),
            ["random"] = new NativeFunctionValue("random", (args, env) =>
            {
                return new NumberValue(Random.Shared.NextDouble());
            }),
            ["PI"] = new NumberValue(Math.PI),
            ["E"] = new NumberValue(Math.E)
        }));
        
        // Built-in Array functions
        global.Define("Array", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["isArray"] = new NativeFunctionValue("isArray", (args, env) =>
            {
                return args.Length > 0 && args[0] is ArrayValue ? BooleanValue.True : BooleanValue.False;
            })
        }));
        
        // Built-in global functions
        global.Define("typeof", new NativeFunctionValue("typeof", (args, env) =>
        {
            if (args.Length == 0) return new StringValue("undefined");
            return new StringValue(args[0].Type.ToString().ToLower());
        }));
        
        global.Define("parseInt", new NativeFunctionValue("parseInt", (args, env) =>
        {
            if (args.Length == 0) return new NumberValue(double.NaN);
            var str = args[0].ToString();
            if (int.TryParse(str, out var result))
                return new NumberValue(result);
            return new NumberValue(double.NaN);
        }));
        
        global.Define("parseFloat", new NativeFunctionValue("parseFloat", (args, env) =>
        {
            if (args.Length == 0) return new NumberValue(double.NaN);
            var str = args[0].ToString();
            if (double.TryParse(str, out var result))
                return new NumberValue(result);
            return new NumberValue(double.NaN);
        }));
        
        global.Define("isNaN", new NativeFunctionValue("isNaN", (args, env) =>
        {
            if (args.Length == 0) return BooleanValue.True;
            if (args[0] is NumberValue num) return double.IsNaN(num.Value) ? BooleanValue.True : BooleanValue.False;
            return BooleanValue.True; // Non-numbers are considered NaN
        }));
        
        global.Define("isFinite", new NativeFunctionValue("isFinite", (args, env) =>
        {
            if (args.Length == 0) return BooleanValue.False;
            if (args[0] is NumberValue num) return double.IsFinite(num.Value) ? BooleanValue.True : BooleanValue.False;
            return BooleanValue.False; // Non-numbers are not finite
        }));
        
        // Built-in constants
        global.Define("undefined", UndefinedValue.Instance);
        global.Define("null", NullValue.Instance);
        global.Define("true", BooleanValue.True);
        global.Define("false", BooleanValue.False);
        global.Define("Infinity", new NumberValue(double.PositiveInfinity));
        global.Define("NaN", new NumberValue(double.NaN));
        
        return global;
    }
}
