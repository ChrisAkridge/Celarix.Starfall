using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class ImageToken : Token
    {
        public string Source { get; }

        public ImageToken(StyleContext styleContext, string source)
        {
            Source = source;
            StyleContext = styleContext;
        }
    }
}
