using Celarix.Starfall.Layout.Helium.Layout;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements.Containers
{
    public sealed class SingleElementContainer : HeliumElement
    {
        public override IReadOnlyList<HeliumElement> Children => Child != null ? [Child] : Array.Empty<HeliumElement>();
        public double LeftPadding { get; set; }
        public double TopPadding { get; set; }
        public double RightPadding { get; set; }
        public double BottomPadding { get; set; }
        public Alignment Alignment { get; set; } = Alignment.Center;
        public HeliumElement? Child { get; set; }
        public override double DesiredWidthFraction => Constants.FullSize;
        public override double DesiredHeightFraction => Constants.FullSize;

        public override PositionedHeliumElement Measure(SSizeF maxSize)
        {
            // The container itself always fills the available space.
            var ownSize = maxSize;

            // If there's no child, we can just return the container's size.
            if (Child == null)
            {
                return new PositionedHeliumElement(this, ownSize);
            }

            // Calculate the available size for the child by subtracting padding.
            var availableWidth = Math.Max(0, ownSize.Width - LeftPadding - RightPadding);
            var availableHeight = Math.Max(0, ownSize.Height - TopPadding - BottomPadding);
            var positionedChild = Child.Measure(new SSizeF(availableWidth, availableHeight));
            var positionedSelf = new PositionedHeliumElement(this, ownSize);
            positionedSelf.AddChild(positionedChild);
            return positionedSelf;
        }

        public override SPointF Arrange(SPointF parentPosition, SPointF parentSize, SSizeF thisSize)
        {
            // The container itself is positioned at the given parent position, since
            // containers always fill the available space.
            return parentPosition;
        }

        public void SetPadding(double padding, Sides side)
        {
            if (side.HasFlag(Sides.Left))
            {
                LeftPadding = padding;
            }

            if (side.HasFlag(Sides.Top))
            {
                TopPadding = padding;
            }

            if (side.HasFlag(Sides.Right))
            {
                RightPadding = padding;
            }

            if (side.HasFlag(Sides.Bottom))
            {
                BottomPadding = padding;
            }
        }
    }
}
