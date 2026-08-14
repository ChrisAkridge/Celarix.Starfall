using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Rules
{
    internal sealed class ReservedFunctionWhenNoneRule : IWhenNoneRule
    {
        public int BindingPower => 100;

        public ExpressionSyntax Parse(LibraParser parser, LibraToken operatorToken)
        {
            parser.Expect(TokenKind.OpenParen);

            var arguments = new List<ExpressionSyntax>();

            if (parser.Peek().Kind != TokenKind.CloseParen)
            {
                while (true)
                {
                    arguments.Add(parser.ParseExpression(0));

                    if (parser.Peek().Kind == TokenKind.CloseParen)
                    {
                        break;
                    }

                    parser.Expect(TokenKind.Comma);
                }
            }

            var closeParen = parser.Expect(TokenKind.CloseParen);

            return new ReservedCallSyntax(operatorToken.Text, arguments, TextSpan.FromBounds(operatorToken.Span, closeParen.Span));
        }
    }
}
