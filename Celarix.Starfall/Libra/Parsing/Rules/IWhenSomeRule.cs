using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal interface IWhenSomeRule
    {
        int LeftBindingPower { get; }

        ExpressionSyntax Parse(LibraParser parser, ExpressionSyntax left, LibraToken operatorToken);
    }
}
