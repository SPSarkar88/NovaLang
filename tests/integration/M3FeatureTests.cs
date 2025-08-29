using Xunit;
using NovaLang.Lexer;
using NovaLang.Parser;
using NovaLang.Evaluator;
using NovaLang.Runtime;

namespace NovaLang.Tests.Integration
{
    public class M3FeatureTests
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
        public void TestArraySpreadSyntax()
        {
            var result = ExecuteProgram(@"
                let arr1 = [1, 2, 3];
                let arr2 = [...arr1, 4, 5];
                arr2;
            ");
            
            var arrayResult = (ArrayValue)result;
            Assert.Equal(5, arrayResult.Elements.Count);
            Assert.Equal(1.0, ((NumberValue)arrayResult.Elements[0]).Value);
            Assert.Equal(2.0, ((NumberValue)arrayResult.Elements[1]).Value);
            Assert.Equal(3.0, ((NumberValue)arrayResult.Elements[2]).Value);
            Assert.Equal(4.0, ((NumberValue)arrayResult.Elements[3]).Value);
            Assert.Equal(5.0, ((NumberValue)arrayResult.Elements[4]).Value);
        }

        [Fact]
        public void TestObjectSpreadSyntax()
        {
            var result = ExecuteProgram(@"
                let obj1 = {a: 1, b: 2};
                let obj2 = {...obj1, c: 3};
                obj2;
            ");
            
            var objectResult = (ObjectValue)result;
            Assert.Equal(3, objectResult.Properties.Count);
            Assert.Equal(1.0, ((NumberValue)objectResult.Properties["a"]).Value);
            Assert.Equal(2.0, ((NumberValue)objectResult.Properties["b"]).Value);
            Assert.Equal(3.0, ((NumberValue)objectResult.Properties["c"]).Value);
        }

        [Fact]
        public void TestArrayDestructuring()
        {
            var result = ExecuteProgram(@"
                let [first, second, third] = [10, 20, 30];
                first + second + third;
            ");
            
            Assert.Equal(60.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestArrayDestructuringWithRest()
        {
            var result = ExecuteProgram(@"
                let [first, ...rest] = [1, 2, 3, 4, 5];
                rest.length;
            ");
            
            Assert.Equal(4.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestObjectDestructuring()
        {
            var result = ExecuteProgram(@"
                let {a, b} = {a: 10, b: 20, c: 30};
                a + b;
            ");
            
            Assert.Equal(30.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestObjectDestructuringWithRenaming()
        {
            var result = ExecuteProgram(@"
                let {a: newA, b: newB} = {a: 5, b: 15};
                newA + newB;
            ");
            
            Assert.Equal(20.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestNestedArraySpread()
        {
            var result = ExecuteProgram(@"
                let arr1 = [1, 2];
                let arr2 = [3, 4];
                let combined = [...arr1, ...arr2, 5];
                combined;
            ");
            
            var arrayResult = (ArrayValue)result;
            Assert.Equal(5, arrayResult.Elements.Count);
            Assert.Equal(1.0, ((NumberValue)arrayResult.Elements[0]).Value);
            Assert.Equal(2.0, ((NumberValue)arrayResult.Elements[1]).Value);
            Assert.Equal(3.0, ((NumberValue)arrayResult.Elements[2]).Value);
            Assert.Equal(4.0, ((NumberValue)arrayResult.Elements[3]).Value);
            Assert.Equal(5.0, ((NumberValue)arrayResult.Elements[4]).Value);
        }

        [Fact]
        public void TestNestedObjectSpread()
        {
            var result = ExecuteProgram(@"
                let obj1 = {a: 1};
                let obj2 = {b: 2};
                let combined = {...obj1, ...obj2, c: 3};
                combined;
            ");
            
            var objectResult = (ObjectValue)result;
            Assert.Equal(3, objectResult.Properties.Count);
            Assert.Equal(1.0, ((NumberValue)objectResult.Properties["a"]).Value);
            Assert.Equal(2.0, ((NumberValue)objectResult.Properties["b"]).Value);
            Assert.Equal(3.0, ((NumberValue)objectResult.Properties["c"]).Value);
        }

        [Fact]
        public void TestSpreadOverride()
        {
            var result = ExecuteProgram(@"
                let obj1 = {a: 1, b: 2};
                let obj2 = {...obj1, b: 10, c: 3};
                obj2.b;
            ");
            
            Assert.Equal(10.0, ((NumberValue)result).Value);
        }

        [Fact]
        public void TestBasicTemplateLiteral()
        {
            var result = ExecuteProgram(@"
                let message = `Hello World!`;
                message;
            ");
            
            Assert.Equal("Hello World!", ((StringValue)result).Value);
        }

        [Fact]
        public void TestComplexDestructuring()
        {
            var result = ExecuteProgram(@"
                let data = [[1, 2], [3, 4], [5, 6]];
                let [first, ...remaining] = data;
                let [a, b] = first;
                a + b;
            ");
            
            Assert.Equal(3.0, ((NumberValue)result).Value);
        }
    }
}
