using Xunit;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.Evaluator;
using NovaLang.Runtime;

namespace NovaLang.Tests.Integration
{
    public class BasicLanguageTests
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
        public void TestBasicArithmetic()
        {
            var result = ExecuteProgram("let x = 5 + 3 * 2; x;");
            Assert.Equal(11.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestStringConcatenation()
        {
            var result = ExecuteProgram("let greeting = 'Hello' + ' ' + 'World'; greeting;");
            Assert.Equal("Hello World", ((StringValue)result).Value);
        }

        [Fact]
        public void TestBooleanLogic()
        {
            var result1 = ExecuteProgram("let result = true && false; result;");
            Assert.False(((BooleanValue)result1).Value);

            var result2 = ExecuteProgram("let result = true || false; result;");
            Assert.True(((BooleanValue)result2).Value);

            var result3 = ExecuteProgram("let result = !false; result;");
            Assert.True(((BooleanValue)result3).Value);
        }

        [Fact]
        public void TestComparisons()
        {
            var result1 = ExecuteProgram("let result = 5 > 3; result;");
            Assert.True(((BooleanValue)result1).Value);

            var result2 = ExecuteProgram("let result = 5 == 5; result;");
            Assert.True(((BooleanValue)result2).Value);

            var result3 = ExecuteProgram("let result = 5 != 3; result;");
            Assert.True(((BooleanValue)result3).Value);
        }

        [Fact]
        public void TestVariableAssignment()
        {
            var result = ExecuteProgram("let x = 10; let y = 20; let z = x + y; z;");
            Assert.Equal(30.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestArrays()
        {
            var result = ExecuteProgram("let arr = [1, 2, 3]; arr[1];");
            Assert.Equal(2.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestObjects()
        {
            var result = ExecuteProgram("let obj = {a: 1, b: 2}; obj.a;");
            Assert.Equal(1.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestFunctions()
        {
            var result = ExecuteProgram(@"
                function add(a, b) {
                    return a + b;
                }
                let result = add(5, 3);
                result;
            ");
            Assert.Equal(8.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestArrowFunctions()
        {
            var result = ExecuteProgram(@"
                let multiply = (a, b) => a * b;
                let result = multiply(4, 3);
                result;
            ");
            Assert.Equal(12.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestIfStatement()
        {
            var result = ExecuteProgram(@"
                let x = 10;
                let result;
                if (x > 5) {
                    result = 'big';
                } else {
                    result = 'small';
                }
                result;
            ");
            Assert.Equal("big", ((StringValue)result).Value);
        }

        [Fact]
        public void TestWhileLoop()
        {
            var result = ExecuteProgram(@"
                let i = 0;
                let sum = 0;
                while (i < 5) {
                    sum = sum + i;
                    i = i + 1;
                }
                sum;
            ");
            Assert.Equal(10.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestForLoop()
        {
            var result = ExecuteProgram(@"
                let sum = 0;
                for (let i = 0; i < 5; i = i + 1) {
                    sum = sum + i;
                }
                sum;
            ");
            Assert.Equal(10.0, ((NumberValue)result).Value);
        }
    }
}
