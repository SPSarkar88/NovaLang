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
    /// Helper method to compare two NovaValues for sorting
    /// </summary>
    private static int CompareValues(NovaValue left, NovaValue right)
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
