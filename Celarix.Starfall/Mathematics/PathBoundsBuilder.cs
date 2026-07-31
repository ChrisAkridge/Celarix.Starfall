using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    internal sealed class PathBoundsBuilder
    {
        private double _minX = double.PositiveInfinity;
        private double _minY = double.PositiveInfinity;
        private double _maxX = double.PositiveInfinity;
        private double _maxY = double.PositiveInfinity;

        public SRectF Bounds => new(_minX, _minY, (_maxX - _minX), (_maxY - _minY));

        public void Include(SPointF point)
        {
            _minX = Math.Min(_minX, point.X);
            _minY = Math.Min(_minY, point.Y);
            _maxX = Math.Max(_maxX, point.X);
            _maxY = Math.Max(_maxY, point.Y);
        }

        public void IncludeQuadratic(SPointF p0,
            SPointF p1,
            SPointF p2)
        {
            Include(p0);
            Include(p2);
            //foreach (var t in FindQuadraticExtrema(p0.X, p1.X, p2.X))
            //{
            //    Include(new SPointF(MathHelpers.EvaluateQuadratic(p0.X, p1.X, p2.X, t), MathHelpers.EvaluateQuadratic(p0.Y, p1.Y, p2.Y, t)));
            //}
        }

        public void IncludeCubic(SPointF p0,
            SPointF p1,
            SPointF p2,
            SPointF p3)
        {
            Include(p0);
            Include(p3);
            foreach (var t in FindCubicExtrema(p0.X, p1.X, p2.X, p3.X))
            {
                Include(new SPointF(EvaluateCubic(p0.X, p1.X, p2.X, p3.X, t), EvaluateCubic(p0.Y, p1.Y, p2.Y, p3.Y, t)));
            }
        }

        private static IEnumerable<double> FindCubicExtrema(
            double p0,
            double p1,
            double p2,
            double p3)
        {
            var a = -p0 + 3d * p1 - 3d * p2 + p3;
            var b = 2d * (p0 - 2d * p1 + p2);
            var c = p1 - p0;

            foreach (var t in MathHelpers.SolveQuadratic(3d * a, 3d * b, 3d * c))
            {
                if (t > 0d && t < 1d)
                {
                    yield return t;
                }
            }
        }

        private static double EvaluateCubic(
            double p0,
            double p1,
            double p2,
            double p3,
            double t)
        {
            var mt = 1d - t;

            return
                mt * mt * mt * p0 +
                3d * mt * mt * t * p1 +
                3d * mt * t * t * p2 +
                t * t * t * p3;
        }
    }
}
