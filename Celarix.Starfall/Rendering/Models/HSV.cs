using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct HSV(double h, double s, double v)
    {
        public double H { get; } = h;
        public double S { get; } = s;
        public double V { get; } = v;
    }
}
