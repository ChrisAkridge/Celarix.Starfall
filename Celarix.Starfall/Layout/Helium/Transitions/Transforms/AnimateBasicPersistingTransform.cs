using Celarix.Starfall.Layout.Helium.Renderables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public sealed class AnimateBasicPersistingTransform : ITransformForPersisting
    {
        private readonly BasicTransform transformFunc;

        public AnimateBasicPersistingTransform(BasicTransform transformFunc)
        {
            this.transformFunc = transformFunc;
        }

        public IRenderable Apply(IRenderable from, IRenderable to, double progress)
        {
            return transformFunc(from, to, progress);
        }
    }
}
