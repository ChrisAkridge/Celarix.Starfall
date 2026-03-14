using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class QueriedTransformForPersisting
    {
        public SelectionQuery<HeliumRenderable>? Query { get; internal set; }
        public ITransformForPersisting? Transform { get; internal set; }
    }
}
