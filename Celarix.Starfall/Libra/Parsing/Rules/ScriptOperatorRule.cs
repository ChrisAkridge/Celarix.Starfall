using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal sealed class ScriptOperatorRule : IWhenSomeRule
    {
        public int LeftBindingPower => OperatorRegistry.ScriptBindingPower;

        public ExpressionSyntax Parse(LibraParser parser, ExpressionSyntax left, LibraToken operatorToken)
        {
            ExpressionSyntax? superscript = null;
            ExpressionSyntax? subscript = null;
            ExpressionSyntax? lastScript = null;

            if (operatorToken.Text == "^")
            {
                superscript = parser.ParseExpression(OperatorRegistry.ScriptBindingPower + 1);
                lastScript = superscript;
                var peek = parser.Peek();
                if (peek.Kind == TokenKind.Operator)
                {
                    if (peek.Text == "_")
                    {
                        parser.Expect(TokenKind.Operator);
                        subscript = parser.ParseExpression(OperatorRegistry.ScriptBindingPower + 1);
                        lastScript = subscript;
                    }
                    else if (peek.Text == "^")
                    {
                        throw new LibraParseException(new("Cannot chain superscripts in this manner - use x^{y^z} instead of x^y^z", peek.Span));
                    }
                }
            }
            else if (operatorToken.Text == "_")
            {
                subscript = parser.ParseExpression(OperatorRegistry.ScriptBindingPower + 1);
                lastScript = subscript;
                var peek = parser.Peek();
                if (peek.Kind == TokenKind.Operator)
                {
                    if (peek.Text == "^")
                    {
                        parser.Expect(TokenKind.Operator);
                        superscript = parser.ParseExpression(OperatorRegistry.ScriptBindingPower + 1);
                        lastScript = superscript;
                    }
                    else if (peek.Text == "_")
                    {
                        throw new LibraParseException(new("Cannot chain subscripts in this manner - use x_{y_z} instead of x_y_z", peek.Span));
                    }
                }
            }

            return new ScriptSyntax(left, superscript, subscript, TextSpan.FromBounds(left.Span, (lastScript ?? left).Span));
        }
    }
}
