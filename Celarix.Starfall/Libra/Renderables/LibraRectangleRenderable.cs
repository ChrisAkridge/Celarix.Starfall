using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables
{
    public sealed class LibraRectangleRenderable : LibraRenderable
    {
        public LibraRectangleRenderable(LibraRenderableKey key, SSizeF size, SColor fillColor)
            : base(key, fillColor, SColor.Transparent)
        {
            Size = size;
        }

        public override LibraRenderable Clone()
        {
            return new LibraRectangleRenderable(Key, Size, ForegroundColor)
            {
                Position = Position,
            };
        }

        public override void RenderAt(IRenderTarget target, SPointF position, double scaleFactor)
        {
            var newBounds = new SRectF(position, Size * scaleFactor);
            target.DrawRectangle(newBounds, ForegroundColor.WithOpacity(Opacity), SPaintStyle.Fill, SAngle.Zero);
        }
    }
}
