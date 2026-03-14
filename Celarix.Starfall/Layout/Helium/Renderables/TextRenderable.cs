using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class TextRenderable : HeliumRenderable,
        IBoundedRenderable,
        IColoredRenderable
    {
        public required string Text { get; init; }
        public required SRectF Bounds { get; set; }
        public required SFont Font { get; init; }
        public required SColor Color { get; set; }
        public required SAngle Rotation { get; init; }
        public Alignment Alignment { get; init; } = Alignment.Center;
        public bool DrawDirectly { get; init; } = false;

        public override void Render(IRenderTarget target)
        {
            if (DrawDirectly)
            {
                target.DrawTextDirectly(Text, Font, Bounds, Color, Rotation);
            }
            else
            {
                target.DrawText(Text, Font, Bounds, Color, Rotation, Alignment);
            }
        }
    }
}
