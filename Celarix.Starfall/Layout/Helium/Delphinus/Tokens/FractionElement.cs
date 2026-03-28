using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class FractionElement : Token
    {
        public AtomicBox? Numerator { get; }
        public AtomicBox? Denominator { get; }
        public FractionElement(StyleContext styleContext, AtomicBox? numerator, AtomicBox? denominator)
        {
            StyleContext = styleContext;
            Numerator = numerator;
            Denominator = denominator;
        }
    }
}
