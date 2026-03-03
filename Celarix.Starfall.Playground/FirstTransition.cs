using Celarix.Starfall.Layout;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground
{
    internal sealed class FirstTransition : IHeliumTransition
    {
        private readonly HeliumRenderable[] renderables;
        private readonly SPointF blueRectStartPosition;
        private readonly SPointF blueRectEndPosition;
        private readonly SColor backgroundColor;

        public double Duration { get; }

        public FirstTransition(HeliumScene from, HeliumScene to, double duration, SSizeF maxSize)
        {
            // This is a very very bad first transition, but it is helping me map out how the
            // interface should become, so hopefully future iterations will be less dumb.
            Duration = duration;
            renderables = [.. from.GetRenderables(maxSize).Cast<HeliumRenderable>()];
            HeliumRenderable[] toRenderables = [.. to.GetRenderables(maxSize).Cast<HeliumRenderable>()];
            var query = SelectionQuery<HeliumRenderable>.StartById("blue-rect");
            var fromBlueRect = query.Query(renderables).Single();
            var toBlueRect = query.Query(toRenderables).Single();
            blueRectStartPosition = (fromBlueRect as RectangleRenderable)!.Bounds.Position;
            blueRectEndPosition = (toBlueRect as RectangleRenderable)!.Bounds.Position;
            backgroundColor = from.BackgroundColor;
        }

        public void Render(double progress, IRenderTarget renderTarget)
        {
            renderTarget.Clear(backgroundColor);
            
            var currentX = MathHelpers.SmoothStep(blueRectStartPosition.X, blueRectEndPosition.X, progress);
            var currentY = MathHelpers.SmoothStep(blueRectStartPosition.Y, blueRectEndPosition.Y, progress);
            var currentPosition = new SPointF(currentX, currentY);
            var query = SelectionQuery<HeliumRenderable>.StartById("blue-rect");
            var blueRect = (query.Query(renderables).Single() as RectangleRenderable);
            blueRect!.Bounds = currentPosition.WithSize(blueRect.Bounds.Size);

            foreach (var renderable in renderables)
            {
                renderable.Render(renderTarget);
            }
        }
    }
}
