using System;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly record struct STransform2D(SPointF Translation, SSizeF Scale)
    {
        public static readonly STransform2D Identity = new(SPointF.Zero, new SSizeF(1d, 1d));

        public STransform2D Translated(double x, double y) =>
            this with { Translation = Translation.Move(x, y) };

        public STransform2D Translated(SPointF amount) =>
            Translated(amount.X, amount.Y);

        public STransform2D Scaled(double scale) =>
            Scaled(scale, scale);

        public STransform2D Scaled(double xScale, double yScale) =>
            this with { Scale = new SSizeF(Scale.Width * xScale, Scale.Height * yScale) };

        public STransform2D Scaled(SSizeF scale) =>
            Scaled(scale.Width, scale.Height);

        public SPointF Transform(SPointF point) =>
            new((point.X * Scale.Width) + Translation.X, (point.Y * Scale.Height) + Translation.Y);

        public SRectF Transform(SRectF rect)
        {
            var left = (rect.Left * Scale.Width) + Translation.X;
            var right = (rect.Right * Scale.Width) + Translation.X;
            var top = (rect.Top * Scale.Height) + Translation.Y;
            var bottom = (rect.Bottom * Scale.Height) + Translation.Y;

            return SRectF.FromSides(top, right, bottom, left);
        }

        public STransform2D Inverted()
        {
            if (Scale.Width == 0d || Scale.Height == 0d)
            {
                throw new InvalidOperationException("Cannot invert a transform with a zero scale component.");
            }

            var inverseScale = new SSizeF(1d / Scale.Width, 1d / Scale.Height);
            var inverseTranslation = new SPointF(-Translation.X * inverseScale.Width, -Translation.Y * inverseScale.Height);

            return new STransform2D(inverseTranslation, inverseScale);
        }
    }
}
