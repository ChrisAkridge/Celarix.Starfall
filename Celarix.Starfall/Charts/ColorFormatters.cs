using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts;

/// <summary>
/// A class of static methods that return functions that provide a color for a given value in a range.
/// To use these functions, compute the range of values in your data set, and normalize the value to
/// a 0-1 range.
/// </summary>
public static class ColorFormatters
{
    public static Func<double, SColor> SingleColor(SColor color)
    {
        return _ => color;
    }

    public static Func<double, SColor> HueWheel(double saturation = 1.0d, double value = 1.0d)
    {
        return value =>
        {
            var hue = value * 360.0d;
            return SColor.FromHSV(hue, saturation, value);
        };
    }
}
