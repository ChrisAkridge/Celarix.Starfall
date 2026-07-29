using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public static class LibraExpressionFactory
    {
        private static readonly SColor FC = SColor.White;
        private static readonly SColor BC = SColor.Transparent;

        public static TextExpression Text(string text) => new(text, FC, BC);
        public static TextExpression Text(string text, string id) => new(text, FC, BC, id);
        public static TextExpression Text(string text, SColor foregroundColor, SColor backgroundColor, string? id = null) => new(text, foregroundColor, backgroundColor, id);

        public static ParenthesizedExpression Paren(LibraExpression expression) => new(expression, FC, BC);
        public static ParenthesizedExpression Paren(LibraExpression expression, string id) => new(expression, FC, BC, id);
        public static ParenthesizedExpression Paren(LibraExpression expression, SColor foregroundColor, SColor backgroundColor, string? id = null) =>
            new(expression, foregroundColor, backgroundColor, id);

        public static ScriptsExpression Exp(LibraExpression baseExpression, LibraExpression superscript) => new(baseExpression, superscript, null, null);
        public static ScriptsExpression Exp(LibraExpression baseExpression, LibraExpression superscript, string? id) => new(baseExpression, superscript, null, id);

        public static ScriptsExpression Subscript(LibraExpression baseExpression, LibraExpression subscript) => new(baseExpression, null, subscript, null);
        public static ScriptsExpression Subscript(LibraExpression baseExpression, LibraExpression subscript, string? id) => new(baseExpression, null, subscript, id);

        public static ScriptsExpression ExpSub(LibraExpression baseExpression, LibraExpression superscript, LibraExpression subscript) => new(baseExpression, superscript, subscript, null);
        public static ScriptsExpression ExpSub(LibraExpression baseExpression, LibraExpression superscript, LibraExpression subscript, string? id) => new(baseExpression, superscript, subscript, id);

        public static FractionExpression Frac(LibraExpression numerator, LibraExpression denominator) => new(numerator, denominator, FC, BC);
        public static FractionExpression Frac(LibraExpression numerator, LibraExpression denominator, string? id) => new(numerator, denominator, FC, BC, id);
        public static FractionExpression Frac(LibraExpression numerator, LibraExpression denominator, SColor foregroundColor, SColor backgroundColor, string? id = null) =>
            new(numerator, denominator, foregroundColor, backgroundColor, id);

        public static BinaryExpression AddExpr(LibraExpression left, LibraExpression right) => new("+", left, right, FC, BC);
        public static BinaryExpression AddExpr(LibraExpression left, LibraExpression right, string? id) => new("+", left, right, FC, BC, id);
        public static BinaryExpression AddExpr(LibraExpression left, LibraExpression right, SColor foregroundColor, SColor backgroundColor, string? id = null) =>
            new("+", left, right, foregroundColor, backgroundColor, id);
    }
}
