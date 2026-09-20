using Celarix.Starfall.Charts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public static class TickFormatters
{
    public static Func<BigInteger, ChartText> DateOnlyFormatter(DateOnly firstDate, string formatString = "yyyy-MM-dd",
        string outOfBoundsString = "OoB")
    {
        var daysUntilMaxDate = (DateOnly.MaxValue.DayNumber - firstDate.DayNumber);
        var daysAfterMinDate = (firstDate.DayNumber - DateOnly.MinValue.DayNumber);

        return (BigInteger x) =>
        {
            if (x > daysUntilMaxDate || x < -daysAfterMinDate)
            {
                return ChartText.String(outOfBoundsString);
            }

            var date = firstDate.AddDays((int)x);
            return ChartText.String(date.ToString(formatString));
        };
    }

    public static Func<double, ChartText> NaNAsDash(Func<double, string> innerFormatter)
    {
        return d =>
        {
            if (double.IsNaN(d))
            {
                return ChartText.String("-");
            }
            return ChartText.String(innerFormatter(d));
        };
    }

    public static Func<double, ChartText> NaNAsDash(Func<double, ChartText> innerFormatter)
    {
        return d =>
        {
            if (double.IsNaN(d))
            {
                return ChartText.String("-");
            }
            return innerFormatter(d);
        };
    }

    public static ChartText QuantityToSIPrefixed(double quantity, string unitAbbreviation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitAbbreviation);

        if (quantity == 0d)
        {
            return ChartText.String($"0 {unitAbbreviation}");
        }

        if (!double.IsFinite(quantity))
        {
            return ChartText.String(quantity switch
            {
                double.NaN => "NaN",
                double.PositiveInfinity => "∞",
                double.NegativeInfinity => "-∞",
                _ => throw new UnreachableException()
            });
        }

        var exponent = (int)Math.Floor(Math.Log10(Math.Abs(quantity)));

        if (exponent < -30 || exponent >= 33)
        {
            var mantissa = quantity / Math.Pow(10d, exponent);

            return ChartText.Libra(
                $"{mantissa:0.###} * 10^{exponent} \"{unitAbbreviation}\"");
        }

        var prefixExponent = Math.Floor(exponent / 3d) * 3d;
        var prefix = prefixExponent switch
        {
            -30 => "q",
            -27 => "r",
            -24 => "y",
            -21 => "z",
            -18 => "a",
            -15 => "f",
            -12 => "p",
            -9 => "n",
            -6 => "µ",
            -3 => "m",
            0 => "",
            3 => "k",
            6 => "M",
            9 => "G",
            12 => "T",
            15 => "P",
            18 => "E",
            21 => "Z",
            24 => "Y",
            27 => "R",
            30 => "Q",
            _ => throw new UnreachableException()
        };

        var scaledQuantity = quantity / Math.Pow(10d, prefixExponent);

        return ChartText.String(
            $"{scaledQuantity:0.###} {prefix}{unitAbbreviation}");
    }
}
