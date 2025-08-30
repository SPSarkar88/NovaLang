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
    /// Enables evaluator-enhanced Lambda operations for full function parameter support
    /// </summary>
    public void EnableEvaluatorEnhancedLambda(NovaLang.Evaluator.Evaluator evaluator)
    {
        // Get existing Lambda object or create new one
        var lambdaObject = _bindings.TryGetValue("Lambda", out var existing) 
            ? existing as ObjectValue 
            : new ObjectValue(new Dictionary<string, NovaValue>());
        
        if (lambdaObject == null) return;
        
        // Add evaluator-enhanced versions of filter and map
        lambdaObject["filterWithEvaluator"] = new NativeFunctionValue("Lambda.filterWithEvaluator", (args, env) =>
        {
            if (args.Length != 2)
                throw new RuntimeException("Lambda.filterWithEvaluator expects (collection, predicate)");
            
            var collection = args[0];
            object filterParam = args[1] is StringValue str ? (object)str.Value : args[1];
            
            var items = ExtractCollectionItems(collection);
            var results = FilterItems(items, filterParam, env, evaluator);
            return new ArrayValue(results);
        });
        
        lambdaObject["mapWithEvaluator"] = new NativeFunctionValue("Lambda.mapWithEvaluator", (args, env) =>
        {
            if (args.Length != 2)
                throw new RuntimeException("Lambda.mapWithEvaluator expects (collection, mapper)");
            
            var collection = args[0];
            object mapperParam = args[1] is StringValue str ? (object)str.Value : args[1];
            
            var items = ExtractCollectionItems(collection);
            var results = MapItems(items, mapperParam, env, evaluator);
            return new ArrayValue(results);
        });
        
        // Update the Lambda object in the environment
        _bindings["Lambda"] = lambdaObject;
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

        // Built-in Collection Functions (Functional approach - no OOP)
        // ArrayList: Dynamic array that can grow or shrink as needed
        global.Define("ArrayList", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                var initialCapacity = args.Length > 0 && args[0] is NumberValue num ? (int)num.Value : 10;
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["items"] = new ArrayValue(),
                    ["capacity"] = new NumberValue(initialCapacity),
                    ["size"] = new NumberValue(0),
                    ["type"] = new StringValue("ArrayList")
                });
            }),
            ["add"] = new NativeFunctionValue("add", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list) 
                    throw new RuntimeException("ArrayList.add expects (arrayList, item)");
                
                var items = (ArrayValue)list["items"];
                items.Add(args[1]);
                list["size"] = new NumberValue(items.Length);
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list || args[1] is not NumberValue index)
                    throw new RuntimeException("ArrayList.get expects (arrayList, index)");
                
                var items = (ArrayValue)list["items"];
                var idx = (int)index.Value;
                return idx >= 0 && idx < items.Length ? items[idx] : UndefinedValue.Instance;
            }),
            ["remove"] = new NativeFunctionValue("remove", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list || args[1] is not NumberValue index)
                    throw new RuntimeException("ArrayList.remove expects (arrayList, index)");
                
                var items = (ArrayValue)list["items"];
                var idx = (int)index.Value;
                if (idx >= 0 && idx < items.Length)
                {
                    items.RemoveAt(idx);
                    list["size"] = new NumberValue(items.Length);
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["size"] = new NativeFunctionValue("size", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue list)
                    throw new RuntimeException("ArrayList.size expects (arrayList)");
                return list["size"];
            })
        }));

        // Hashtable: Stores key-value pairs based on the hash code of the key
        global.Define("Hashtable", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["data"] = new ObjectValue(),
                    ["size"] = new NumberValue(0),
                    ["type"] = new StringValue("Hashtable")
                });
            }),
            ["put"] = new NativeFunctionValue("put", (args, env) =>
            {
                if (args.Length < 3 || args[0] is not ObjectValue hashtable)
                    throw new RuntimeException("Hashtable.put expects (hashtable, key, value)");
                
                var data = (ObjectValue)hashtable["data"];
                var key = args[1].ToString();
                var wasNew = !data.HasProperty(key);
                
                data[key] = args[2];
                if (wasNew)
                {
                    var currentSize = ((NumberValue)hashtable["size"]).Value;
                    hashtable["size"] = new NumberValue(currentSize + 1);
                }
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashtable)
                    throw new RuntimeException("Hashtable.get expects (hashtable, key)");
                
                var data = (ObjectValue)hashtable["data"];
                var key = args[1].ToString();
                return data[key];
            }),
            ["containsKey"] = new NativeFunctionValue("containsKey", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashtable)
                    throw new RuntimeException("Hashtable.containsKey expects (hashtable, key)");
                
                var data = (ObjectValue)hashtable["data"];
                var key = args[1].ToString();
                return data.HasProperty(key) ? BooleanValue.True : BooleanValue.False;
            }),
            ["remove"] = new NativeFunctionValue("remove", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashtable)
                    throw new RuntimeException("Hashtable.remove expects (hashtable, key)");
                
                var data = (ObjectValue)hashtable["data"];
                var key = args[1].ToString();
                if (data.HasProperty(key))
                {
                    data.DeleteProperty(key);
                    var currentSize = ((NumberValue)hashtable["size"]).Value;
                    hashtable["size"] = new NumberValue(currentSize - 1);
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["keys"] = new NativeFunctionValue("keys", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue hashtable)
                    throw new RuntimeException("Hashtable.keys expects (hashtable)");
                
                var data = (ObjectValue)hashtable["data"];
                var keys = data.Properties.Keys.Select(k => new StringValue(k)).Cast<NovaValue>().ToArray();
                return new ArrayValue(keys);
            })
        }));

        // Queue: Implements a First-In, First-Out (FIFO) collection
        global.Define("Queue", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["items"] = new ArrayValue(),
                    ["type"] = new StringValue("Queue")
                });
            }),
            ["enqueue"] = new NativeFunctionValue("enqueue", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue queue)
                    throw new RuntimeException("Queue.enqueue expects (queue, item)");
                
                var items = (ArrayValue)queue["items"];
                items.Add(args[1]);
                return UndefinedValue.Instance;
            }),
            ["dequeue"] = new NativeFunctionValue("dequeue", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue queue)
                    throw new RuntimeException("Queue.dequeue expects (queue)");
                
                var items = (ArrayValue)queue["items"];
                if (items.Length == 0) return UndefinedValue.Instance;
                
                var item = items[0];
                items.RemoveAt(0);
                return item;
            }),
            ["peek"] = new NativeFunctionValue("peek", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue queue)
                    throw new RuntimeException("Queue.peek expects (queue)");
                
                var items = (ArrayValue)queue["items"];
                return items.Length > 0 ? items[0] : UndefinedValue.Instance;
            }),
            ["isEmpty"] = new NativeFunctionValue("isEmpty", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue queue)
                    throw new RuntimeException("Queue.isEmpty expects (queue)");
                
                var items = (ArrayValue)queue["items"];
                return items.Length == 0 ? BooleanValue.True : BooleanValue.False;
            }),
            ["size"] = new NativeFunctionValue("size", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue queue)
                    throw new RuntimeException("Queue.size expects (queue)");
                
                var items = (ArrayValue)queue["items"];
                return new NumberValue(items.Length);
            })
        }));

        // Stack: Implements a Last-In, First-Out (LIFO) collection
        global.Define("Stack", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["items"] = new ArrayValue(),
                    ["type"] = new StringValue("Stack")
                });
            }),
            ["push"] = new NativeFunctionValue("push", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue stack)
                    throw new RuntimeException("Stack.push expects (stack, item)");
                
                var items = (ArrayValue)stack["items"];
                items.Add(args[1]);
                return UndefinedValue.Instance;
            }),
            ["pop"] = new NativeFunctionValue("pop", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue stack)
                    throw new RuntimeException("Stack.pop expects (stack)");
                
                var items = (ArrayValue)stack["items"];
                if (items.Length == 0) return UndefinedValue.Instance;
                
                var item = items[items.Length - 1];
                items.RemoveAt(items.Length - 1);
                return item;
            }),
            ["peek"] = new NativeFunctionValue("peek", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue stack)
                    throw new RuntimeException("Stack.peek expects (stack)");
                
                var items = (ArrayValue)stack["items"];
                return items.Length > 0 ? items[items.Length - 1] : UndefinedValue.Instance;
            }),
            ["isEmpty"] = new NativeFunctionValue("isEmpty", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue stack)
                    throw new RuntimeException("Stack.isEmpty expects (stack)");
                
                var items = (ArrayValue)stack["items"];
                return items.Length == 0 ? BooleanValue.True : BooleanValue.False;
            }),
            ["size"] = new NativeFunctionValue("size", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue stack)
                    throw new RuntimeException("Stack.size expects (stack)");
                
                var items = (ArrayValue)stack["items"];
                return new NumberValue(items.Length);
            })
        }));

        // SortedList: Stores key-value pairs sorted by the keys and accessible by both key and index
        global.Define("SortedList", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["keys"] = new ArrayValue(),
                    ["values"] = new ArrayValue(),
                    ["type"] = new StringValue("SortedList")
                });
            }),
            ["add"] = new NativeFunctionValue("add", (args, env) =>
            {
                if (args.Length < 3 || args[0] is not ObjectValue sortedList)
                    throw new RuntimeException("SortedList.add expects (sortedList, key, value)");
                
                var keys = (ArrayValue)sortedList["keys"];
                var values = (ArrayValue)sortedList["values"];
                var newKey = args[1];
                var newValue = args[2];
                
                // Find insertion position to maintain sorted order
                int insertIndex = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                    var comparison = CompareValues(newKey, keys[i]);
                    if (comparison == 0)
                    {
                        // Key exists, update value
                        values[i] = newValue;
                        return UndefinedValue.Instance;
                    }
                    if (comparison < 0)
                    {
                        insertIndex = i;
                        break;
                    }
                    insertIndex = i + 1;
                }
                
                // Insert at correct position
                var keysList = keys.Elements.ToList();
                var valuesList = values.Elements.ToList();
                keysList.Insert(insertIndex, newKey);
                valuesList.Insert(insertIndex, newValue);
                
                sortedList["keys"] = new ArrayValue(keysList);
                sortedList["values"] = new ArrayValue(valuesList);
                
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue sortedList)
                    throw new RuntimeException("SortedList.get expects (sortedList, key)");
                
                var keys = (ArrayValue)sortedList["keys"];
                var values = (ArrayValue)sortedList["values"];
                var searchKey = args[1];
                
                for (int i = 0; i < keys.Length; i++)
                {
                    if (CompareValues(searchKey, keys[i]) == 0)
                    {
                        return values[i];
                    }
                }
                return UndefinedValue.Instance;
            }),
            ["getByIndex"] = new NativeFunctionValue("getByIndex", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue sortedList || args[1] is not NumberValue index)
                    throw new RuntimeException("SortedList.getByIndex expects (sortedList, index)");
                
                var values = (ArrayValue)sortedList["values"];
                var idx = (int)index.Value;
                return idx >= 0 && idx < values.Length ? values[idx] : UndefinedValue.Instance;
            }),
            ["removeByKey"] = new NativeFunctionValue("removeByKey", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue sortedList)
                    throw new RuntimeException("SortedList.removeByKey expects (sortedList, key)");
                
                var keys = (ArrayValue)sortedList["keys"];
                var values = (ArrayValue)sortedList["values"];
                var searchKey = args[1];
                
                for (int i = 0; i < keys.Length; i++)
                {
                    if (CompareValues(searchKey, keys[i]) == 0)
                    {
                        keys.RemoveAt(i);
                        values.RemoveAt(i);
                        return BooleanValue.True;
                    }
                }
                return BooleanValue.False;
            })
        }));

        // List: A dynamic array similar to ArrayList (generic version)
        global.Define("List", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["items"] = new ArrayValue(),
                    ["type"] = new StringValue("List")
                });
            }),
            ["add"] = new NativeFunctionValue("add", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list)
                    throw new RuntimeException("List.add expects (list, item)");
                
                var items = (ArrayValue)list["items"];
                items.Add(args[1]);
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list || args[1] is not NumberValue index)
                    throw new RuntimeException("List.get expects (list, index)");
                
                var items = (ArrayValue)list["items"];
                var idx = (int)index.Value;
                return idx >= 0 && idx < items.Length ? items[idx] : UndefinedValue.Instance;
            }),
            ["set"] = new NativeFunctionValue("set", (args, env) =>
            {
                if (args.Length < 3 || args[0] is not ObjectValue list || args[1] is not NumberValue index)
                    throw new RuntimeException("List.set expects (list, index, value)");
                
                var items = (ArrayValue)list["items"];
                var idx = (int)index.Value;
                if (idx >= 0 && idx < items.Length)
                {
                    items[idx] = args[2];
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["remove"] = new NativeFunctionValue("remove", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list || args[1] is not NumberValue index)
                    throw new RuntimeException("List.remove expects (list, index)");
                
                var items = (ArrayValue)list["items"];
                var idx = (int)index.Value;
                if (idx >= 0 && idx < items.Length)
                {
                    items.RemoveAt(idx);
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["contains"] = new NativeFunctionValue("contains", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue list)
                    throw new RuntimeException("List.contains expects (list, item)");
                
                var items = (ArrayValue)list["items"];
                var searchItem = args[1];
                
                foreach (var item in items.Elements)
                {
                    if (item.Equals(searchItem))
                        return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["size"] = new NativeFunctionValue("size", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue list)
                    throw new RuntimeException("List.size expects (list)");
                
                var items = (ArrayValue)list["items"];
                return new NumberValue(items.Length);
            })
        }));

        // Dictionary: A version of Hashtable for key-value pairs (generic)
        global.Define("Dictionary", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["data"] = new ObjectValue(),
                    ["type"] = new StringValue("Dictionary")
                });
            }),
            ["set"] = new NativeFunctionValue("set", (args, env) =>
            {
                if (args.Length < 3 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.set expects (dictionary, key, value)");
                
                var data = (ObjectValue)dictionary["data"];
                var key = args[1].ToString();
                data[key] = args[2];
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.get expects (dictionary, key)");
                
                var data = (ObjectValue)dictionary["data"];
                var key = args[1].ToString();
                return data[key];
            }),
            ["containsKey"] = new NativeFunctionValue("containsKey", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.containsKey expects (dictionary, key)");
                
                var data = (ObjectValue)dictionary["data"];
                var key = args[1].ToString();
                return data.HasProperty(key) ? BooleanValue.True : BooleanValue.False;
            }),
            ["remove"] = new NativeFunctionValue("remove", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.remove expects (dictionary, key)");
                
                var data = (ObjectValue)dictionary["data"];
                var key = args[1].ToString();
                if (data.HasProperty(key))
                {
                    data.DeleteProperty(key);
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["keys"] = new NativeFunctionValue("keys", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.keys expects (dictionary)");
                
                var data = (ObjectValue)dictionary["data"];
                var keys = data.Properties.Keys.Select(k => new StringValue(k)).Cast<NovaValue>().ToArray();
                return new ArrayValue(keys);
            }),
            ["values"] = new NativeFunctionValue("values", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue dictionary)
                    throw new RuntimeException("Dictionary.values expects (dictionary)");
                
                var data = (ObjectValue)dictionary["data"];
                var values = data.Properties.Values.ToArray();
                return new ArrayValue(values);
            })
        }));

        // SortedDictionary: Stores key-value pairs sorted by the key, offering efficient retrieval by key
        global.Define("SortedDictionary", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["keys"] = new ArrayValue(),
                    ["values"] = new ArrayValue(),
                    ["type"] = new StringValue("SortedDictionary")
                });
            }),
            ["set"] = new NativeFunctionValue("set", (args, env) =>
            {
                if (args.Length < 3 || args[0] is not ObjectValue sortedDict)
                    throw new RuntimeException("SortedDictionary.set expects (sortedDictionary, key, value)");
                
                var keys = (ArrayValue)sortedDict["keys"];
                var values = (ArrayValue)sortedDict["values"];
                var newKey = args[1];
                var newValue = args[2];
                
                // Find insertion position
                int insertIndex = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                    var comparison = CompareValues(newKey, keys[i]);
                    if (comparison == 0)
                    {
                        // Key exists, update value
                        values[i] = newValue;
                        return UndefinedValue.Instance;
                    }
                    if (comparison < 0)
                    {
                        insertIndex = i;
                        break;
                    }
                    insertIndex = i + 1;
                }
                
                // Insert at correct position
                var keysList = keys.Elements.ToList();
                var valuesList = values.Elements.ToList();
                keysList.Insert(insertIndex, newKey);
                valuesList.Insert(insertIndex, newValue);
                
                sortedDict["keys"] = new ArrayValue(keysList);
                sortedDict["values"] = new ArrayValue(valuesList);
                
                return UndefinedValue.Instance;
            }),
            ["get"] = new NativeFunctionValue("get", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue sortedDict)
                    throw new RuntimeException("SortedDictionary.get expects (sortedDictionary, key)");
                
                var keys = (ArrayValue)sortedDict["keys"];
                var values = (ArrayValue)sortedDict["values"];
                var searchKey = args[1];
                
                for (int i = 0; i < keys.Length; i++)
                {
                    if (CompareValues(searchKey, keys[i]) == 0)
                    {
                        return values[i];
                    }
                }
                return UndefinedValue.Instance;
            }),
            ["containsKey"] = new NativeFunctionValue("containsKey", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue sortedDict)
                    throw new RuntimeException("SortedDictionary.containsKey expects (sortedDictionary, key)");
                
                var keys = (ArrayValue)sortedDict["keys"];
                var searchKey = args[1];
                
                for (int i = 0; i < keys.Length; i++)
                {
                    if (CompareValues(searchKey, keys[i]) == 0)
                    {
                        return BooleanValue.True;
                    }
                }
                return BooleanValue.False;
            })
        }));

        // HashSet: Stores a collection of unique elements
        global.Define("HashSet", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["create"] = new NativeFunctionValue("create", (args, env) =>
            {
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["items"] = new ObjectValue(), // Use object for uniqueness
                    ["type"] = new StringValue("HashSet")
                });
            }),
            ["add"] = new NativeFunctionValue("add", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.add expects (hashSet, item)");
                
                var items = (ObjectValue)hashSet["items"];
                var itemKey = args[1].ToString();
                var wasNew = !items.HasProperty(itemKey);
                
                items[itemKey] = args[1]; // Store both key and value for completeness
                return wasNew ? BooleanValue.True : BooleanValue.False;
            }),
            ["contains"] = new NativeFunctionValue("contains", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.contains expects (hashSet, item)");
                
                var items = (ObjectValue)hashSet["items"];
                var itemKey = args[1].ToString();
                return items.HasProperty(itemKey) ? BooleanValue.True : BooleanValue.False;
            }),
            ["remove"] = new NativeFunctionValue("remove", (args, env) =>
            {
                if (args.Length < 2 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.remove expects (hashSet, item)");
                
                var items = (ObjectValue)hashSet["items"];
                var itemKey = args[1].ToString();
                if (items.HasProperty(itemKey))
                {
                    items.DeleteProperty(itemKey);
                    return BooleanValue.True;
                }
                return BooleanValue.False;
            }),
            ["size"] = new NativeFunctionValue("size", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.size expects (hashSet)");
                
                var items = (ObjectValue)hashSet["items"];
                return new NumberValue(items.Properties.Count);
            }),
            ["clear"] = new NativeFunctionValue("clear", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.clear expects (hashSet)");
                
                hashSet["items"] = new ObjectValue();
                return UndefinedValue.Instance;
            }),
            ["toArray"] = new NativeFunctionValue("toArray", (args, env) =>
            {
                if (args.Length < 1 || args[0] is not ObjectValue hashSet)
                    throw new RuntimeException("HashSet.toArray expects (hashSet)");
                
                var items = (ObjectValue)hashSet["items"];
                var values = items.Properties.Values.ToArray();
                return new ArrayValue(values);
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

        // ====================================
        // Lambda-Style Query Operations with Pipeline Support
        // ====================================
        
        // Lambda.pipeline - Execute multiple operations in sequence (Functional Builder Pattern)
        global.Define("Lambda", new ObjectValue(new Dictionary<string, NovaValue>
        {
            ["pipeline"] = new NativeFunctionValue("Lambda.pipeline", (args, env) =>
            {
                if (args.Length < 2)
                    throw new RuntimeException("Lambda.pipeline expects (collection, operations...)");
                
                var collection = args[0];
                var currentData = ExtractCollectionItems(collection);
                
                // Process each operation in sequence
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] is not ArrayValue operation || operation.Length < 1)
                        throw new RuntimeException($"Operation {i} must be an array [operation] or [operation, param]");
                    
                    var opName = operation[0].ToString();
                    object opParam = operation.Length > 1 ? 
                        (operation[1] is StringValue str ? str.Value : operation[1]) : "";
                    
                    // Apply operation using enhanced Lambda methods with function support
                    currentData = opName switch
                    {
                        "filter" => FilterItems(currentData, opParam, env),
                        "map" => MapItems(currentData, opParam, env),
                        "sort" => SortItems(currentData, opParam.ToString()!),
                        "take" => TakeItems(currentData, opParam.ToString()!),
                        "skip" => SkipItems(currentData, opParam.ToString()!),
                        "distinct" => DistinctItems(currentData),
                        "reverse" => ReverseItems(currentData),
                        _ => throw new RuntimeException($"Unknown operation: {opName}")
                    };
                }
                
                return new ArrayValue(currentData);
            }),
            
            // Lambda.chain - Fluent chaining helper
            ["chain"] = new NativeFunctionValue("Lambda.chain", (args, env) =>
            {
                if (args.Length != 1)
                    throw new RuntimeException("Lambda.chain expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                // Return a chain object with fluent methods
                return new ObjectValue(new Dictionary<string, NovaValue>
                {
                    ["data"] = new ArrayValue(items),
                    
                    ["filter"] = new NativeFunctionValue("filter", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 2 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("filter expects (chain, filterType)");
                        
                        var data = (ArrayValue)chain["data"];
                        var filterType = chainArgs[1].ToString();
                        var filtered = FilterItems(data.Elements.ToArray(), filterType);
                        
                        // Return new chain object
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(filtered),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["map"] = new NativeFunctionValue("map", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 2 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("map expects (chain, operation)");
                        
                        var data = (ArrayValue)chain["data"];
                        var operation = chainArgs[1].ToString();
                        var mapped = MapItems(data.Elements.ToArray(), operation);
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(mapped),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["sort"] = new NativeFunctionValue("sort", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length < 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("sort expects (chain) or (chain, direction)");
                        
                        var data = (ArrayValue)chain["data"];
                        var direction = chainArgs.Length > 1 ? chainArgs[1].ToString() : "asc";
                        var sorted = SortItems(data.Elements.ToArray(), direction);
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(sorted),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["take"] = new NativeFunctionValue("take", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 2 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("take expects (chain, count)");
                        
                        var data = (ArrayValue)chain["data"];
                        var count = chainArgs[1].ToString();
                        var taken = TakeItems(data.Elements.ToArray(), count);
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(taken),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["skip"] = new NativeFunctionValue("skip", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 2 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("skip expects (chain, count)");
                        
                        var data = (ArrayValue)chain["data"];
                        var count = chainArgs[1].ToString();
                        var skipped = SkipItems(data.Elements.ToArray(), count);
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(skipped),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["distinct"] = new NativeFunctionValue("distinct", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("distinct expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        var distinct = DistinctItems(data.Elements.ToArray());
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(distinct),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    ["reverse"] = new NativeFunctionValue("reverse", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("reverse expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        var reversed = ReverseItems(data.Elements.ToArray());
                        
                        return new ObjectValue(new Dictionary<string, NovaValue>
                        {
                            ["data"] = new ArrayValue(reversed),
                            ["filter"] = chain["filter"],
                            ["map"] = chain["map"],
                            ["sort"] = chain["sort"],
                            ["take"] = chain["take"],
                            ["skip"] = chain["skip"],
                            ["distinct"] = chain["distinct"],
                            ["reverse"] = chain["reverse"],
                            ["toArray"] = chain["toArray"],
                            ["sum"] = chain["sum"],
                            ["count"] = chain["count"],
                            ["average"] = chain["average"],
                            ["min"] = chain["min"],
                            ["max"] = chain["max"],
                            ["first"] = chain["first"],
                            ["last"] = chain["last"]
                        });
                    }),
                    
                    // Terminal operations
                    ["toArray"] = new NativeFunctionValue("toArray", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("toArray expects (chain)");
                        
                        return chain["data"];
                    }),
                    
                    ["sum"] = new NativeFunctionValue("sum", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("sum expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        double sum = 0;
                        foreach (var item in data.Elements)
                        {
                            if (item is NumberValue num)
                                sum += num.Value;
                        }
                        return new NumberValue(sum);
                    }),
                    
                    ["count"] = new NativeFunctionValue("count", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("count expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        return new NumberValue(data.Length);
                    }),
                    
                    ["average"] = new NativeFunctionValue("average", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("average expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        if (data.Length == 0) return new NumberValue(0);
                        
                        double sum = 0;
                        int count = 0;
                        foreach (var item in data.Elements)
                        {
                            if (item is NumberValue num)
                            {
                                sum += num.Value;
                                count++;
                            }
                        }
                        return count == 0 ? new NumberValue(0) : new NumberValue(sum / count);
                    }),
                    
                    ["min"] = new NativeFunctionValue("min", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("min expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        if (data.Length == 0) return NullValue.Instance;
                        
                        NovaValue min = data[0];
                        for (int i = 1; i < data.Length; i++)
                        {
                            if (CompareValues(data[i], min) < 0)
                                min = data[i];
                        }
                        return min;
                    }),
                    
                    ["max"] = new NativeFunctionValue("max", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("max expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        if (data.Length == 0) return NullValue.Instance;
                        
                        NovaValue max = data[0];
                        for (int i = 1; i < data.Length; i++)
                        {
                            if (CompareValues(data[i], max) > 0)
                                max = data[i];
                        }
                        return max;
                    }),
                    
                    ["first"] = new NativeFunctionValue("first", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("first expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        return data.Length > 0 ? data[0] : NullValue.Instance;
                    }),
                    
                    ["last"] = new NativeFunctionValue("last", (chainArgs, chainEnv) =>
                    {
                        if (chainArgs.Length != 1 || chainArgs[0] is not ObjectValue chain)
                            throw new RuntimeException("last expects (chain)");
                        
                        var data = (ArrayValue)chain["data"];
                        return data.Length > 0 ? data[data.Length - 1] : NullValue.Instance;
                    })
                });
            }),
            
            // Keep the original static methods for backward compatibility
            ["filter"] = new NativeFunctionValue("Lambda.filter", (args, env) =>
            {
                if (args.Length != 2)
                    throw new RuntimeException("Lambda.filter expects (collection, filterType)");
                
                var collection = args[0];
                object filterParam = args[1] is StringValue str ? (object)str.Value : args[1];
                var items = ExtractCollectionItems(collection);
                var results = FilterItems(items, filterParam, env);
                return new ArrayValue(results);
            }),
            
            ["map"] = new NativeFunctionValue("Lambda.map", (args, env) =>
            {
                if (args.Length != 2)
                    throw new RuntimeException("Lambda.map expects (collection, operation)");
                
                var collection = args[0];
                object mapperParam = args[1] is StringValue str ? (object)str.Value : args[1];
                
                var items = ExtractCollectionItems(collection);
                var results = MapItems(items, mapperParam, env);
                return new ArrayValue(results);
            }),
            
            ["sort"] = new NativeFunctionValue("Lambda.sort", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.sort expects (collection) or (collection, direction)");
                
                var collection = args[0];
                var direction = args.Length > 1 ? args[1].ToString() : "asc";
                
                var items = ExtractCollectionItems(collection);
                
                var sorted = direction == "desc" 
                    ? items.OrderByDescending(x => x, new NovaValueComparer()).ToArray()
                    : items.OrderBy(x => x, new NovaValueComparer()).ToArray();
                
                return new ArrayValue(sorted);
            }),
            
            ["count"] = new NativeFunctionValue("Lambda.count", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.count expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                return new NumberValue(items.Length);
            }),
            
            ["sum"] = new NativeFunctionValue("Lambda.sum", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.sum expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                double sum = 0;
                
                foreach (var item in items)
                {
                    if (item is NumberValue num)
                        sum += num.Value;
                }
                
                return new NumberValue(sum);
            }),
            
            ["average"] = new NativeFunctionValue("Lambda.average", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.average expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                if (items.Length == 0)
                    return new NumberValue(0);
                
                double sum = 0;
                int count = 0;
                
                foreach (var item in items)
                {
                    if (item is NumberValue num)
                    {
                        sum += num.Value;
                        count++;
                    }
                }
                
                return count == 0 ? new NumberValue(0) : new NumberValue(sum / count);
            }),
            
            ["min"] = new NativeFunctionValue("Lambda.min", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.min expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                if (items.Length == 0)
                    return NullValue.Instance;
                
                NovaValue min = items[0];
                for (int i = 1; i < items.Length; i++)
                {
                    if (CompareValues(items[i], min) < 0)
                        min = items[i];
                }
                
                return min;
            }),
            
            ["max"] = new NativeFunctionValue("Lambda.max", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.max expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                if (items.Length == 0)
                    return NullValue.Instance;
                
                NovaValue max = items[0];
                for (int i = 1; i < items.Length; i++)
                {
                    if (CompareValues(items[i], max) > 0)
                        max = items[i];
                }
                
                return max;
            }),
            
            ["first"] = new NativeFunctionValue("Lambda.first", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.first expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                return items.Length > 0 ? items[0] : NullValue.Instance;
            }),
            
            ["last"] = new NativeFunctionValue("Lambda.last", (args, env) =>
            {
                if (args.Length == 0)
                    throw new RuntimeException("Lambda.last expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                return items.Length > 0 ? items[items.Length - 1] : NullValue.Instance;
            }),
            
            ["skip"] = new NativeFunctionValue("Lambda.skip", (args, env) =>
            {
                if (args.Length != 2)
                    throw new RuntimeException("Lambda.skip expects (collection, count)");
                
                var collection = args[0];
                var count = args[1] as NumberValue ?? throw new RuntimeException("Skip count must be a number");
                
                var items = ExtractCollectionItems(collection);
                var skipCount = Math.Max(0, (int)count.Value);
                
                if (skipCount >= items.Length) 
                    return new ArrayValue(Array.Empty<NovaValue>());
                
                return new ArrayValue(items.Skip(skipCount).ToArray());
            }),
            
            ["take"] = new NativeFunctionValue("Lambda.take", (args, env) =>
            {
                if (args.Length != 2)
                    throw new RuntimeException("Lambda.take expects (collection, count)");
                
                var collection = args[0];
                var count = args[1] as NumberValue ?? throw new RuntimeException("Take count must be a number");
                
                var items = ExtractCollectionItems(collection);
                var takeCount = Math.Max(0, (int)count.Value);
                
                if (takeCount == 0) 
                    return new ArrayValue(Array.Empty<NovaValue>());
                
                return new ArrayValue(items.Take(takeCount).ToArray());
            }),
            
            ["reverse"] = new NativeFunctionValue("Lambda.reverse", (args, env) =>
            {
                if (args.Length != 1)
                    throw new RuntimeException("Lambda.reverse expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                
                return new ArrayValue(items.Reverse().ToArray());
            }),
            
            ["distinct"] = new NativeFunctionValue("Lambda.distinct", (args, env) =>
            {
                if (args.Length != 1)
                    throw new RuntimeException("Lambda.distinct expects (collection)");
                
                var collection = args[0];
                var items = ExtractCollectionItems(collection);
                var seen = new HashSet<string>();
                var results = new List<NovaValue>();
                
                foreach (var item in items)
                {
                    var key = item.ToString();
                    if (seen.Add(key))
                        results.Add(item);
                }
                
                return new ArrayValue(results.ToArray());
            })
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

    /// <summary>
    /// Helper method to extract items from various collection types for Lambda operations
    /// </summary>
    private static NovaValue[] ExtractCollectionItems(NovaValue collection)
    {
        return collection switch
        {
            ArrayValue arr => arr.Elements.ToArray(),
            ObjectValue obj when obj.Properties.TryGetValue("items", out var items) && items is ArrayValue arrItems => 
                arrItems.Elements.ToArray(),
            ObjectValue obj when obj.Properties.TryGetValue("type", out var type) => 
                ExtractFromNovaLangCollection(obj, type.ToString()),
            _ => new[] { collection }
        };
    }
    
    /// <summary>
    /// Extract items from NovaLang collection objects (ArrayList, List, Queue, Stack, etc.)
    /// </summary>
    private static NovaValue[] ExtractFromNovaLangCollection(ObjectValue collection, string type)
    {
        if (!collection.Properties.TryGetValue("items", out var itemsValue) || itemsValue is not ArrayValue items)
            return Array.Empty<NovaValue>();
        
        return type switch
        {
            "Queue" or "Stack" => items.Elements.ToArray(),
            "ArrayList" or "List" => items.Elements.ToArray(),
            "HashSet" => items.Elements.ToArray(),
            "Hashtable" or "Dictionary" or "SortedDictionary" or "SortedList" => 
                ExtractKeyValuePairs(collection),
            _ => items.Elements.ToArray()
        };
    }
    
    /// <summary>
    /// Extract key-value pairs from dictionary-like collections
    /// </summary>
    private static NovaValue[] ExtractKeyValuePairs(ObjectValue collection)
    {
        if (!collection.Properties.TryGetValue("keys", out var keysValue) || keysValue is not ArrayValue keys ||
            !collection.Properties.TryGetValue("values", out var valuesValue) || valuesValue is not ArrayValue values)
            return Array.Empty<NovaValue>();
        
        var result = new List<NovaValue>();
        for (int i = 0; i < Math.Min(keys.Length, values.Length); i++)
        {
            var pair = new Dictionary<string, NovaValue>
            {
                ["key"] = keys.Elements[i],
                ["value"] = values.Elements[i]
            };
            result.Add(new ObjectValue(pair));
        }
        
        return result.ToArray();
    }
    
    /// <summary>
    /// Helper method to call a function value with arguments
    /// </summary>
    private static NovaValue CallFunction(NovaValue function, NovaValue[] args, Environment env, NovaLang.Evaluator.Evaluator? evaluator = null)
    {
        // For any callable function (native or user-defined), use the built-in Call method
        if (function.IsCallable)
        {
            return function.Call(args, env);
        }
        
        throw new RuntimeException($"Value is not a function: {function}");
    }
    
    // ====================================
    // Lambda Pipeline Helper Methods with Function Support
    // ====================================
    
    private static NovaValue[] FilterItems(NovaValue[] items, object filter, Environment? env = null, NovaLang.Evaluator.Evaluator? evaluator = null)
    {
        var results = new List<NovaValue>();
        
        foreach (var item in items)
        {
            bool include = false;
            
            if (filter is string filterType)
            {
                // Backward compatibility: magic string filters
                include = filterType switch
                {
                    "even" => item is NumberValue n && n.Value % 2 == 0,
                    "odd" => item is NumberValue n && n.Value % 2 != 0,
                    "positive" => item is NumberValue n && n.Value > 0,
                    "negative" => item is NumberValue n && n.Value < 0,
                    "high" => item is NumberValue n && n.Value > 85, // for scores
                    "nonEmpty" => item is StringValue s && !string.IsNullOrEmpty(s.Value),
                    "truthy" => IsTruthy(item),
                    _ => true
                };
            }
            else if (filter is NovaValue filterFunc && env != null)
            {
                // Function-based filtering
                try
                {
                    var result = CallFunction(filterFunc, new[] { item }, env, evaluator);
                    include = IsTruthy(result);
                }
                catch
                {
                    include = false; // If function call fails, exclude the item
                }
            }
            
            if (include) results.Add(item);
        }
        
        return results.ToArray();
    }
    
    private static NovaValue[] MapItems(NovaValue[] items, object mapper, Environment? env = null, NovaLang.Evaluator.Evaluator? evaluator = null)
    {
        var results = new List<NovaValue>();
        
        foreach (var item in items)
        {
            NovaValue result = item; // Default: return original item
            
            if (mapper is string operation)
            {
                // Backward compatibility: magic string operations
                result = operation switch
                {
                    "double" => item is NumberValue n ? new NumberValue(n.Value * 2) : item,
                    "square" => item is NumberValue n ? new NumberValue(n.Value * n.Value) : item,
                    "abs" => item is NumberValue n ? new NumberValue(Math.Abs(n.Value)) : item,
                    "upper" => item is StringValue s ? new StringValue(s.Value.ToUpperInvariant()) : item,
                    "lower" => item is StringValue s ? new StringValue(s.Value.ToLowerInvariant()) : item,
                    "length" => item is StringValue s ? new NumberValue(s.Value.Length) : 
                               item is ArrayValue a ? new NumberValue(a.Length) : item,
                    _ => item
                };
            }
            else if (mapper is NovaValue mapperFunc && env != null)
            {
                // Function-based mapping
                try
                {
                    result = CallFunction(mapperFunc, new[] { item }, env, evaluator);
                }
                catch
                {
                    result = item; // If function call fails, return original item
                }
            }
            
            results.Add(result);
        }
        
        return results.ToArray();
    }
    
    private static NovaValue[] SortItems(NovaValue[] items, string direction)
    {
        return direction == "desc" 
            ? items.OrderByDescending(x => x, new NovaValueComparer()).ToArray()
            : items.OrderBy(x => x, new NovaValueComparer()).ToArray();
    }
    
    private static NovaValue[] TakeItems(NovaValue[] items, string countStr)
    {
        if (int.TryParse(countStr, out int count))
        {
            return items.Take(Math.Max(0, count)).ToArray();
        }
        return items;
    }
    
    private static NovaValue[] SkipItems(NovaValue[] items, string countStr)
    {
        if (int.TryParse(countStr, out int count))
        {
            return items.Skip(Math.Max(0, count)).ToArray();
        }
        return items;
    }
    
    private static NovaValue[] DistinctItems(NovaValue[] items)
    {
        var seen = new HashSet<string>();
        var results = new List<NovaValue>();
        
        foreach (var item in items)
        {
            var key = item.ToString();
            if (seen.Add(key))
                results.Add(item);
        }
        
        return results.ToArray();
    }
    
    private static NovaValue[] ReverseItems(NovaValue[] items)
    {
        return items.Reverse().ToArray();
    }
    
    /// <summary>
    /// Helper method to determine if a value is truthy
    /// </summary>
    private static bool IsTruthy(NovaValue value)
    {
        return value switch
        {
            BooleanValue b => b.Value,
            NumberValue n => n.Value != 0 && !double.IsNaN(n.Value),
            StringValue s => !string.IsNullOrEmpty(s.Value),
            NullValue => false,
            UndefinedValue => false,
            ArrayValue a => a.Length > 0,
            ObjectValue o => o.Properties.Count > 0,
            _ => true
        };
    }
    
    /// <summary>
    /// Helper method to compare two NovaValues for sorting
    /// </summary>
    public static int CompareValues(NovaValue left, NovaValue right)
    {
        return (left, right) switch
        {
            (NumberValue l, NumberValue r) => l.Value.CompareTo(r.Value),
            (StringValue l, StringValue r) => string.Compare(l.Value, r.Value, StringComparison.Ordinal),
            (BooleanValue l, BooleanValue r) => l.Value.CompareTo(r.Value),
            _ => string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal)
        };
    }
}

/// <summary>
/// Comparer for NovaValue objects used in Lambda operations
/// </summary>
public class NovaValueComparer : IComparer<NovaValue>
{
    public int Compare(NovaValue? x, NovaValue? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        
        return Environment.CompareValues(x, y);
    }
}
