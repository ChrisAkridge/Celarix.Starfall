using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class TextRenderable : HeliumRenderable
    {
        public required string Text { get; init; }
        public required SRectF Bounds { get; init; }
        public required SFont Font { get; init; }
        public required SColor Color { get; init; }
        public required SAngle Rotation { get; init; }

        public override void Render(IRenderTarget target)
        {
            target.DrawText(Text, Font, Bounds, Color, Rotation);
        }
    }
}
