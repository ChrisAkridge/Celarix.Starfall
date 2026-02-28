using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public abstract class ResizableHeliumElement : HeliumElement
    {
        public abstract void SetDesiredWidthFraction(double widthFraction);
        public abstract void SetDesiredHeightFraction(double heightFraction);
    }
}
