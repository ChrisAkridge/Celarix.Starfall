using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public abstract class LibraExpression
    {
        public string? Id { get; set; }
        public SColor ForegroundColor { get; set; } = SColor.White;
        public SColor BackgroundColor { get; set; } = SColor.Transparent;

        protected internal abstract LibraLayoutResult Layout(LibraRenderingContext context);

        protected internal static void MoveRenderables(IReadOnlyList<LibraRenderable> renderables, SPointF offset)
        {
            foreach (var renderable in renderables)
            {
                renderable.Position += offset;
            }
        }

        protected internal static IReadOnlyList<LibraRenderable> MoveRenderablesCopy(IReadOnlyList<LibraRenderable> renderables, SPointF offset)
        {
            var copy = new List<LibraRenderable>(renderables.Count);
            foreach (var renderable in renderables)
            {
                var renderableCopy = renderable.Clone();
                renderableCopy.Position += offset;
                copy.Add(renderableCopy);
            }
            return copy;
        }

        protected internal static double DefaultMathAxisY(LibraRenderingContext context)
        {
            return context.BaselineY - context.Em * 0.25;
        }
    }
}
