using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal abstract class LibraToken
    {
        
    }

    internal sealed class FlatToken : LibraToken
    {
        // Represents a token that is not made of other tokens, at least at this time.
        public TokenKind Kind { get; }
        public string Text { get; }

        public FlatToken(TokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }
    }
}
