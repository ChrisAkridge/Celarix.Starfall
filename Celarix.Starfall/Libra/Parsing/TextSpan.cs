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
    }
}
