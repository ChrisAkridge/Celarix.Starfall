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

        public RectangleRenderable(SRectF bounds, SColor color, SPaintStyle paintStyle)
        {
            Bounds = bounds;
            Color = color;
            PaintStyle = paintStyle;
        }

        public override void Render(IRenderTarget target)
        {
            target.DrawRectangle(Bounds, Color, PaintStyle, SAngle.Zero);
        }
    }
}
