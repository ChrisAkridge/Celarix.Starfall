using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SRectF
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;

        public SPointF Position => new SPointF(X, Y);
        public SSizeF Size => new SSizeF(Width, Height);

        public SRectF(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public SRectF(SPointF position, SSizeF size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public override readonly string ToString() => $"(X: {X}d, Y: {Y}d, Width: {Width}d, Height: {Height}d)";

        public static SRectF operator +(SRectF rect, SPointF point)
        {
            return new SRectF(rect.X + point.X, rect.Y + point.Y, rect.Width, rect.Height);
        }
    }
}
