using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public static class AlignmentHelper
    {
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

        public static double LeftAlign(double outerLeft, double outerRight, double innerWidth)
        {
            return outerLeft;
        }

        public static double RightAlign(double outerLeft, double outerRight, double innerWidth)
        {
            return outerRight - innerWidth;
        }

        public static double CenterAlign(double outerLeft, double outerRight, double innerWidth)
        {
            var totalSpace = outerRight - outerLeft;
            return outerLeft + (totalSpace - innerWidth) / 2;
        }
    }
}
