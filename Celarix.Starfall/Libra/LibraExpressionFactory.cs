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
