using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class GlyphRun : Token
    {
        public string Text { get; }

        public GlyphRun(string text, StyleContext styleContext)
        {
            Text = text;
            StyleContext = styleContext;
        }
    }
}
