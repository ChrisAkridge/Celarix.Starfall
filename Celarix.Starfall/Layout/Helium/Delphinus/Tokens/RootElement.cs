using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    /// <summary>
    /// Represents a &lt;root&gt; element in the Delphinus layout. This is a math element representing an Nth
    /// root.
    /// </summary>
    internal sealed class RootElement : Token
    {
        public AtomicBox? Index { get; }
        public AtomicBox? Radicand { get; }

        public RootElement(StyleContext styleContext, AtomicBox? index, AtomicBox? radicand)
        {
            StyleContext = styleContext;
            Index = index;
            Radicand = radicand;
        }
    }
}
