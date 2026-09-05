using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables;

public sealed record PositionedLibraRenderable(LibraRenderable Renderable, SPointF Position, double ScaleFactor)
{
    public void Render(IRenderTarget target)
    {
        Renderable.RenderAt(target, Position, ScaleFactor);
    }

    public static IEnumerable<PositionedLibraRenderable> FromLayout(LibraLayoutResult result,
        SPointF offset, double scaleFactor)
    {
        // First, scale the positions and sizes of each renderable inside the result toward the origin
        var scaledRenderables = result.Renderables.Select(r => r.Scale(scaleFactor)).ToList();
    }
}
