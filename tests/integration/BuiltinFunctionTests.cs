using Xunit;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.Evaluator;
using NovaLang.Runtime;
using System.IO;
using System;

namespace NovaLang.Tests.Integration
{
    public class BuiltinFunctionTests
    {
        private NovaValue ExecuteProgram(string source)
        {
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.ScanTokens();
            var parser = new Parser.Parser(tokens);
            var program = parser.Parse();
            var evaluator = new Evaluator.Evaluator(Environment.CreateGlobal());
            return program.Accept(evaluator);
        }

        [Fact]
        public void TestConsoleLog()
        {
            // Capture console output
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            try
            {
                ExecuteProgram("console.log('Hello, World!');");
                var output = stringWriter.ToString();
                Assert.Contains("Hello, World!", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TestConsoleLogMultipleArgs()
        {
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            try
            {
                ExecuteProgram("console.log('Number:', 42, 'Boolean:', true);");
                var output = stringWriter.ToString();
                Assert.Contains("Number: 42 Boolean: true", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TestTypeofFunction()
        {
            var result1 = ExecuteProgram("typeof 42;");
            Assert.Equal("number", ((StringValue)result1).Value);

            var result2 = ExecuteProgram("typeof 'hello';");
            Assert.Equal("string", ((StringValue)result2).Value);

            var result3 = ExecuteProgram("typeof true;");
            Assert.Equal("boolean", ((StringValue)result3).Value);

            var result4 = ExecuteProgram("typeof null;");
            Assert.Equal("null", ((StringValue)result4).Value);

            var result5 = ExecuteProgram("typeof undefined;");
            Assert.Equal("undefined", ((StringValue)result5).Value);

            var result6 = ExecuteProgram("typeof [1, 2, 3];");
            Assert.Equal("array", ((StringValue)result6).Value);

            var result7 = ExecuteProgram("typeof {a: 1};");
            Assert.Equal("object", ((StringValue)result7).Value);
        }

        [Fact]
        public void TestMathFunctions()
        {
            var result1 = ExecuteProgram("Math.abs(-5);");
            Assert.Equal(5.0, ((NumberValue)result1).Value);

            var result2 = ExecuteProgram("Math.floor(4.7);");
            Assert.Equal(4.0, ((NumberValue)result2).Value);

            var result3 = ExecuteProgram("Math.ceil(4.2);");
            Assert.Equal(5.0, ((NumberValue)result3).Value);

            var result4 = ExecuteProgram("Math.round(4.6);");
            Assert.Equal(5.0, ((NumberValue)result4).Value);

            var result5 = ExecuteProgram("Math.sqrt(16);");
            Assert.Equal(4.0, ((NumberValue)result5).Value);

            var result6 = ExecuteProgram("Math.pow(2, 3);");
            Assert.Equal(8.0, ((NumberValue)result6).Value);

            var result7 = ExecuteProgram("Math.min(5, 3, 8, 1);");
            Assert.Equal(1.0, ((NumberValue)result7).Value);

            var result8 = ExecuteProgram("Math.max(5, 3, 8, 1);");
            Assert.Equal(8.0, ((NumberValue)result8).Value);
        }

        [Fact]
        public void TestMathRandom()
        {
            var result = ExecuteProgram("Math.random();");
            var value = ((NumberValue)result).Value;
            Assert.True(value >= 0.0 && value < 1.0);
        }

        [Fact]
        public void TestArrayIsArray()
        {
            var result1 = ExecuteProgram("Array.isArray([1, 2, 3]);");
            Assert.True(((BooleanValue)result1).Value);

            var result2 = ExecuteProgram("Array.isArray('not an array');");
            Assert.False(((BooleanValue)result2).Value);

            var result3 = ExecuteProgram("Array.isArray({a: 1});");
            Assert.False(((BooleanValue)result3).Value);
        }

        [Fact]
        public void TestComplexBuiltinUsage()
        {
            var result = ExecuteProgram(@"
                let numbers = [1, -2, 3.7, -4.2, 5];
                let absNumbers = [];
                for (let i = 0; i < numbers.length; i = i + 1) {
                    absNumbers[i] = Math.abs(numbers[i]);
                }
                Math.max(...absNumbers);
            ");
            Assert.Equal(5.0, ((NumberValue)result).Value);
        }
    }
}
