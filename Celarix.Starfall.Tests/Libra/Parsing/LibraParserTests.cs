using Celarix.Starfall.Libra.Parsing;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Tests.Libra.Parsing;

public sealed class LibraParserTests
{
    [Fact]
    public void Parse_TextAtom_ReturnsTextSyntax()
    {
        var text = Assert.IsType<TextSyntax>(Parse("abc123"));

        Assert.Equal("abc123", text.Text);
        AssertSpan(text, "abc123", 0, 6);
    }

    [Fact]
    public void Parse_String_ReturnsStringSyntaxWithSourceText()
    {
        var source = "\"hello world\"";

        var text = Assert.IsType<StringSyntax>(Parse(source));

        Assert.Equal(source, text.Text);
        AssertSpan(text, source, 0, source.Length);
    }

    [Fact]
    public void Parse_MultiplicationBindsTighterThanAddition()
    {
        var binary = AssertBinary(Parse("x+y*z"), "+");

        AssertText(binary.Left, "x");
        var right = AssertBinary(binary.Right, "*");
        AssertText(right.Left, "y");
        AssertText(right.Right, "z");
    }

    [Fact]
    public void Parse_LeftAssociativeBinaryOperators_GroupToTheLeft()
    {
        var binary = AssertBinary(Parse("x-y-z"), "-");

        var left = AssertBinary(binary.Left, "-");
        AssertText(left.Left, "x");
        AssertText(left.Right, "y");
        AssertText(binary.Right, "z");
    }

    [Fact]
    public void Parse_ParenthesesOverrideBinaryPrecedence()
    {
        var binary = AssertBinary(Parse("(x+y)*z"), "*");

        var group = Assert.IsType<ParenthesizedExpressionSyntax>(binary.Left);
        AssertBinary(group.Expression, "+");
        AssertText(binary.Right, "z");
    }

    [Fact]
    public void Parse_PrefixBindsTighterThanAddition()
    {
        var binary = AssertBinary(Parse("-x+y"), "+");

        var prefix = Assert.IsType<PrefixSyntax>(binary.Left);
        Assert.Equal("-", prefix.Operator);
        AssertText(prefix.Operand, "x");
        AssertText(binary.Right, "y");
    }

    [Fact]
    public void Parse_SuperscriptThenSubscript_AttachesBothScriptsToBase()
    {
        var source = "x^y_z";

        var script = Assert.IsType<ScriptSyntax>(Parse(source));

        AssertText(script.Base, "x");
        AssertText(script.Superscript, "y");
        AssertText(script.Subscript, "z");
        AssertSpan(script, source, 0, source.Length);
    }

    [Fact]
    public void Parse_SubscriptThenSuperscript_AttachesBothScriptsToBase()
    {
        var source = "x_y^z";

        var script = Assert.IsType<ScriptSyntax>(Parse(source));

        AssertText(script.Base, "x");
        AssertText(script.Superscript, "z");
        AssertText(script.Subscript, "y");
        AssertSpan(script, source, 0, source.Length);
    }

    [Fact]
    public void Parse_ChainedSuperscript_ThrowsDiagnostic()
    {
        var exception = ParseThrows("x^y^z");

        Assert.Contains("Cannot chain superscripts", exception.Diagnostic.Message);
        Assert.Equal(3, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_ChainedSubscript_ThrowsDiagnostic()
    {
        var exception = ParseThrows("x_y_z");

        Assert.Contains("Cannot chain subscripts", exception.Diagnostic.Message);
        Assert.Equal(3, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_SpanIncludesBothDelimiters()
    {
        var source = "(x)";

        var group = Assert.IsType<ParenthesizedExpressionSyntax>(Parse(source));

        AssertText(group.Expression, "x");
        AssertSpan(group, source, 0, source.Length);
    }

    [Fact]
    public void Parse_BracedExpression_SpanIncludesBothDelimiters()
    {
        var source = "{x}";

        var group = Assert.IsType<BracedExpressionSyntax>(Parse(source));

        AssertText(group.Expression, "x");
        AssertSpan(group, source, 0, source.Length);
    }

    [Fact]
    public void Parse_ReservedFunctionCall_AllowsCommaSeparatedArguments()
    {
        var source = ";frac(x,y)";

        var call = Assert.IsType<ReservedCallSyntax>(Parse(source));

        Assert.Equal(";frac", call.Name);
        Assert.Collection(call.Arguments,
            first => AssertText(first, "x"),
            second => AssertText(second, "y"));
        AssertSpan(call, source, 0, source.Length);
    }

    [Fact]
    public void Parse_UnknownReservedNameAfterExpression_ThrowsUnexpectedTokenDiagnostic()
    {
        var exception = ParseThrows("x;y");

        Assert.Contains("Unexpected token ';y'", exception.Diagnostic.Message);
        Assert.Equal(1, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_UnknownOperatorAfterExpression_ThrowsUnexpectedTokenDiagnostic()
    {
        var exception = ParseThrows("x!y");

        Assert.Contains("Unexpected token '!'", exception.Diagnostic.Message);
        Assert.Equal(1, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_CommaAtTopLevel_ThrowsCommaDiagnostic()
    {
        var exception = ParseThrows("x,y");

        AssertCommaDiagnostic(exception);
        Assert.Equal(1, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_CommaInsideParentheses_ThrowsCommaDiagnostic()
    {
        var exception = ParseThrows("(x,y)");

        AssertCommaDiagnostic(exception);
        Assert.Equal(2, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_MissingCloseParen_ThrowsExpectedCloseParenDiagnostic()
    {
        var exception = ParseThrows("(x+y");

        Assert.Contains("Expected token of kind CloseParen", exception.Diagnostic.Message);
        Assert.Equal(4, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_EmptyInput_ThrowsUnexpectedEndOfInputDiagnostic()
    {
        var exception = ParseThrows("");

        Assert.Contains("Unexpected token '' of kind EndOfInput", exception.Diagnostic.Message);
    }

    private static ExpressionSyntax Parse(string source)
    {
        var tokens = new Lexer(source).Parse();
        return new LibraParser(tokens).Parse();
    }

    private static LibraParseException ParseThrows(string source)
    {
        return Assert.Throws<LibraParseException>(() => Parse(source));
    }

    private static TextSyntax AssertText(ExpressionSyntax? syntax, string expectedText)
    {
        var text = Assert.IsType<TextSyntax>(syntax);
        Assert.Equal(expectedText, text.Text);
        return text;
    }

    private static BinarySyntax AssertBinary(ExpressionSyntax syntax, string expectedOperator)
    {
        var binary = Assert.IsType<BinarySyntax>(syntax);
        Assert.Equal(expectedOperator, binary.Operator);
        return binary;
    }

    private static void AssertSpan(ExpressionSyntax syntax,
        string source,
        int startIndex,
        int length)
    {
        Assert.Equal(source, syntax.Span.Text);
        Assert.Equal(startIndex, syntax.Span.StartIndex);
        Assert.Equal(length, syntax.Span.Length);
    }

    private static void AssertCommaDiagnostic(LibraParseException exception)
    {
        Assert.Contains("Unexpected comma", exception.Diagnostic.Message);
        Assert.Contains("reserved function calls", exception.Diagnostic.Message);
    }
}
