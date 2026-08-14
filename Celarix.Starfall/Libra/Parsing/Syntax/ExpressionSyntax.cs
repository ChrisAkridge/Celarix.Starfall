using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Syntax
{
    internal abstract record ExpressionSyntax(TextSpan Span);

    internal sealed record TextSyntax(string Text,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record StringSyntax(string Text,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record PrefixSyntax(string Operator,
        ExpressionSyntax Operand,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record BinarySyntax(string Operator,
        ExpressionSyntax Left,
        ExpressionSyntax Right,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record ScriptSyntax(ExpressionSyntax Base,
        ExpressionSyntax? Superscript,
        ExpressionSyntax? Subscript,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record ReservedNameSyntax(string Name,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record ReservedCallSyntax(string Name,
        IReadOnlyList<ExpressionSyntax> Arguments,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record IdentifierSyntax(ExpressionSyntax Expression,
        string IdentifierBlock,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record PropertyBlockSyntax(ExpressionSyntax Expression,
        string PropertyBlock,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record SubstitutionSyntax(string Name,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record ParenthesizedExpressionSyntax(ExpressionSyntax Expression,
        TextSpan Span) : ExpressionSyntax(Span);

    internal sealed record BracedExpressionSyntax(ExpressionSyntax Expression,
        TextSpan Span) : ExpressionSyntax(Span);
}
