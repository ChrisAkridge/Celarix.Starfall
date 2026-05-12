using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    public static class Interpolators
    {
        private static readonly Dictionary<Type, object> _registry = new()
        {
            [typeof(double)] = new DoubleInterpolator(),
            [typeof(SPoint)] = new SPointInterpolator(),
            [typeof(SPointF)] = new SPointFInterpolator(),
            [typeof(SColor)] = new SColorInterpolator(),
        };

        public static IInterpolator<TProp> Get<TProp>()
        {
            if (_registry.TryGetValue(typeof(TProp), out var interpolator))
                return (IInterpolator<TProp>)interpolator;
            throw new InvalidOperationException($"No interpolator registered for {typeof(TProp).Name}");
        }

        public static void Register<TProp>(IInterpolator<TProp> interpolator)
        {
            _registry[typeof(TProp)] = interpolator;
        }
    }

    public interface IInterpolator<TProp>
    {
        TProp Interpolate(TProp from, TProp to, double progress);
    }

    public sealed class DoubleInterpolator : IInterpolator<double>
    {
        public double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }
    }

    public sealed class SPointInterpolator : IInterpolator<SPoint>
    {
        public SPoint Interpolate(SPoint from, SPoint to, double progress)
        {
            return new SPoint(
                (int)(from.X + (to.X - from.X) * progress),
                (int)(from.Y + (to.Y - from.Y) * progress)
            );
        }
    }

    public sealed class SPointFInterpolator : IInterpolator<SPointF>
    {
        public SPointF Interpolate(SPointF from, SPointF to, double progress)
        {
            return new SPointF(
                (float)(from.X + (to.X - from.X) * progress),
                (float)(from.Y + (to.Y - from.Y) * progress)
            );
        }
    }

    public sealed class SColorInterpolator : IInterpolator<SColor>
    {
        public SColor Interpolate(SColor from, SColor to, double progress)
        {
            byte r = (byte)(from.R + (to.R - from.R) * progress);
            byte g = (byte)(from.G + (to.G - from.G) * progress);
            byte b = (byte)(from.B + (to.B - from.B) * progress);
            byte a = (byte)(from.A + (to.A - from.A) * progress);
            return new SColor(r, g, b, a);
        }
    }
}
