using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal sealed class PrefixOperatorRule : IWhenNoneRule
    {
        public int BindingPower { get; }

        public PrefixOperatorRule(int bindingPower)
        {
            BindingPower = bindingPower;
        }

        public ExpressionSyntax Parse(LibraParser parser, LibraToken operatorToken)
        {
            var operand = parser.ParseExpression(BindingPower);

            return new PrefixSyntax(operatorToken.Text, operand, TextSpan.FromBounds(operatorToken.Span, operand.Span));
        }
    }
}
