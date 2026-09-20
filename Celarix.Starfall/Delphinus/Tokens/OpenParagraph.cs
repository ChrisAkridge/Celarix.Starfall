using Celarix.Starfall.Delphinus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Delphinus.Tokens
{
    internal sealed class OpenParagraph : Token
    {
        public OpenParagraph(StyleContext styleContext)
        {
            StyleContext = styleContext;
        }
    }
}
