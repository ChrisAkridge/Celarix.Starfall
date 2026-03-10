using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public static class AlignmentHelper
    {
        /// <summary>
        /// Calculates the left and top coordinates for an inner element based on the specified alignment within an outer bounding rectangle.
        /// </summary>
        /// <param name="alignment">The alignment of the inner element within the outer bounds.</param>
        /// <param name="outerBounds">The outer bounds within which the inner element is to be aligned.</param>
        /// <param name="innerSize">The size of the inner element to be aligned.</param>
        /// <returns>The calculated position for the inner element.</returns>
        public static SPointF Align(Alignment alignment, SRectF outerBounds, SSizeF innerSize)
        {
            // Horizontal alignment
            double innerX = double.NaN;
            if (alignment is Alignment.TopLeft or Alignment.LeftCenter or Alignment.BottomLeft)
            {
                innerX = LeftAlign(outerBounds.X, outerBounds.X + outerBounds.Width, innerSize.Width);
            }
            else if (alignment is Alignment.TopRight or Alignment.RightCenter or Alignment.BottomRight)
            {
                innerX = RightAlign(outerBounds.X, outerBounds.X + outerBounds.Width, innerSize.Width);
            }
            else if (alignment is Alignment.TopCenter or Alignment.Center or Alignment.BottomCenter)
            {
                innerX = CenterAlign(outerBounds.X, outerBounds.X + outerBounds.Width, innerSize.Width);
            }

            // Vertical alignment
            double innerY = double.NaN;
            if (alignment is Alignment.TopLeft or Alignment.TopCenter or Alignment.TopRight)
            {
                innerY = LeftAlign(outerBounds.Y, outerBounds.Y + outerBounds.Height, innerSize.Height);
            }
            else if (alignment is Alignment.BottomLeft or Alignment.BottomCenter or Alignment.BottomRight)
            {
                innerY = RightAlign(outerBounds.Y, outerBounds.Y + outerBounds.Height, innerSize.Height);
            }
            else if (alignment is Alignment.LeftCenter or Alignment.Center or Alignment.RightCenter)
            {
                innerY = CenterAlign(outerBounds.Y, outerBounds.Y + outerBounds.Height, innerSize.Height);
            }

            return new SPointF(innerX, innerY);
        }

        /// <summary>
        /// Computes the left-aligned position for an inner element within an outer container. The
        /// inner element will be flush with the left edge of the outer container.
        /// </summary>
        /// <param name="outerLeft">The left edge of the outer container.</param>
        /// <param name="outerRight">The right edge of the outer container.</param>
        /// <param name="innerWidth">The width of the inner element.</param>
        /// <returns>The left-aligned position for the inner element.</returns>
        public static double LeftAlign(double outerLeft, double outerRight, double innerWidth)
        {
            return outerLeft;
        }

        /// <summary>
        /// Calculates the left coordinate required to right-align an inner element within an outer boundary.
        /// </summary>
        /// <remarks>Use this method when positioning an element so that its right edge matches the right
        /// edge of a container. The returned value does not account for cases where the inner element may not fit
        /// within the outer boundary.</remarks>
        /// <param name="outerLeft">The left coordinate of the outer boundary.</param>
        /// <param name="outerRight">The right coordinate of the outer boundary.</param>
        /// <param name="innerWidth">The width of the inner element to be aligned. Must be non-negative.</param>
        /// <returns>The left coordinate at which the inner element should be positioned to align its right edge with the outer
        /// boundary.</returns>
        public static double RightAlign(double outerLeft, double outerRight, double innerWidth)
        {
            return outerRight - innerWidth;
        }

        /// <summary>
        /// Calculates the right coordinate required to center-align an inner element within an outer boundary.
        /// </summary>
        /// <remarks>Use this method when positioning an element so that its center matches the center
        /// of a container. The returned value does not account for cases where the inner element may not fit
        /// within the outer boundary.</remarks>
        /// <param name="outerLeft">The left coordinate of the outer boundary.</param>
        /// <param name="outerRight">The right coordinate of the outer boundary.</param>
        /// <param name="innerWidth">The width of the inner element to be aligned. Must be non-negative.</param>
        /// <returns>The left coordinate at which the inner element should be positioned to align its center with the outer
        /// boundary.</returns>
        public static double CenterAlign(double outerLeft, double outerRight, double innerWidth)
        {
            var totalSpace = outerRight - outerLeft;
            return outerLeft + (totalSpace - innerWidth) / 2;
        }
    }
}
