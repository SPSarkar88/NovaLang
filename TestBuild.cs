using NovaLang.AST;

// Simple test to see what types can be found
public class TestBuild
{
    public void Test()
    {
        var lit = new LiteralExpression(null, NovaLang.Lexer.TokenType.Number, default);
    }
}
