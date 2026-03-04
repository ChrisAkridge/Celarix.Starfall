using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Components
{
    public readonly struct Padding
    {
        public double Left { get; }
        public double Top { get; }
        public double Right { get; }
        public double Bottom { get; }

        public Padding(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Padding WithPadding(double padding, Sides sides)
        {
            double left = (sides & Sides.Left) != 0 ? padding : Left;
            double top = (sides & Sides.Top) != 0 ? padding : Top;
            double right = (sides & Sides.Right) != 0 ? padding : Right;
            double bottom = (sides & Sides.Bottom) != 0 ? padding : Bottom;

            return new Padding(left, top, right, bottom);
        }

        public double GetInnerWidth(double maxWidth)
        {
            double totalPadding = Left + Right;
            return Math.Max(0, maxWidth - totalPadding);
        }

        public double GetInnerHeight(double maxHeight)
        {
            double totalPadding = Top + Bottom;
            return Math.Max(0, maxHeight - totalPadding);
        }

        public SSizeF GetInnerSize(SSizeF maxSize)
        {
            return new SSizeF(GetInnerWidth(maxSize.Width), GetInnerHeight(maxSize.Height));
        }

        public SRectF GetInnerRectForOuterRect(SRectF outerRect)
        {
            double innerX = outerRect.X + Left;
            double innerY = outerRect.Y + Top;
            double innerWidth = GetInnerWidth(outerRect.Width);
            double innerHeight = GetInnerHeight(outerRect.Height);
            return new SRectF(innerX, innerY, innerWidth, innerHeight);
        }
    }
}
