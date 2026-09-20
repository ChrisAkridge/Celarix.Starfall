using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal sealed class PropertyBlockRule : IWhenSomeRule
    {
        public int LeftBindingPower => OperatorRegistry.CallAndPostfixBlockBindingPower;

        public ExpressionSyntax Parse(LibraParser parser, ExpressionSyntax left, LibraToken operatorToken)
        {
            return new PropertyBlockSyntax(left, operatorToken.Text, TextSpan.FromBounds(left.Span, operatorToken.Span));
        }
    }
}
