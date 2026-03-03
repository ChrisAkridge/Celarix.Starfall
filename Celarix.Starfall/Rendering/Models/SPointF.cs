using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SPointF
    {
        public static readonly SPointF Zero = new(0, 0);

        public double X { get; }
        public double Y { get; }
        public SPointF(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override readonly string ToString() => $"({X}d, {Y}d)";

        public SRectF WithSize(SSizeF size)
        {
            return new SRectF(X, Y, size.Width, size.Height);
        }

        public static SPointF operator +(SPointF a, SPointF b)
        {
            return new SPointF(a.X + b.X, a.Y + b.Y);
        }

        public static SPointF operator -(SPointF a, SPointF b)
        {
            return new SPointF(a.X - b.X, a.Y - b.Y);
        }

        public static SPointF operator *(SPointF point, double scalar)
        {
            return new SPointF(point.X * scalar, point.Y * scalar);
        }
    }
}
