using Xunit;
using NovaLang.Parser;
using NovaLang.Lexer;
using NovaLang.AST;

namespace NovaLang.Tests.Unit
{
    public class ParserTests
    {
        private Program ParseProgram(string source)
        {
            var lexer = new Lexer.Lexer(source);
            var tokens = lexer.ScanTokens();
            var parser = new Parser.Parser(tokens);
            return parser.Parse();
        }

        [Fact]
        public void TestVariableDeclaration()
        {
            var program = ParseProgram("let x = 42;");
            
            Assert.Single(program.Statements);
            Assert.IsType<VariableDeclaration>(program.Statements[0]);
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            Assert.Equal("let", varDecl.Kind);
            Assert.Single(varDecl.Declarations);
            
            var declarator = varDecl.Declarations[0];
            Assert.IsType<IdentifierExpression>(declarator.Id);
            Assert.IsType<LiteralExpression>(declarator.Init);
            
            var id = (IdentifierExpression)declarator.Id;
            var init = (LiteralExpression)declarator.Init;
            Assert.Equal("x", id.Name);
            Assert.Equal(42.0, init.Value);
        }

        [Fact]
        public void TestFunctionDeclaration()
        {
            var program = ParseProgram("function add(a, b) { return a + b; }");
            
            Assert.Single(program.Statements);
            Assert.IsType<FunctionDeclaration>(program.Statements[0]);
            
            var funcDecl = (FunctionDeclaration)program.Statements[0];
            Assert.Equal("add", funcDecl.Name);
            Assert.Equal(2, funcDecl.Parameters.Count);
            Assert.Equal("a", funcDecl.Parameters[0]);
            Assert.Equal("b", funcDecl.Parameters[1]);
            Assert.IsType<BlockStatement>(funcDecl.Body);
        }

        [Fact]
        public void TestArrayExpression()
        {
            var program = ParseProgram("let arr = [1, 2, 3];");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var init = (ArrayExpression)varDecl.Declarations[0].Init;
            
            Assert.Equal(3, init.Elements.Count);
            Assert.All(init.Elements, element => Assert.IsType<LiteralExpression>(element));
        }

        [Fact]
        public void TestObjectExpression()
        {
            var program = ParseProgram("let obj = {a: 1, b: 2};");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var init = (ObjectExpression)varDecl.Declarations[0].Init;
            
            Assert.Equal(2, init.Properties.Count);
            Assert.Equal("a", ((IdentifierExpression)init.Properties[0].Key).Name);
            Assert.Equal("b", ((IdentifierExpression)init.Properties[1].Key).Name);
        }

        [Fact]
        public void TestSpreadInArray()
        {
            var program = ParseProgram("let arr = [1, ...other, 2];");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var init = (ArrayExpression)varDecl.Declarations[0].Init;
            
            Assert.Equal(3, init.Elements.Count);
            Assert.IsType<LiteralExpression>(init.Elements[0]);
            Assert.IsType<SpreadExpression>(init.Elements[1]);
            Assert.IsType<LiteralExpression>(init.Elements[2]);
            
            var spread = (SpreadExpression)init.Elements[1];
            Assert.IsType<IdentifierExpression>(spread.Argument);
        }

        [Fact]
        public void TestSpreadInObject()
        {
            var program = ParseProgram("let obj = {a: 1, ...other, b: 2};");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var init = (ObjectExpression)varDecl.Declarations[0].Init;
            
            Assert.Equal(3, init.Properties.Count);
            Assert.IsType<SpreadExpression>(init.Properties[1].Value);
        }

        [Fact]
        public void TestArrayDestructuring()
        {
            var program = ParseProgram("let [a, b, ...rest] = array;");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var pattern = (ArrayPattern)varDecl.Declarations[0].Id;
            
            Assert.Equal(2, pattern.Elements.Count);
            Assert.NotNull(pattern.RestElement);
            Assert.IsType<IdentifierExpression>(pattern.Elements[0]);
            Assert.IsType<IdentifierExpression>(pattern.Elements[1]);
            Assert.IsType<IdentifierExpression>(pattern.RestElement);
        }

        [Fact]
        public void TestObjectDestructuring()
        {
            var program = ParseProgram("let {a, b: newB} = obj;");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var pattern = (ObjectPattern)varDecl.Declarations[0].Id;
            
            Assert.Equal(2, pattern.Properties.Count);
            Assert.True(pattern.Properties[0].IsShorthand);
            Assert.False(pattern.Properties[1].IsShorthand);
        }

        [Fact]
        public void TestBinaryExpression()
        {
            var program = ParseProgram("let result = a + b * c;");
            
            var varDecl = (VariableDeclaration)program.Statements[0];
            var init = (BinaryExpression)varDecl.Declarations[0].Init;
            
            Assert.Equal(TokenType.Plus, init.Operator);
            Assert.IsType<IdentifierExpression>(init.Left);
            Assert.IsType<BinaryExpression>(init.Right); // b * c
        }

        [Fact]
        public void TestIfStatement()
        {
            var program = ParseProgram("if (x > 0) { return x; } else { return -x; }");
            
            Assert.Single(program.Statements);
            Assert.IsType<IfStatement>(program.Statements[0]);
            
            var ifStmt = (IfStatement)program.Statements[0];
            Assert.IsType<BinaryExpression>(ifStmt.Test);
            Assert.IsType<BlockStatement>(ifStmt.Consequent);
            Assert.IsType<BlockStatement>(ifStmt.Alternate);
        }

        [Fact]
        public void TestWhileLoop()
        {
            var program = ParseProgram("while (i < 10) { i = i + 1; }");
            
            Assert.Single(program.Statements);
            Assert.IsType<WhileStatement>(program.Statements[0]);
            
            var whileStmt = (WhileStatement)program.Statements[0];
            Assert.IsType<BinaryExpression>(whileStmt.Test);
            Assert.IsType<BlockStatement>(whileStmt.Body);
        }
    }
}
