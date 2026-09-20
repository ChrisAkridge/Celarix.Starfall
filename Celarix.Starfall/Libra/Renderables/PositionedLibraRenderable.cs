using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables;

public sealed record PositionedLibraRenderable(LibraRenderable Renderable, SPointF Position, double ScaleFactor)
{
    public SRectF Bounds => (Renderable.Bounds * ScaleFactor).At(Position);

    public void Render(IRenderTarget target)
    {
        Renderable.RenderAt(target, Position, ScaleFactor);
    }

    public static IEnumerable<PositionedLibraRenderable> FromLayout(LibraLayoutResult result,
        SPointF offset, double scaleFactor)
    {
        foreach (var renderable in result.Renderables)
        {
            var internalScaledPosition = renderable.Position * scaleFactor;
            yield return new PositionedLibraRenderable(renderable, internalScaledPosition + offset, scaleFactor);
        }
    }
}
