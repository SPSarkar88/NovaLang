using System;
using System.IO;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.Evaluator;
using NovaLang.Runtime;

namespace NovaLang;

/// <summary>
/// NovaLang - A JavaScript-like functional programming language interpreter
/// Built in C#/.NET with focus on functional programming and embedding capabilities
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("NovaLang - JavaScript-like Functional Language Interpreter");
        Console.WriteLine("Version: 1.0.0-alpha");
        Console.WriteLine();

        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var firstArg = args[0].ToLowerInvariant();

        // Check if first argument is a command or a file
        if (IsCommand(firstArg))
        {
            var command = firstArg;
            switch (command)
            {
                case "test":
                    RunTests();
                    break;
                case "repl":
                    StartRepl();
                    break;
                case "run":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Please specify a script file to run.");
                        Console.WriteLine("Usage: novalang run <file.sf>");
                        return;
                    }
                    RunScript(args[1]);
                    break;
                case "fmt":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Please specify a file to format.");
                        Console.WriteLine("Usage: novalang fmt <file.sf>");
                        return;
                    }
                    FormatFile(args[1]);
                    break;
                case "lint":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Please specify a file to lint.");
                        Console.WriteLine("Usage: novalang lint <file.sf>");
                        return;
                    }
                    LintFile(args[1]);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"Error: Unknown command '{command}'");
                    ShowHelp();
                    break;
            }
        }
        else
        {
            // First argument is not a command, treat it as a script file
            RunScript(args[0]);
        }
    }

    static bool IsCommand(string arg)
    {
        var commands = new[] { "test", "repl", "run", "fmt", "lint", "help", "--help", "-h" };
        return commands.Contains(arg);
    }

    static void RunTests()
    {
        Console.WriteLine("Running language implementation tests...");
        Console.WriteLine();
        
        LanguageTest.RunTests();
        
        Console.WriteLine("Tests completed!");
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage: novalang [command] [options] or novalang <script.sf>");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  test           Run language implementation tests");
        Console.WriteLine("  repl           Start the interactive REPL");
        Console.WriteLine("  run <file>     Execute a NovaLang script file (.sf)");
        Console.WriteLine("  fmt <file>     Format a NovaLang script file");
        Console.WriteLine("  lint <file>    Lint a NovaLang script file");
        Console.WriteLine("  help           Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  novalang test              # Run implementation tests");
        Console.WriteLine("  novalang repl              # Start interactive mode");
        Console.WriteLine("  novalang script.sf         # Run a script file directly");
        Console.WriteLine("  novalang run script.sf     # Run a script file explicitly");
        Console.WriteLine("  novalang fmt script.sf     # Format a script file");
        Console.WriteLine("  novalang lint script.sf    # Lint a script file");
    }

    static void StartRepl()
    {
        Console.WriteLine("Starting NovaLang REPL...");
        Console.WriteLine("Type .help for commands, .exit to quit");
        Console.WriteLine();
        
        var evaluator = new NovaLang.Evaluator.Evaluator(Runtime.Environment.CreateGlobal());
        
        // Enable full function parameter support for Lambda operations
        evaluator.EnableFunctionParameterSupport();
        
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                continue;
                
            if (input == ".exit")
                break;
                
            if (input == ".help")
            {
                Console.WriteLine("REPL Commands:");
                Console.WriteLine("  .help     Show this help");
                Console.WriteLine("  .exit     Exit the REPL");
                Console.WriteLine("  .env      Show environment variables");
                Console.WriteLine("  .time     Toggle timing display");
                Console.WriteLine("  .load     Load a script file");
                Console.WriteLine("  .type     Show type of expression");
                continue;
            }
            
            try
            {
                var lexer = new Lexer.Lexer(input);
                var tokens = lexer.Tokenize();
                var parser = new Parser.Parser(tokens);
                var program = parser.Parse();
                
                var result = program.Accept(evaluator);
                
                // Don't print undefined results
                if (result.Type != NovaValueType.Undefined)
                {
                    Console.WriteLine(result.ToString());
                }
            }
            catch (RuntimeException ex)
            {
                Console.WriteLine($"Runtime Error: {ex.Message}");
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Parse Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        Console.WriteLine("Goodbye!");
    }

    static void RunScript(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Error: File '{filename}' not found.");
            return;
        }
        
        if (!filename.EndsWith(".sf", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Warning: NovaLang script files should use the .sf extension.");
        }

        Console.WriteLine($"Running script: {filename}");
        
        try
        {
            var source = File.ReadAllText(filename);
            
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser.Parser(tokens);
            var program = parser.Parse();
            
            var evaluator = new NovaLang.Evaluator.Evaluator(NovaLang.Runtime.Environment.CreateGlobal());
            
            // Enable full function parameter support for Lambda operations
            evaluator.EnableFunctionParameterSupport();
            
            var result = program.Accept(evaluator);
            
            Console.WriteLine($"Script completed. Exit value: {result}");
        }
        catch (RuntimeException ex)
        {
            Console.WriteLine($"Runtime Error: {ex.Message}");
        }
        catch (ParseException ex)
        {
            Console.WriteLine($"Parse Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void FormatFile(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Error: File '{filename}' not found.");
            return;
        }

        Console.WriteLine($"Formatting file: {filename}");
        // TODO: Implement formatter
        Console.WriteLine("[Not implemented] Formatter would process the file here.");
    }

    static void LintFile(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Error: File '{filename}' not found.");
            return;
        }

        Console.WriteLine($"Linting file: {filename}");
        // TODO: Implement linter
        Console.WriteLine("[Not implemented] Linter would analyze the file here.");
    }
}
