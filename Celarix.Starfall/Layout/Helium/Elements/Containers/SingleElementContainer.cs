using Celarix.Starfall.Layout.Helium.Components;
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
        public Padding Padding { get; set; }
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
            Child.MeasureSelf(Padding.GetInnerSize(ActualSize.Value));
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            if (Child == null) { return; }

            var innerBounds = Padding.GetInnerRectForOuterRect(thisBounds);
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
                Padding = Padding,
                Alignment = Alignment,
                Child = Child?.Clone()
            };
        }
    }
}
