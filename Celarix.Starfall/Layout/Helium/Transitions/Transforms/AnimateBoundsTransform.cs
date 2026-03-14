using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Renderables.Interfaces;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public sealed class AnimateBoundsTransform : ITransformForPersisting
    {
        private Easing easing;

        public AnimateBoundsTransform(Easing easing)
        {
            this.easing = easing;
        }

        public IRenderable Apply(IRenderable from, IRenderable to, double progress)
        {
            if ((from is not IBoundedRenderable fromBounded) || (to is not IBoundedRenderable toBounded))
            {
                // Design decision: Transforms applied to renderables that don't support the transform are just ignored.
                // Maybe we can log it someday.
                return from;
            }

            fromBounded.Bounds = SRectF.Lerp(fromBounded.Bounds, toBounded.Bounds, easing(progress));
            return from;
        }
    }
}
