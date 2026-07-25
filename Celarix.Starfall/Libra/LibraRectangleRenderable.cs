using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public sealed class LibraRectangleRenderable : LibraRenderable
    {
        public LibraRectangleRenderable(SSizeF size, SColor fillColor, string? id = null)
            : base(fillColor, SColor.Transparent, id)
        {
            Size = size;
        }

        public override LibraRenderable Clone()
        {
            return new LibraRectangleRenderable(Size, ForegroundColor, Id)
            {
                Position = Position,
            };
        }

        public override void RenderAt(IRenderTarget target, SPointF position, double scaleFactor)
        {
            var newBounds = new SRectF(position, Size * scaleFactor);
            target.DrawRectangle(newBounds, ForegroundColor, SPaintStyle.Fill, SAngle.Zero);
        }
    }
}
