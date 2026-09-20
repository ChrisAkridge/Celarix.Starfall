using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    public sealed record TextSpan(string Text,
        int StartIndex,
        int Length)
    {
        public int EndIndex => StartIndex + Length;

        public static TextSpan FromBounds(TextSpan left, TextSpan right)
        {
            if (!left.Text.Equals(right.Text))
            {
                throw new ArgumentException("Cannot create a TextSpan from bounds of two different texts.");
            }

            var startIndex = Math.Min(left.StartIndex, right.StartIndex);
            var endIndex = Math.Max(left.EndIndex, right.EndIndex);
            return new TextSpan(left.Text, startIndex, endIndex - startIndex);
        }
    }
}
