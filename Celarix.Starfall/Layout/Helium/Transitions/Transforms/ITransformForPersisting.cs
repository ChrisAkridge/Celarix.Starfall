using Celarix.Starfall.Layout.Helium.Renderables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public interface ITransformForPersisting
    {
        IRenderable Apply(IRenderable from, IRenderable to, double progress);
    }
}
