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
    }
}
