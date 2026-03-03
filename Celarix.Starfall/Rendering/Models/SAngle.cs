using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SAngle
    {
        public static readonly SAngle Zero = new(0);

        public double Degrees { get; init; }
        public readonly double Radians => Degrees * (double)(Math.PI / 180);

        public SAngle(double degrees)
        {
            if (degrees < 0 || degrees >= 360)
            {
                degrees = degrees % 360;
                if (degrees < 0)
                {
                    degrees += 360;
                }
            }
            Degrees = degrees;
        }

        public override readonly string ToString() => $"{Degrees}° ({Radians} rad)";

        public static SAngle FromRadians(double radians)
        {
            return new SAngle(radians * (double)(180 / Math.PI));
        }
    }
}
