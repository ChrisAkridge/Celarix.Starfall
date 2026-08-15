using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing;
using Celarix.Starfall.Libra.Parsing.Syntax;

namespace Celarix.Starfall.Tests.Libra.Parsing;

public sealed class LibraBinderTests
{
    [Fact]
    public void Build_FractionReservedCall_BindsArgumentsAsExpressions()
    {
        var fraction = Assert.IsType<FractionExpression>(Build(";frac(x,y)"));

        AssertText(fraction.Numerator, "x");
        AssertText(fraction.Denominator, "y");
    }

    [Fact]
    public void Build_UnaryPlus_BindsPrefixOperatorWithoutInfixAmbiguity()
    {
        var prefix = Assert.IsType<UnaryPrefixExpression>(Build("+x"));

        Assert.Equal("+", prefix.Operator);
        AssertText(prefix.Operand, "x");
    }

    [Fact]
    public void Build_BinaryPlus_BindsInfixOperatorWithoutPrefixAmbiguity()
    {
        var binary = Assert.IsType<BinaryExpression>(Build("x+y"));

        Assert.Equal("+", binary.Operator);
        AssertText(binary.Left, "x");
        AssertText(binary.Right, "y");
    }

    [Fact]
    public void Build_FractionReservedCallWithWrongArity_ThrowsBinderDiagnostic()
    {
        var exception = BuildThrows(";frac(x)");

        Assert.Contains(";frac requires exactly two arguments", exception.Diagnostic.Message);
        Assert.Equal(0, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Build_CatEmReservedCall_BindsGapAndExpressions()
    {
        var row = Assert.IsType<RowExpression>(Build(";catEm(2,x,y,z)"));

        Assert.Equal(2d, row.GapEm);
        Assert.Collection(row.Children,
            first => AssertText(first, "x"),
            second => AssertText(second, "y"),
            third => AssertText(third, "z"));
    }

    [Fact]
    public void Build_CatEmReservedCall_AllowsDecimalGap()
    {
        var row = Assert.IsType<RowExpression>(Build(";catEm(2.5,x,y)"));

        Assert.Equal(2.5d, row.GapEm);
    }

    [Fact]
    public void Build_CatEmReservedCall_AllowsNegativeGap()
    {
        var row = Assert.IsType<RowExpression>(Build(";catEm(-2,x,y)"));

        Assert.Equal(-2d, row.GapEm);
    }

    [Fact]
    public void Build_CatEmReservedCallWithTooFewArguments_ThrowsBinderDiagnostic()
    {
        var exception = BuildThrows(";catEm(2)");

        Assert.Contains(";catEm requires a gap followed by at least one expression", exception.Diagnostic.Message);
        Assert.Equal(0, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Build_CatEmReservedCallWithExpressionGap_ThrowsNumericLiteralDiagnostic()
    {
        var exception = BuildThrows(";catEm(1+1,x)");

        Assert.Contains("Expected a numeric literal", exception.Diagnostic.Message);
        Assert.Equal(7, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Parse_UnknownReservedCall_StillProducesReservedCallSyntax()
    {
        var call = Assert.IsType<ReservedCallSyntax>(Parse(";unknown(x,y)"));

        Assert.Equal(";unknown", call.Name);
        Assert.Collection(call.Arguments,
            first => AssertTextSyntax(first, "x"),
            second => AssertTextSyntax(second, "y"));
    }

    [Fact]
    public void Build_UnknownReservedCall_ThrowsBinderDiagnostic()
    {
        var exception = BuildThrows(";unknown(x,y)");

        Assert.Contains("Unknown reserved function ';unknown'", exception.Diagnostic.Message);
        Assert.Equal(0, exception.Diagnostic.textSpan?.StartIndex);
    }

    [Fact]
    public void Build_DelphinusSlideBezierExpression_BindsExpectedExpressionTree()
    {
        const string source = ";catEm(2, mt = 1 - t, (mt^2 * X_0) + (2 * mt * t * X_1) + (t^2 * X_2))";

        var row = Assert.IsType<RowExpression>(Build(source));

        Assert.Equal(2d, row.GapEm);
        Assert.Collection(row.Children,
            AssertBezierComplementDefinition,
            AssertQuadraticBezierBlend);
    }

    [Fact]
    public void Build_FractionWithPolynomialArguments_BindsNestedBinaryAndScriptExpressions()
    {
        var fraction = Assert.IsType<FractionExpression>(Build(";frac(x^2+1,y_0-3)"));

        var numerator = AssertBinary(fraction.Numerator, "+");
        AssertSuperscript(numerator.Left, "x", "2");
        AssertText(numerator.Right, "1");

        var denominator = AssertBinary(fraction.Denominator, "-");
        AssertSubscript(denominator.Left, "y", "0");
        AssertText(denominator.Right, "3");
    }

    [Fact]
    public void Build_NestedFraction_BindsReservedCallsRecursively()
    {
        var outer = Assert.IsType<FractionExpression>(Build(";frac(;frac(a,b),c)"));

        var inner = Assert.IsType<FractionExpression>(outer.Numerator);
        AssertText(inner.Numerator, "a");
        AssertText(inner.Denominator, "b");
        AssertText(outer.Denominator, "c");
    }

    [Fact]
    public void Build_ParenthesizedExpression_BindsAsFencedExpression()
    {
        var fenced = Assert.IsType<FencedExpression>(Build("(x+y)"));

        AssertBinary(fenced.Expression, "+");
    }

    [Fact]
    public void Build_ExpressionWithBothScripts_BindsSingleScriptsExpression()
    {
        var script = Assert.IsType<ScriptsExpression>(Build("x_i^2"));

        AssertText(script.BaseExpression, "x");
        AssertText(script.Subscript!, "i");
        AssertText(script.Superscript!, "2");
    }

    private static LibraExpression Build(string source)
    {
        return LibraExpression.Parse(source).Build();
    }

    private static LibraParseException BuildThrows(string source)
    {
        return Assert.Throws<LibraParseException>(() => Build(source));
    }

    private static ExpressionSyntax Parse(string source)
    {
        var tokens = new Lexer(source).Parse();
        return new LibraParser(tokens).Parse();
    }

    private static TextExpression AssertText(LibraExpression expression,
        string expected)
    {
        var text = Assert.IsType<TextExpression>(expression);
        Assert.Equal(expected, text.Text);
        return text;
    }

    private static TextSyntax AssertTextSyntax(ExpressionSyntax syntax,
        string expected)
    {
        var text = Assert.IsType<TextSyntax>(syntax);
        Assert.Equal(expected, text.Text);
        return text;
    }

    private static void AssertBezierComplementDefinition(LibraExpression expression)
    {
        var equals = AssertBinary(expression, "=");
        AssertText(equals.Left, "mt");

        var subtraction = AssertBinary(equals.Right, "-");
        AssertText(subtraction.Left, "1");
        AssertText(subtraction.Right, "t");
    }

    private static void AssertQuadraticBezierBlend(LibraExpression expression)
    {
        var terms = FlattenBinary(expression, "+");

        Assert.Collection(terms,
            AssertQuadraticStartTerm,
            AssertQuadraticControlTerm,
            AssertQuadraticEndTerm);
    }

    private static void AssertQuadraticStartTerm(LibraExpression expression)
    {
        var fenced = Assert.IsType<FencedExpression>(expression);
        var factors = FlattenBinary(fenced.Expression, "·");

        Assert.Collection(factors,
            factor => AssertSuperscript(factor, "mt", "2"),
            factor => AssertSubscript(factor, "X", "0"));
    }

    private static void AssertQuadraticControlTerm(LibraExpression expression)
    {
        var fenced = Assert.IsType<FencedExpression>(expression);
        var factors = FlattenBinary(fenced.Expression, "·");

        Assert.Collection(factors,
            factor => AssertText(factor, "2"),
            factor => AssertText(factor, "mt"),
            factor => AssertText(factor, "t"),
            factor => AssertSubscript(factor, "X", "1"));
    }

    private static void AssertQuadraticEndTerm(LibraExpression expression)
    {
        var fenced = Assert.IsType<FencedExpression>(expression);
        var factors = FlattenBinary(fenced.Expression, "·");

        Assert.Collection(factors,
            factor => AssertSuperscript(factor, "t", "2"),
            factor => AssertSubscript(factor, "X", "2"));
    }

    private static BinaryExpression AssertBinary(LibraExpression expression,
        string expectedOperator)
    {
        var binary = Assert.IsType<BinaryExpression>(expression);
        Assert.Equal(expectedOperator, binary.Operator);
        return binary;
    }

    private static ScriptsExpression AssertSuperscript(LibraExpression expression,
        string expectedBase,
        string expectedSuperscript)
    {
        var script = Assert.IsType<ScriptsExpression>(expression);
        AssertText(script.BaseExpression, expectedBase);
        AssertText(script.Superscript!, expectedSuperscript);
        Assert.Null(script.Subscript);
        return script;
    }

    private static ScriptsExpression AssertSubscript(LibraExpression expression,
        string expectedBase,
        string expectedSubscript)
    {
        var script = Assert.IsType<ScriptsExpression>(expression);
        AssertText(script.BaseExpression, expectedBase);
        Assert.Null(script.Superscript);
        AssertText(script.Subscript!, expectedSubscript);
        return script;
    }

    private static IReadOnlyList<LibraExpression> FlattenBinary(LibraExpression expression,
        string operatorText)
    {
        if (expression is not BinaryExpression binary
            || binary.Operator != operatorText)
        {
            return [expression];
        }

        return [.. FlattenBinary(binary.Left, operatorText), .. FlattenBinary(binary.Right, operatorText)];
    }
}
