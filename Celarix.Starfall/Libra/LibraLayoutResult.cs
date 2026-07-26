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

        public IEnumerable<LibraRenderable> RenderablesMovedBy(SPointF offset)
        {
            foreach (var renderable in Renderables)
            {
                var renderableCopy = renderable.Clone();
                renderableCopy.Position += offset;
                yield return renderableCopy;
            }
        }

        public LibraLayoutResult Normalize()
        {
            var computedBounds = SRectF.BoundsOf(Renderables.Select(r => r.Bounds));
            var offset = -computedBounds.Position;

            return new LibraLayoutResult(
                [.. RenderablesMovedBy(offset)],
                computedBounds.At(SPointF.Zero),
                BaselineY + offset.Y,
                MathAxisY + offset.Y);
        }
    }
}
