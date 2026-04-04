using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    public static class MathHelpers
    {
        public static double SmoothStep(double start, double end, double progress)
        {
            // This is the standard smooth step function, which is a cubic Hermite interpolation between 0 and 1.
            // It has the property that it starts at 0, ends at 1, and has a smooth transition between the two.
            // The formula is: f(x) = 3x^2 - 2x^3, where x is the progress between 0 and 1.
            // To apply this to a range between start and end, we can use the formula: result = start + (end - start) * f(progress).
            double t = Math.Clamp(progress, 0, 1);
            double smoothProgress = t * t * (3 - 2 * t);
            return start + (end - start) * smoothProgress;
        }

        /// <summary>
        /// Gets the largest square that can fit within the given rectangular size. The resulting square
        /// will have its width and height equal to the smaller of the rectangle's width and height.
        /// </summary>
        /// <param name="rectangle">The rectangle containing the square.</param>
        /// <returns>An <see cref="SSizeF"/> instance whose width equals its height.</returns>
        public static SSizeF LargestSquareFittingSize(SSizeF rectangle)
        {
            double size = Math.Min(rectangle.Width, rectangle.Height);
            return new SSizeF(size, size);
        }

        public static SColor InterpolateColor(SColor from, SColor to, double progress)
        {
            // TODO: make gamma aware
            byte r = (byte)(from.R + (to.R - from.R) * progress);
            byte g = (byte)(from.G + (to.G - from.G) * progress);
            byte b = (byte)(from.B + (to.B - from.B) * progress);
            byte a = (byte)(from.A + (to.A - from.A) * progress);
            return new SColor(r, g, b, a);
        }
    }
}
