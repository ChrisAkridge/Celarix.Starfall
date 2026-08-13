using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Passes
{
    internal sealed class MetaSeparationPass : IParsePass
    {
        internal enum State
        {
            Start,
            Token,
            Identifier,
            PropertyBlock,
            Substitution
        }

        private State _state = State.Start;
        private readonly List<LibraToken> _parsedTokens = new();
        private readonly StringBuilder _tokenBuilder = new();

        public bool TryPass(IReadOnlyList<LibraToken> tokens,
            [NotNullWhen(true)] out IReadOnlyList<LibraToken>? parsedTokens,
            [NotNullWhen(false)] out LibraDiagnostic? diagnostic)
        {
            if (InputValid(tokens) is LibraDiagnostic inputValid)
            {
                parsedTokens = null;
                diagnostic = inputValid;
                return false;
            }

            foreach (var token in tokens)
            {
                if (token is FlatToken flatToken && flatToken.Kind == TokenKind.TextLiteral)
                {
                    _parsedTokens.Append(flatToken);
                    _state = State.Token;
                }
            }

            // Going a different direction here.
            throw new NotImplementedException();
        }

        

        private LibraDiagnostic? InputValid(IReadOnlyList<LibraToken> tokens)
        {
            if (tokens.Count == 0)
            {
                return new LibraDiagnostic("Internal parser error: Meta separation pass expects a non-empty list of tokens as input.", null);
            }
            return null;
        }
    }
}
