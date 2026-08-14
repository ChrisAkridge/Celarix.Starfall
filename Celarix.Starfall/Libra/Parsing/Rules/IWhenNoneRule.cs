using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal interface IWhenNoneRule
    {
        int BindingPower { get; }
        ExpressionSyntax Parse(LibraParser parser, LibraToken operatorToken);
    }
}
