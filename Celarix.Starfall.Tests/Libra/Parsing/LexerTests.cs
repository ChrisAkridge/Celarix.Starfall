using Celarix.Starfall.Libra.Parsing;

namespace Celarix.Starfall.Tests.Libra.Parsing;

public sealed class LexerTests
{
    [Fact]
    public void Parse_BareTextAtom_EmitsTextAndEndOfInput()
    {
        var tokens = Lex("xy");

        AssertTokens(tokens,
            (TokenKind.Text, "xy"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_DecimalTextAtom_AllowsDecimalPoint()
    {
        var tokens = Lex("2.5");

        AssertTokens(tokens,
            (TokenKind.Text, "2.5"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_WhitespaceOutsideStrings_IsDiscarded()
    {
        var spaced = Lex("x   +   y");
        var compact = Lex("x+y");

        AssertTokenSequenceEqual(compact, spaced);
    }

    [Fact]
    public void Parse_QuotedString_PreservesSpacesAndPunctuation()
    {
        var tokens = Lex("\" hello; world \"");

        AssertTokens(tokens,
            (TokenKind.String, "\" hello; world \""),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_QuotedString_DecodesEscapedQuoteAndBackslash()
    {
        var tokens = Lex("\"a\\\"b\\\\c\"");

        AssertTokens(tokens,
            (TokenKind.String, "\"a\\\"b\\\\c\""),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_ReservedName_IncludesSemicolonInTokenText()
    {
        var tokens = Lex(";frac(x,y)");

        AssertTokens(tokens,
            (TokenKind.ReservedName, ";frac"),
            (TokenKind.OpenParen, "("),
            (TokenKind.Text, "x"),
            (TokenKind.Comma, ","),
            (TokenKind.Text, "y"),
            (TokenKind.CloseParen, ")"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_ReservedOperator_IncludesSemicolonInTokenText()
    {
        var tokens = Lex("x;equaldef y");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.ReservedName, ";equaldef"),
            (TokenKind.Text, "y"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_LongestOperator_UsesSingleOperatorToken()
    {
        var tokens = Lex("a<=b");

        AssertTokens(tokens,
            (TokenKind.Text, "a"),
            (TokenKind.Operator, "<="),
            (TokenKind.Text, "b"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_CommaOutsideReservedCall_IsStillTokenized()
    {
        var tokens = Lex("a,b");

        AssertTokens(tokens,
            (TokenKind.Text, "a"),
            (TokenKind.Comma, ","),
            (TokenKind.Text, "b"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_Substitution_EmitsNameWithDelimiters()
    {
        var tokens = Lex("[[name]]");

        AssertTokens(tokens,
            (TokenKind.Substitution, "[[name]]"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_SubstitutionWithPostfixIdentifier_TokenizesForValidator()
    {
        var tokens = Lex("[[name]]@#id");

        AssertTokens(tokens,
            (TokenKind.Substitution, "[[name]]"),
            (TokenKind.IdentifierBlock, "@#id"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_IdentifierBlock_EmitsRawBlockWithoutAtSign()
    {
        var tokens = Lex("x@#id.class1.class2");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.IdentifierBlock, "@#id.class1.class2"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_PropertyBlock_EmitsRawBlockWithoutBrackets()
    {
        var tokens = Lex("x[foreground=ff0000,fencetype=SquareBrackets]");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.PropertyBlock, "[foreground=ff0000,fencetype=SquareBrackets]"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_RepeatedPostfixBlocks_TokenizesForValidator()
    {
        var tokens = Lex("x@#id[color=red]@.class2");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.IdentifierBlock, "@#id"),
            (TokenKind.PropertyBlock, "[color=red]"),
            (TokenKind.IdentifierBlock, "@.class2"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_FenceTypeOnText_TokenizesForBinderDiagnostic()
    {
        var tokens = Lex("x[fencetype=SquareBrackets]");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.PropertyBlock, "[fencetype=SquareBrackets]"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_UnknownReservedName_TokenizesForParserOrBinderDiagnostic()
    {
        var tokens = Lex("x;y");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.ReservedName, ";y"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_UnknownOperator_TokenizesForParserDiagnostic()
    {
        var tokens = Lex("x!y");

        AssertTokens(tokens,
            (TokenKind.Text, "x"),
            (TokenKind.Operator, "!"),
            (TokenKind.Text, "y"),
            (TokenKind.EndOfInput, ""));
    }

    [Fact]
    public void Parse_UnquotedPunctuation_ThrowsParseException()
    {
        Assert.Throws<LibraParseException>(() => Lex("x:y"));
    }

    [Fact]
    public void Parse_BareSemicolon_ThrowsParseException()
    {
        Assert.Throws<LibraParseException>(() => Lex(";"));
    }

    [Fact]
    public void Parse_UnterminatedString_ThrowsParseExceptionWithDiagnostic()
    {
        var exception = Assert.Throws<LibraParseException>(() => Lex("\"unterminated"));

        Assert.NotNull(exception.Diagnostic);
        Assert.Contains("Unterminated", exception.Diagnostic.Message);
    }

    [Fact]
    public void Parse_UnterminatedSubstitution_ThrowsParseExceptionWithDiagnostic()
    {
        var exception = Assert.Throws<LibraParseException>(() => Lex("[[name"));

        Assert.NotNull(exception.Diagnostic);
        Assert.Contains("Unterminated", exception.Diagnostic.Message);
    }

    [Fact]
    public void Parse_UnterminatedPropertyBlock_ThrowsParseExceptionWithDiagnostic()
    {
        var exception = Assert.Throws<LibraParseException>(() => Lex("x[color=red"));

        Assert.NotNull(exception.Diagnostic);
        Assert.Contains("Unterminated", exception.Diagnostic.Message);
    }

    private static IReadOnlyList<LibraToken> Lex(string source)
    {
        return new Lexer(source).Parse();
    }

    private static void AssertTokens(IReadOnlyList<LibraToken> actual,
        params (TokenKind Kind, string Text)[] expected)
    {
        Assert.Equal(expected.Length, actual.Count);

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Kind, actual[i].Kind);
            Assert.Equal(expected[i].Text, actual[i].Text);
        }
    }

    private static void AssertTokenSequenceEqual(IReadOnlyList<LibraToken> expected,
        IReadOnlyList<LibraToken> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Kind, actual[i].Kind);
            Assert.Equal(expected[i].Text, actual[i].Text);
        }
    }
}
