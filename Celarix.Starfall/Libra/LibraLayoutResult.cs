using Celarix.Starfall.Rendering.Models;
using System.Collections.Immutable;

namespace Celarix.Starfall.Libra
{
    public sealed record LibraLayoutResult(
        ImmutableArray<LibraRenderable> Renderables,
        SRectF Bounds,
        double BaselineY,
        double MathAxisY)
    {
        public LibraLayoutResult(
            IEnumerable<LibraRenderable> renderables,
            SRectF bounds,
            double baselineY,
            double mathAxisY)
            : this([.. renderables], bounds, baselineY, mathAxisY)
        {
        }
    }
}
