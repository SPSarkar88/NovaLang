using Xunit;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.Evaluator;
using NovaLang.Runtime;
using System.IO;
using System;

namespace NovaLang.Tests.Integration
{
    public class ScriptTests
    {
        private string GetScriptPath(string scriptName) =>
            Path.Combine(AppContext.BaseDirectory, "scripts", scriptName);

        private NovaValue ExecuteScriptFile(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {scriptPath}");
            }

            var source = File.ReadAllText(scriptPath);
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.ScanTokens();
            var parser = new Parser.Parser(tokens);
            var program = parser.Parse();
            var evaluator = new Evaluator.Evaluator(Environment.CreateGlobal());
            return program.Accept(evaluator);
        }

        private string CaptureOutput(Action action)
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            Console.SetError(stringWriter);

            try
            {
                action();
                return stringWriter.ToString();
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        [Fact]
        public void TestArithmeticScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("arithmetic.sf")));
            
            Assert.Contains("Sum: 30", output);
            Assert.Contains("Difference: 10", output);
            Assert.Contains("Product: 200", output);
            Assert.Contains("Quotient: 2", output);
            Assert.Contains("Remainder: 0", output);
        }

        [Fact]
        public void TestFunctionsScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("functions.sf")));
            
            Assert.Contains("Hello, Alice!", output);
            Assert.Contains("8", output); // add(5, 3)
            Assert.Contains("120", output); // factorial(5)
            Assert.Contains("24", output); // multiply(4, 6)
            Assert.Contains("49", output); // square(7)
        }

        [Fact]
        public void TestArraysObjectsScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("arrays_objects.sf")));
            
            Assert.Contains("First number: 1", output);
            Assert.Contains("Last fruit: cherry", output);
            Assert.Contains("Person name: John", output);
            Assert.Contains("Person age: 30", output);
        }

        [Fact]
        public void TestControlFlowScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("control_flow.sf")));
            
            Assert.Contains("x is greater than 10", output);
            Assert.Contains("Sum from while loop: 10", output);
            Assert.Contains("Product from for loop: 120", output);
            Assert.Contains("1 * 1 = 1", output);
            Assert.Contains("3 * 3 = 9", output);
        }

        [Fact]
        public void TestM3FeaturesScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("m3_features.sf")));
            
            Assert.Contains("Combined array: [1, 2, 3, 4, 5, 6, 7, 8]", output);
            Assert.Contains("First: 10", output);
            Assert.Contains("Second: 20", output);
            Assert.Contains("Rest: [30, 40, 50]", output);
            Assert.Contains("Destructured name: Bob", output);
            Assert.Contains("Full name: Charlie", output);
            Assert.Contains("Template message: Hello World!", output);
        }

        [Fact]
        public void TestBuiltinsScript()
        {
            var output = CaptureOutput(() => ExecuteScriptFile(GetScriptPath("builtins.sf")));
            
            Assert.Contains("typeof 42: number", output);
            Assert.Contains("typeof 'hello': string", output);
            Assert.Contains("typeof true: boolean", output);
            Assert.Contains("Math.abs(-15): 15", output);
            Assert.Contains("Math.floor(4.7): 4", output);
            Assert.Contains("Math.ceil(4.2): 5", output);
            Assert.Contains("Array.isArray([1,2,3]): true", output);
            Assert.Contains("Array.isArray('string'): false", output);
        }

        [Fact]
        public void TestAllScriptsExecuteWithoutErrors()
        {
            var scriptFiles = new[]
            {
                "arithmetic.sf",
                "functions.sf",
                "arrays_objects.sf",
                "control_flow.sf",
                "m3_features.sf",
                "builtins.sf"
            };

            foreach (var scriptFile in scriptFiles)
            {
                var scriptPath = GetScriptPath(scriptFile);
                
                // This should not throw an exception
                var exception = Record.Exception(() => ExecuteScriptFile(scriptPath));
                Assert.Null(exception);
            }
        }
    }
}
