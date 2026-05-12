using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SSizeF
    {
        public static readonly SSizeF Zero = new(0, 0);

        public double Width { get; }
        public double Height { get; }
        public SSizeF(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public override readonly string ToString() => $"(Width: {Width}d, Height: {Height}d)";
        public SRectF At(SPointF point) => new(point, this);
        public SRectF CenterAt(SPointF center) => new(center - new SPointF(Width / 2, Height / 2), this);

        public SSizeF ExpandFromCenter(double widthAmount, double heightAmount)
        {
            return new SSizeF(Width + (widthAmount * 2),
                Height + (heightAmount * 2));
        }

        public SSizeF ShrinkTowardCenter(double widthAmount, double heightAmount)
        {
            return new SSizeF(Width - (widthAmount * 2),
                Height - (heightAmount * 2));
        }

        public SSizeF FitAspectRatioInside(double desiredAspectRatio)
        {
            var currentAspectRatio = Width / Height;
            if (currentAspectRatio > desiredAspectRatio)
            {
                // Too wide, reduce width
                var newWidth = Height * desiredAspectRatio;
                return new SSizeF(newWidth, Height);
            }
            else
            {
                // Too tall, reduce height
                var newHeight = Width / desiredAspectRatio;
                return new SSizeF(Width, newHeight);
            }
        }

        public static bool operator ==(SSizeF left, SSizeF right) => left.Equals(right);
        public static bool operator !=(SSizeF left, SSizeF right) => !(left == right);
    }
}
