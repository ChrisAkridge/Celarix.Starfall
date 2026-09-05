using Celarix.Starfall.Extensions;
using Celarix.Starfall.Libra.Renderables;
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
        public static LibraLayoutResult Empty => new([], SRectF.Empty, 0d, 0d);

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

        public LibraLayoutResult Scale(double scaleFactor)
        {
            return new LibraLayoutResult(
                Renderables.Select(r => r.Scale(scaleFactor)),
                new SRectF(Bounds.X, Bounds.Y, Bounds.Width * scaleFactor, Bounds.Height * scaleFactor),
                BaselineY * scaleFactor,
                MathAxisY * scaleFactor);
        }

        public LibraLayoutResult ScaleToFitWidth(double width)
        {
            width.ThrowIfNotPositive(nameof(width));

            if (Bounds.Width < width)
            {
                return this;
            }

            var scaleFactor = width / Bounds.Width;
            return Scale(scaleFactor);
        }

        // I don't actually know if I designed Libra to support this, but here goes...
        public LibraLayoutResult Translate(SPointF offset)
        {
            return new LibraLayoutResult(
                Renderables.Select(r => r.Translate(offset)),
                Bounds + offset,
                BaselineY + offset.Y,
                MathAxisY + offset.Y);
        }
    }
}
