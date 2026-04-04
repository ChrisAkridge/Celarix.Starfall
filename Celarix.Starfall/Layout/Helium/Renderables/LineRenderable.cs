using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class LineRenderable : HeliumRenderable,
        IBoundedRenderable,
        IColoredRenderable
    {
        public SPointF Start { get; set; }
        public SPointF End { get; set; }
        public SColor Color { get; set; }
        public float Thickness { get; set; }

        // A line doesn't really have bounds in the same way a rectangle does, but what we can do is
        // use the bounding box of the line. By executive decision, the line is always from the top-left
        // of the bounding box to the bottom-right.
        public SRectF Bounds
        {
            get
            {
                var left = Math.Min(Start.X, End.X);
                var top = Math.Min(Start.Y, End.Y);
                var right = Math.Max(Start.X, End.X);
                var bottom = Math.Max(Start.Y, End.Y);
                return SRectF.FromSides(top, right, bottom, left);
            }
            set
            {
                Start = value.Position;
                End = value.Position + value.Size;
            }
        }

        public LineRenderable(SPointF start, SPointF end, SColor color, float thickness)
        {
            Start = start;
            End = end;
            Color = color;
            Thickness = thickness;
        }

        public override void Render(IRenderTarget target)
        {
            target.DrawLine(Start, End, Color, Thickness);
        }
    }
}
