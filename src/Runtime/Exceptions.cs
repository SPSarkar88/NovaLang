using System;

namespace NovaLang.Runtime;

/// <summary>
/// Base class for NovaLang runtime exceptions
/// </summary>
public class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message)
    {
    }
    
    public RuntimeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception used for function returns (control flow)
/// </summary>
public class ReturnException : Exception
{
    public NovaValue Value { get; }
    
    public ReturnException(NovaValue value)
    {
        Value = value ?? UndefinedValue.Instance;
    }
}

/// <summary>
/// Exception used for break statements (control flow)
/// </summary>
public class BreakException : Exception
{
    public static readonly BreakException Instance = new();
    
    private BreakException() { }
}

/// <summary>
/// Exception used for continue statements (control flow)
/// </summary>
public class ContinueException : Exception
{
    public static readonly ContinueException Instance = new();
    
    private ContinueException() { }
}

/// <summary>
/// Exception used for thrown errors in NovaLang
/// </summary>
public class NovaThrowException : RuntimeException
{
    public NovaValue ThrownValue { get; }
    
    public NovaThrowException(NovaValue value) : base(value?.ToString() ?? "null")
    {
        ThrownValue = value ?? NullValue.Instance;
    }
}
