using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class QueriedTransformForTransients
    {
        public SelectionQuery<HeliumRenderable>? Query { get; internal set; }
        public ITransformForTransients? Transform { get; internal set; }
    }
}
