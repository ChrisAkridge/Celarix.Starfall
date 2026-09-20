using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal readonly record struct LibraToken
    {
        public TokenKind Kind { get; }
        public string Text { get; }
        public TextSpan Span { get; }

        public LibraToken(TokenKind kind, string text, TextSpan span)
        {
            Kind = kind;
            Text = text;
            Span = span;
        }
    }
}
