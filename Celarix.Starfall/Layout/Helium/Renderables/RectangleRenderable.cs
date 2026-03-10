using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class RectangleRenderable : HeliumRenderable
    {
        public required SRectF Bounds { get; set; }
        public required SColor Color { get; set; }
        public required SPaintStyle PaintStyle { get; set; }

        public override void Render(IRenderTarget target)
        {
            target.DrawRectangle(Bounds, Color, PaintStyle, SAngle.Zero);
        }
    }
}
