using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    public static class MathHelpers
    {
        public static double Ease(double start, double end, double progress, Easing easingFunction)
        {
            double easedProgress = easingFunction(Math.Clamp(progress, 0, 1));
            return start + (end - start) * easedProgress;
        }

        public static SPointF Ease(SPointF start, SPointF end, double progress, Easing easingFunction)
        {
            double easedProgress = easingFunction(Math.Clamp(progress, 0, 1));
            double x = start.X + (end.X - start.X) * easedProgress;
            double y = start.Y + (end.Y - start.Y) * easedProgress;
            return new SPointF(x, y);
        }

        public static SRectF Ease(SRectF start, SRectF end, double progress, Easing easingFunction)
        {
            double easedProgress = easingFunction(Math.Clamp(progress, 0, 1));
            double x = start.X + (end.X - start.X) * easedProgress;
            double y = start.Y + (end.Y - start.Y) * easedProgress;
            double width = start.Width + (end.Width - start.Width) * easedProgress;
            double height = start.Height + (end.Height - start.Height) * easedProgress;
            return new SRectF(x, y, width, height);
        }

        public static SSizeF Ease(SSizeF start, SSizeF end, double progress, Easing easingFunction)
        {
            double easedProgress = easingFunction(Math.Clamp(progress, 0, 1));
            double width = start.Width + (end.Width - start.Width) * easedProgress;
            double height = start.Height + (end.Height - start.Height) * easedProgress;
            return new SSizeF(width, height);
        }

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

        public static double[] SubdivideRange(double start, double end, int subdivisions)
        {
            if (subdivisions <= 0) { throw new ArgumentException("Subdivisions must be greater than zero.", nameof(subdivisions)); }
            var result = new double[subdivisions + 1];
            var step = (end - start) / subdivisions;
            for (int i = 0; i <= subdivisions; i++)
            {
                result[i] = start + (i * step);
            }
            return result;
        }

        public static double[] EquallySpacePoints(double start, double end, int points)
        {
            if (points <= 0) { throw new ArgumentException("Points must be greater than zero.", nameof(points)); }
            var result = new double[points];
            var step = (end - start) / (points - 1);
            for (int i = 0; i < points; i++)
            {
                result[i] = start + (i * step);
            }
            return result;
        }

        public static double[] EquallySpaceCenteredPoints(double start, double end, int points)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Points must be greater than zero.", nameof(points));
            }

            // Treat it as if we have two more points (one at the start and one at the end) than was provided,
            // so that we get the points centered within the range.
            var spacedPoints = EquallySpacePoints(start, end, points + 2);
            var result = new double[points];
            Array.Copy(spacedPoints, 1, result, 0, points);
            return result;
        }

        public static double DotProduct(SPointF a, SPointF b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static bool RectangleIntersectsXCoordinate(SRectF rectangle, double xCoordinate)
        {
            return rectangle.Left <= xCoordinate && rectangle.Right >= xCoordinate;
        }

        public static double RandomInRange(Random random, double min, double max)
        {
            var range = max - min;
            var sample = random.NextDouble() * range;
            return sample + min;
        }

        public static double PadSides(double width, double padding)
        {
            return width + (padding * 2);
        }

        public static double CenterOf(double a, double b)
        {
            return (a + b) / 2;
        }

        public static IEnumerable<double> SolveQuadratic(double a, double b, double c)
        {
            var discriminant = (b * b) - (4 * a * c);
            if (discriminant < 0)
            {
                throw new InvalidOperationException("No real roots exist for the given quadratic equation.");
            }
            var sqrtDiscriminant = Math.Sqrt(discriminant);
            var root1 = (-b + sqrtDiscriminant) / (2 * a);
            var root2 = (-b - sqrtDiscriminant) / (2 * a);
            yield return root1;
            yield return root2;
        }

        public static double EvaluateQuadraticBezier(SPointF p0, SPointF p1, SPointF p2, double t)
        {
            var mt = 1 - t;
            return (mt * mt * p0.X) + (2 * mt * t * p1.X) + (t * t * p2.X);
        }
    }
}
