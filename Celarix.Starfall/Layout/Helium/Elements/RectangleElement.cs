using Celarix.Starfall.Layout.Helium.Renderables;
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

        public RectangleElement(double desiredWidthFraction, double desiredHeightFraction, SColor color, string? id = null)
        {
            this.desiredWidthFraction = desiredWidthFraction;
            this.desiredHeightFraction = desiredHeightFraction;
            Color = color;
            Id = id;
        }

        public override void MeasureSelf(SSizeF maxSize)
        {
            var width = maxSize.Width * (double)DesiredWidthFraction;
            var height = maxSize.Height * (double)DesiredHeightFraction;
            ActualSize = new SSizeF(width, height);
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
