using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class RectangleRenderable : HeliumRenderable,
        IBoundedRenderable,
        IColoredRenderable
    {
        public SRectF Bounds { get; set; }
        public SColor Color { get; set; }
        public SPaintStyle PaintStyle { get; set; }
        public double? FrameThickness { get; set; }

        public RectangleRenderable(SRectF bounds, SColor color, SPaintStyle paintStyle)
        {
            Bounds = bounds;
            Color = color;
            PaintStyle = paintStyle;
        }

        public static RectangleRenderable CreateFrame(SRectF bounds, SColor color, double thickness)
        {
            return new RectangleRenderable(bounds, color, SPaintStyle.Stroke) { FrameThickness = thickness };
        }

        public override void Render(IRenderTarget target)
        {
            if (!FrameThickness.HasValue)
            {
                target.DrawRectangle(Bounds, Color, PaintStyle, SAngle.Zero);
            }
            else
            {
                // Draw a frame by drawing four lines. Each line has the full thickness, but the start
                // and end coordinates refer to the center of the line, so we'll need to inset them by
                // half the thickness.
                var lineInset = FrameThickness.Value / 2;
                var top = Bounds.Y + lineInset;
                var left = Bounds.X + lineInset;
                var right = Bounds.Right - lineInset;
                var bottom = Bounds.Bottom - lineInset;
                // Draw the full length for the top and bottom lines to cover the corners, but inset the left and right lines to avoid overdrawing the corners.
                target.DrawLine(new SPointF(Bounds.Left, top), new SPointF(Bounds.Right, top), Color, (float)FrameThickness.Value);
                target.DrawLine(new SPointF(right, top), new SPointF(right, bottom), Color, (float)FrameThickness.Value);
                target.DrawLine(new SPointF(Bounds.Right, bottom), new SPointF(Bounds.Left, bottom), Color, (float)FrameThickness.Value);
                target.DrawLine(new SPointF(left, bottom), new SPointF(left, top), Color, (float)FrameThickness.Value);
            }
        }
    }
}
