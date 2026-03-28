using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class LargeOperatorElement : Token
    {
        public AtomicBox? Upper { get; }
        public AtomicBox? Lower { get; }
        public AtomicBox? Expression { get; }
        public LargeOperatorKind Kind { get; }

        public LargeOperatorElement(StyleContext styleContext, LargeOperatorKind kind, AtomicBox? upper, AtomicBox? lower, AtomicBox? expression)
        {
            StyleContext = styleContext;
            Kind = kind;
            Upper = upper;
            Lower = lower;
            Expression = expression;
        }
    }
}
