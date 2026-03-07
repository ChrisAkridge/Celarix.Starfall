using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public sealed class RectangleElement : ResizableHeliumElement
    {
        public override IReadOnlyList<HeliumElement> Children => Array.Empty<HeliumElement>();
        public SColor Color { get; set; }

        public RectangleElement(double desiredWidthFraction, double desiredHeightFraction, SColor color, string? id = null)
        {
            this.desiredWidthFraction = desiredWidthFraction;
            this.desiredHeightFraction = desiredHeightFraction;
            Color = color;
            Id = id;
        }

        public override void ArrangeChildren(SRectF parentBounds)
        {
            // No children, so nothing to do.
        }

        public override HeliumElement Clone()
        {
            return new RectangleElement(desiredWidthFraction, desiredHeightFraction, Color)
            {
                Id = Id,
            };
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            return [new RectangleRenderable
            {
                Bounds = ActualBounds!.Value,
                Color = Color,
                Id = Id
            }];
        }
    }
}
