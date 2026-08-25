using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public static class TickFormatters
{
    public static Func<BigInteger, string> DateOnlyFormatter(DateOnly firstDate, string formatString = "yyyy-MM-dd",
        string outOfBoundsString = "OoB")
    {
        var daysUntilMaxDate = (DateOnly.MaxValue.DayNumber - firstDate.DayNumber);
        var daysAfterMinDate = (firstDate.DayNumber - DateOnly.MinValue.DayNumber);

        return (BigInteger x) =>
        {
            if (x > daysUntilMaxDate || x < -daysAfterMinDate)
            {
                return outOfBoundsString;
            }

            var date = firstDate.AddDays((int)x);
            return date.ToString(formatString);
        };
    }
}
