using Celarix.Starfall.Layout.Helium.Renderables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public delegate IRenderable BasicTransform(IRenderable from, IRenderable to, double progress);

    public static class BasicTransforms
    {
        public static BasicTransform StepStart = (from, to, progress) => progress > 0 ? to : from;
        public static BasicTransform StepEnd = (from, to, progress) => progress < 1 ? from : to;
    }
}
