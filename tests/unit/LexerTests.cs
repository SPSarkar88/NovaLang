using Xunit;
using NovaLang.Lexer;
using System.Linq;

namespace NovaLang.Tests.Unit
{
    public class LexerTests
    {
        [Fact]
        public void TestBasicTokens()
        {
            var lexer = new Lexer.Lexer("let x = 42;");
            var tokens = lexer.ScanTokens();
            
            Assert.Equal(6, tokens.Count); // let, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Let, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("x", tokens[1].Value);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void TestOperators()
        {
            var lexer = new Lexer.Lexer("+ - * / % ** == === != !== < <= > >= && || ! ??");
            var tokens = lexer.ScanTokens();
            
            var expectedTypes = new[]
            {
                TokenType.Plus, TokenType.Minus, TokenType.Star, TokenType.Slash, 
                TokenType.Percent, TokenType.Power, TokenType.Equal, TokenType.StrictEqual,
                TokenType.NotEqual, TokenType.StrictNotEqual, TokenType.Less, TokenType.LessEqual,
                TokenType.Greater, TokenType.GreaterEqual, TokenType.And, TokenType.Or,
                TokenType.Not, TokenType.Nullish, TokenType.EOF
            };

            Assert.Equal(expectedTypes.Length, tokens.Count);
            for (int i = 0; i < expectedTypes.Length - 1; i++)
            {
                Assert.Equal(expectedTypes[i], tokens[i].Type);
            }
        }

        [Fact]
        public void TestStringLiterals()
        {
            var lexer = new Lexer.Lexer("\"hello\" 'world'");
            var tokens = lexer.ScanTokens();
            
            Assert.Equal(3, tokens.Count); // "hello", 'world', EOF
            Assert.Equal(TokenType.String, tokens[0].Type);
            Assert.Equal("hello", tokens[0].Value);
            Assert.Equal(TokenType.String, tokens[1].Type);
            Assert.Equal("world", tokens[1].Value);
        }

        [Fact]
        public void TestSpreadOperator()
        {
            var lexer = new Lexer.Lexer("...args");
            var tokens = lexer.ScanTokens();
            
            Assert.Equal(3, tokens.Count); // ..., args, EOF
            Assert.Equal(TokenType.Spread, tokens[0].Type);
            Assert.Equal("...", tokens[0].Value);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("args", tokens[1].Value);
        }

        [Fact]
        public void TestTemplateString()
        {
            var lexer = new Lexer.Lexer("`hello world`");
            var tokens = lexer.ScanTokens();
            
            Assert.Equal(2, tokens.Count); // template, EOF
            Assert.Equal(TokenType.TemplateString, tokens[0].Type);
            Assert.Equal("hello world", tokens[0].Value);
        }

        [Fact]
        public void TestArrayAndObjectLiterals()
        {
            var lexer = new Lexer.Lexer("[1, 2] {a: 1}");
            var tokens = lexer.ScanTokens();
            
            var expectedTypes = new[]
            {
                TokenType.LeftBracket, TokenType.Number, TokenType.Comma, TokenType.Number, TokenType.RightBracket,
                TokenType.LeftBrace, TokenType.Identifier, TokenType.Colon, TokenType.Number, TokenType.RightBrace,
                TokenType.EOF
            };

            Assert.Equal(expectedTypes.Length, tokens.Count);
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                Assert.Equal(expectedTypes[i], tokens[i].Type);
            }
        }
    }
}
