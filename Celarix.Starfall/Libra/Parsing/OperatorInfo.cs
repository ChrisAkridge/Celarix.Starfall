using Celarix.Starfall.Libra.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed record OperatorInfo(string Symbol,
        string RenderedSymbol,
        OperatorKind Kind,
        IWhenNoneRule? WhenNoneRule,
        IWhenSomeRule? WhenSomeRule)
    {
        public bool IsReservedName => Symbol.StartsWith(";");
    }
}
