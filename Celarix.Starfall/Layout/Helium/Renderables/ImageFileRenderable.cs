using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public sealed class ImageFileRenderable : HeliumRenderable,
        IBoundedRenderable
    {
        public string FilePath { get; }
        public SAngle Rotation { get; }
        public double Opacity { get; }
        public SRectF Bounds { get; set; }

        public ImageFileRenderable(string filePath, SRectF bounds, double opacity, SAngle rotation = default)
        {
            FilePath = filePath;
            Bounds = bounds;
            Rotation = rotation;
            Opacity = opacity;
        }

        public override void Render(IRenderTarget target)
        {
            target.DrawImageFromFile(FilePath, Bounds, Opacity, Rotation);
        }
    }
}
