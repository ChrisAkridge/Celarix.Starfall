using Celarix.Starfall.Layout.Helium.Layout;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public sealed class RectangleElement : ResizableHeliumElement
    {
        private double desiredWidthFraction = Constants.DefaultSize;
        private double desiredHeightFraction = Constants.DefaultSize;

        public override IReadOnlyList<HeliumElement> Children => Array.Empty<HeliumElement>();
        public override double DesiredWidthFraction => desiredWidthFraction;
        public override double DesiredHeightFraction => desiredHeightFraction;
        public SColor Color { get; set; }

        public override PositionedHeliumElement Measure(SSizeF maxSize)
        {
            var width = maxSize.Width * (double)DesiredWidthFraction;
            var height = maxSize.Height * (double)DesiredHeightFraction;
            return new PositionedHeliumElement(this, new SSizeF(width, height));
        }

        public override SPointF Arrange(SPointF parentPosition, SPointF? parentSize, SSizeF thisSize)
        {
            return AlignmentHelper.Align()
        }

        public override void SetDesiredWidthFraction(double widthFraction)
        {
            desiredWidthFraction = widthFraction;
        }

        public override void SetDesiredHeightFraction(double heightFraction)
        {
            desiredHeightFraction = heightFraction;
        }
    }
}
