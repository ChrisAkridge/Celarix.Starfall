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

        /// <summary>
        /// Gets the inner width available after accounting for the left and right padding.
        /// The returned value will never be negative; if the total horizontal padding exceeds
        /// <paramref name="maxWidth"/>, this method will return 0. WARNING: The units of <see cref="Left"/>
        /// and <see cref="Right"/> are not in pixels, but in multiples of <paramref name="maxWidth"/>. For example,
        /// if <see cref="Left"/> is 0.1, it means the inner rectangle's left edge will be 10% of <paramref name="maxWidth"/>
        /// away from the left edge of <paramref name="outerRect"/>.
        /// </summary>
        /// <param name="maxWidth">The outer width to fit the padded inner width into.</param>
        /// <returns>A pixel width representing the available width after padding.</returns>
        public double GetInnerWidth(double maxWidth)
        {
            var actualLeft = Left * maxWidth;
            var actualRight = Right * maxWidth;
            double totalPadding = actualLeft + actualRight;
            return Math.Max(0, maxWidth - totalPadding);
        }

        /// <summary>
        /// Gets the inner height available after accounting for the top and bottom padding.
        /// The returned value will never be negative; if the total vertical padding exceeds
        /// <paramref name="maxHeight"/>, this method will return 0. WARNING: The units of <see cref="Top"/>
        /// and <see cref="Bottom"/> are not in pixels, but in multiples of <paramref name="maxHeight"/>. For example,
        /// if <see cref="Top"/> is 0.1, it means the inner rectangle's top edge will be 10% of <paramref name="maxHeight"/>
        /// away from the top edge of <paramref name="outerRect"/>.
        /// </summary>
        /// <param name="maxHeight">The outer height to fit the padded inner height into.</param>
        /// <returns>A pixel width representing the available width after padding.</returns>
        public double GetInnerHeight(double maxHeight)
        {
            var actualTop = Top * maxHeight;
            var actualBottom = Bottom * maxHeight;
            double totalPadding = actualTop + actualBottom;
            return Math.Max(0, maxHeight - totalPadding);
        }

        public SSizeF GetInnerSize(SSizeF maxSize)
        {
            return new SSizeF(GetInnerWidth(maxSize.Width), GetInnerHeight(maxSize.Height));
        }

        /// <summary>
        /// Gets the inner rectangle that fits within the given outer rectangle, accounting for the padding.
        /// WARNING: The units of <see cref="Top"/>, <see cref="Left"/>, <see cref="Right"/>, and <see cref="Bottom"/> are not in pixels,
        /// but in multiples of <paramref name="outerRect"/>'s width (for Left and Right) and height (for Top and Bottom). For example,
        /// if <see cref="Left"/> is 0.1, it means the inner rectangle's left edge will be 10% of <paramref name="outerRect"/>'s width
        /// away from the left edge of <paramref name="outerRect"/>.
        /// </summary>
        /// <param name="outerRect"></param>
        /// <returns></returns>
        public SRectF GetInnerRectForOuterRect(SRectF outerRect)
        {
            var actualTop = Top * outerRect.Height;
            var actualLeft = Left * outerRect.Width;

            double innerX = Math.Min(outerRect.Right, outerRect.X + actualLeft);
            double innerY = Math.Min(outerRect.Bottom, outerRect.Y + actualTop);
            double innerWidth = GetInnerWidth(outerRect.Width);
            double innerHeight = GetInnerHeight(outerRect.Height);
            return new SRectF(innerX, innerY, innerWidth, innerHeight);
        }
    }
}
