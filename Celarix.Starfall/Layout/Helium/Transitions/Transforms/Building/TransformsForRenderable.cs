using Celarix.Starfall.Layout.Helium.Renderables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class TransformsForRenderable
    {
        private readonly List<ITransformForPersisting> transforms = [];
        public HeliumRenderable Renderable { get; init; }
        public IReadOnlyList<ITransformForPersisting> Transforms => transforms;

        public TransformsForRenderable(HeliumRenderable renderable)
        {
            Renderable = renderable;
        }

        public void AddTransform(ITransformForPersisting transform)
        {
            transforms.Add(transform);
        }
    }
}
