using Celarix.Starfall.Layout.Helium.Renderables;
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

        public override void MeasureSelf(SSizeF availableSize)
        {
            // The container itself always fills the available space.
            ActualSize = availableSize;

            // If there's no child, we're done.
            if (Child == null)
            {
                return;
            }

            // Calculate the available size for the child by subtracting padding.
            var availableWidth = Math.Max(0, ActualSize.Value.Width - LeftPadding - RightPadding);
            var availableHeight = Math.Max(0, ActualSize.Value.Height - TopPadding - BottomPadding);
            Child.MeasureSelf(new SSizeF(availableWidth, availableHeight));
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            if (Child == null) { return; }

            var containerSize = ActualSize!.Value;
            var innerWidth = Math.Max(0, containerSize.Width - LeftPadding - RightPadding);
            var innerHeight = Math.Max(0, containerSize.Height - TopPadding - BottomPadding);
            var innerBounds = new SRectF(thisBounds.X + LeftPadding,
                thisBounds.Y + TopPadding, innerWidth, innerHeight);
            var childSize = Child.ActualSize!.Value;
            Child.ActualPosition = AlignmentHelper.Align(Alignment, innerBounds, childSize);
            Child.ArrangeChildren(Child.ActualBounds!.Value);
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            return Child != null ? Child.GetRenderables() : Array.Empty<IRenderable>();
        }

        public override HeliumElement Clone()
        {
            return new SingleElementContainer
            {
                Id = Id,
                LeftPadding = LeftPadding,
                TopPadding = TopPadding,
                RightPadding = RightPadding,
                BottomPadding = BottomPadding,
                Alignment = Alignment,
                Child = Child?.Clone()
            };
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
