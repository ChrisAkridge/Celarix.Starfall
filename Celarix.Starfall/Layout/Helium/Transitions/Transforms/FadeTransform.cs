using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public sealed class FadeTransform : ITransformForTransients
    {
        private readonly FadeDirection direction;
        private readonly Easing easing;

        public FadeTransform(FadeDirection direction, Easing easing)
        {
            this.direction = direction;
            this.easing = easing;
        }

        public void Apply(IRenderable renderable, double progress)
        {
            if (renderable is not IColoredRenderable coloredRenderable)
            {
                // Design decision: Transforms applied to renderables that don't support the transform are just ignored.
                // Maybe we can log it someday.
                return;
            }

            var color = coloredRenderable.Color;
            var originalAlpha = color.A;
            var alphaFactor = direction == FadeDirection.In ? easing(progress) : 1 - easing(progress);
            var newAlpha = (byte)(originalAlpha * alphaFactor);
            coloredRenderable.Color = new SColor(color.R, color.G, color.B, newAlpha);
        }
    }
}
