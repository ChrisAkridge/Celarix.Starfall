using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class Int32Extensions
    {
        public static void ThrowIfNotPositive(this int value, string paramName = "value")
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
            }
        }
    }
}
