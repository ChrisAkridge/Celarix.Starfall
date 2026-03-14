using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    public delegate double Easing(double t);

    public static class Easings
    {
        public static double Linear(double t) => t;
        public static double Smoothstep(double t) => t * t * (3 - 2 * t);
        public static double StepStart(double t) => t > 0 ? 1 : 0;
        public static double StepEnd(double t) => t >= 1 ? 1 : 0;
    }
}
