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

        public static bool Intersects(SRectF a, SRectF b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }

        public static SRectF Lerp(SRectF from, SRectF to, double progress)
        {
            var newX = from.X + (to.X - from.X) * progress;
            var newY = from.Y + (to.Y - from.Y) * progress;
            var newWidth = from.Width + (to.Width - from.Width) * progress;
            var newHeight = from.Height + (to.Height - from.Height) * progress;
            return new SRectF(newX, newY, newWidth, newHeight);
        }

        public static SRectF FromSides(double top, double right, double bottom, double left)
        {
            if (right < left)
            {
                (right, left) = (left, right);
            }
            if (bottom < top)
            {
                (bottom, top) = (top, bottom);
            }

            var width = right - left;
            var height = bottom - top;
            return new SRectF(left, top, width, height);
        }

        public SRectF Shrink(double horizontalAmount, double verticalAmount)
        {
            var newX = X + horizontalAmount;
            var newY = Y + verticalAmount;
            var newWidth = Math.Max(0, Width - 2 * horizontalAmount);
            var newHeight = Math.Max(0, Height - 2 * verticalAmount);
            return new SRectF(newX, newY, newWidth, newHeight);
        }

        public SRectF RoundStandard()
        {
            // Position rounds to the nearest integer. If the fractional part is < 0.5, round down;
            // if it's >= 0.5, round up. Size always rounds up, so that the rounded rect fully contains
            // the original rect.
            var newX = Math.Round(X, MidpointRounding.AwayFromZero);
            var newY = Math.Round(Y, MidpointRounding.AwayFromZero);
            var newWidth = Math.Ceiling(X + Width) - Math.Floor(X);
            var newHeight = Math.Ceiling(Y + Height) - Math.Floor(Y);
            return new SRectF(newX, newY, newWidth, newHeight);
        }

        public bool FitsWithin(SRectF outer)
        {
            return Left >= outer.Left && Right <= outer.Right && Top >= outer.Top && Bottom <= outer.Bottom;
        }
    }
}
