using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Passes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    public static class LibraParser
    {
        public static LibraExpression Parse(string text)
        {
            if (TryParse(text, out var expression, out var diagnostic))
            {
                return expression;
            }
            throw new LibraParseException(diagnostic);
        }

        public static bool TryParse(string text,
            [NotNullWhen(true)] out LibraExpression? expression,
            [NotNullWhen(false)] out LibraDiagnostic? diagnostic)
        {
            var passes = new IParsePass[]
            {
                new TextLiteralSeparationPass()
            };

            var tokenList = new List<LibraToken>
            {
                new FlatToken(TokenKind.Unresolved, text)
            };

            foreach (var pass in passes)
            {
                if (!pass.TryPass(tokenList, out var parsedTokens, out diagnostic))
                {
                    expression = null;
                    return false;
                }
                tokenList = [.. parsedTokens];
            }
            
            throw new NotImplementedException("Going a different direction.");
        }
    }
}
