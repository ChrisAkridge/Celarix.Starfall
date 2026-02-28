using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SPointF
    {
        public static readonly SPointF Zero = new(0, 0);

        public double X { get; }
        public double Y { get; }
        public SPointF(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
