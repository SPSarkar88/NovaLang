using System;
using System.Collections.Generic;
using System.Linq;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.AST;
using NovaLang.Evaluator;
using NovaLang.Runtime;

namespace NovaLang;

/// <summary>
/// Simple test class to validate lexer and parser functionality
/// </summary>
public static class LanguageTest
{
    public static void TestLexer()
    {
        Console.WriteLine("Testing Lexer...");
        
        var source = @"
            let x = 42;
            const name = ""Hello"";
            function add(a, b) {
                return a + b;
            }
            
            if (x > 0) {
                console.log(""positive"");
            }
        ";
        
        var lexer = new Lexer.Lexer(source);
        var tokens = lexer.Tokenize().ToList();
        
        Console.WriteLine($"Found {tokens.Count} tokens:");
        foreach (var token in tokens.Take(20)) // Show first 20 tokens
        {
            Console.WriteLine($"  {token.Type,-15} {token.Value,-10} at {token.Range}");
        }
        Console.WriteLine();
    }
    
    public static void TestParser()
    {
        Console.WriteLine("Testing Parser...");
        
        var source = @"
            let x = 42;
            const greeting = ""Hello World"";
            
            function greet(name) {
                return greeting + "" "" + name;
            }
            
            let result = greet(""NovaLang"");
        ";
        
        try
        {
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser.Parser(tokens);
            var program = parser.Parse();
            
            Console.WriteLine($"Parsed program with {program.Body.Count} statements:");
            foreach (var statement in program.Body)
            {
                Console.WriteLine($"  {statement.GetType().Name} at {statement.Range}");
            }
            Console.WriteLine();
        }
        catch (ParseException ex)
        {
            Console.WriteLine($"Parse error: {ex.Message} at {ex.Range}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
    
    public static void TestEvaluator()
    {
        Console.WriteLine("Testing Evaluator...");
        
        var source = @"
            let x = 10;
            let y = 20;
            console.log(""Testing basic arithmetic:"");
            console.log(x + y);
            
            function add(a, b) {
                return a + b;
            }
            console.log(""Function result:"", add(5, 7));
            
            // Test arrays and objects
            let arr = [1, 2, 3];
            let obj = { name: ""NovaLang"", version: 1 };
            console.log(""Array:"", arr);
            console.log(""Object:"", obj);
            
            // Test control flow
            if (x > y) {
                console.log(""x is greater"");
            } else {
                console.log(""y is greater or equal"");
            }
        ";
        
        try
        {
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser.Parser(tokens);
            var program = parser.Parse();
            
            var evaluator = new Evaluator.Evaluator(Runtime.Environment.CreateGlobal());
            var result = program.Accept(evaluator);
            
            Console.WriteLine($"Evaluation completed. Final result: {result}");
            Console.WriteLine();
        }
        catch (RuntimeException ex)
        {
            Console.WriteLine($"Runtime error: {ex.Message}");
        }
        catch (ParseException ex)
        {
            Console.WriteLine($"Parse error: {ex.Message} at {ex.Range}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
    
    public static void RunTests()
    {
        TestLexer();
        TestParser();
        TestEvaluator();
    }
}
