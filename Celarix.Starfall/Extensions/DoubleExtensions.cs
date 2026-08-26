using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions;

public static class DoubleExtensions
{
    public static bool EqualsWithTolerance(this double a, double b, double tolerance = 0.0001)
    {
        return Math.Abs(a - b) <= tolerance;
    }

    public static void ThrowIfNotPositive(this double value, string paramName = "value")
    {
        if (value <= 0d)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }
    }
}
