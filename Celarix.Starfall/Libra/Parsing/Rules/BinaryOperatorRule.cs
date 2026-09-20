using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal sealed class BinaryOperatorRule : IWhenSomeRule
    {
        public int LeftBindingPower { get; }
        public int RightBindingPower { get; }

        public BinaryOperatorRule(int leftBindingPower, int rightBindingPower)
        {
            LeftBindingPower = leftBindingPower;
            RightBindingPower = rightBindingPower;
        }

        public static BinaryOperatorRule LeftAssociative(int leftBindingPower)
        {
            return new BinaryOperatorRule(leftBindingPower, leftBindingPower + 1);
        }

        public static BinaryOperatorRule RightAssociative(int leftBindingPower)
        {
            return new BinaryOperatorRule(leftBindingPower, leftBindingPower);
        }

        public ExpressionSyntax Parse(LibraParser parser, ExpressionSyntax left, LibraToken operatorToken)
        {
            var right = parser.ParseExpression(RightBindingPower);

            return new BinarySyntax(operatorToken.Text, left, right, TextSpan.FromBounds(left.Span, right.Span));
        }
    }
}
