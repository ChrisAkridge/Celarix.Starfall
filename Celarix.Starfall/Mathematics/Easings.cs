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

        /// <summary>
        /// Accelerates away from rest. This is the departure counterpart to <see cref="Land"/>.
        /// </summary>
        public static double LiftOff(double t) => TakeOff(t);

        public static double TakeOffFaster(double t, double exponent) => Math.Clamp(Math.Pow(t, exponent), 0, 1);

        /// <summary>
        /// An easing representing a constant deceleration to zero velocity.
        /// Useful for arriving elements in transitions, as it gives a nice "landing" effect.
        /// </summary>
        public static double Land(double t) => 1 - (1 - t) * (1 - t);

        public static double LandFaster(double t, double exponent) => 1 - Math.Pow(1 - t, exponent);

        /// <summary>
        /// Moves beyond the target before reversing direction and settling exactly on it.
        /// Useful for giving an arriving value a single spring-like overshoot without oscillation.
        /// </summary>
        /// <param name="t">Animation progress from 0 to 1.</param>
        /// <param name="overshoot">
        /// A unitless curve-shape coefficient, not an overshoot percentage. Larger values travel
        /// farther beyond the target. The conventional value of 1.70158 produces approximately
        /// 10% positional overshoot; 0.7 produces a subtler effect.
        /// </param>
        /// <returns>
        /// Eased progress that may exceed 1 between the endpoints, and is exactly 1 when
        /// <paramref name="t"/> is 1.
        /// </returns>
        public static double BackOut(double t, double overshoot = 1.70158d)
        {
            t = Math.Clamp(t, 0d, 1d);
            var shifted = t - 1d;
            return 1d
                + ((overshoot + 1d) * shifted * shifted * shifted)
                + (overshoot * shifted * shifted);
        }

        /// <summary>
        /// Preserves the incoming velocity of a linear motion, then gently settles to zero velocity.
        /// </summary>
        public static double SoftLand(double t)
        {
            t = Math.Clamp(t, 0d, 1d);
            return (-t * t * t) + (t * t) + t;
        }

        /// <summary>
        /// Runs one easing up to <paramref name="switchAt"/>, then runs another easing for the
        /// remainder. Each easing is remapped to its own portion of the input and output ranges.
        /// </summary>
        public static Easing Dual(Easing first, Easing second, double switchAt)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            if (switchAt <= 0d || switchAt >= 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(switchAt), "Switch point must be between 0 and 1.");
            }

            return t =>
            {
                t = Math.Clamp(t, 0d, 1d);
                if (t < switchAt)
                {
                    return switchAt * first(t / switchAt);
                }

                var secondProgress = (t - switchAt) / (1d - switchAt);
                return switchAt + ((1d - switchAt) * second(secondProgress));
            };
        }

        /// <summary>
        /// Creates an easing with constant acceleration, constant cruising velocity, and constant
        /// deceleration. The resulting position and velocity are continuous at both transitions.
        /// </summary>
        /// <param name="accelerationFraction">
        /// The fraction of the total duration spent accelerating from rest.
        /// </param>
        /// <param name="decelerationFraction">
        /// The fraction of the total duration spent decelerating to rest.
        /// </param>
        /// <returns>
        /// An easing whose remaining duration is spent moving at a constant velocity.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A fraction is outside the range 0 through 1, or the two fractions total more than 1.
        /// </exception>
        public static Easing AccelerateCruiseDecelerate(
            double accelerationFraction,
            double decelerationFraction)
        {
            if (accelerationFraction < 0d || accelerationFraction > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(accelerationFraction),
                    "Acceleration fraction must be between 0 and 1.");
            }
            if (decelerationFraction < 0d || decelerationFraction > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decelerationFraction),
                    "Deceleration fraction must be between 0 and 1.");
            }
            if (accelerationFraction + decelerationFraction > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decelerationFraction),
                    "Acceleration and deceleration fractions must total no more than 1.");
            }

            var cruiseEnd = 1d - decelerationFraction;
            var peakVelocity = 1d
                / (1d - ((accelerationFraction + decelerationFraction) / 2d));

            return t =>
            {
                t = Math.Clamp(t, 0d, 1d);

                if (accelerationFraction > 0d && t < accelerationFraction)
                {
                    return peakVelocity * t * t / (2d * accelerationFraction);
                }

                if (decelerationFraction > 0d && t > cruiseEnd)
                {
                    var remaining = 1d - t;
                    return 1d
                        - (peakVelocity * remaining * remaining / (2d * decelerationFraction));
                }

                return peakVelocity * (t - (accelerationFraction / 2d));
            };
        }

        public static double TakeOffCliff(double t, double exponent)
        {
            t = Math.Clamp(t, 0, 1);
            exponent = Math.Max(exponent, 0);

            // 1 - (1 - t)^exponent
            return 1d - Math.Pow(1d - t, exponent);
        }
    }
}
