using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_05_ButWellJustPickBinary : AtriaSlide
    {
        public SlideFP_05_ButWellJustPickBinary(int width, int height) : base(width, height)
        {
            var tempElement = new FloatingPointWindowElement("#tempElement")
            {
                Size = Size
            };
            var anchor = new BasisPoint(TopLeft, "#tempAnchor");
            tempElement.AnchorTopLeftTo(anchor);
            Add([anchor, tempElement]);
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
        }
    }
}
