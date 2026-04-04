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

        /// <summary>
        /// An easing representing a constant acceleration from zero velocity.
        /// Useful for departing elements in transitions, as it gives a nice "takeoff" effect.
        /// The element starts moving slowly and then speeds up, which can make the departure feel more natural and less abrupt.
        /// </summary>
        public static double TakeOff(double t) => Math.Clamp(t * t, 0, 1);

        public static double TakeOffFaster(double t, double exponent) => Math.Clamp(Math.Pow(t, exponent), 0, 1);

        /// <summary>
        /// An easing representing a constant deceleration to zero velocity.
        /// Useful for arriving elements in transitions, as it gives a nice "landing" effect.
        /// </summary>
        public static double Land(double t) => 1 - (1 - t) * (1 - t);

        public static double TakeOffCliff(double t, double exponent)
        {
            t = Math.Clamp(t, 0, 1);
            exponent = Math.Max(exponent, 0);

            // 1 - (1 - t)^exponent
            return 1d - Math.Pow(1d - t, exponent);
        }
    }
}
