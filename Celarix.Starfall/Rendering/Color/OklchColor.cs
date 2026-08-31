using Celarix.Starfall.Extensions;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Color;

public readonly record struct OklchColor
{
    public static OklchColor Black => new(0, 0, 0);
    public static OklchColor White => new(1, 0, 0);

    public double Lightness { get; init; }
    public double Chroma { get; init; }
    public double Hue { get; init; }

    public OklchColor(double l, double c, double h)
    {
        Lightness = l;
        Chroma = c;
        Hue = h;
    }

    public OklchColor(SColor scolor)
    {
        var normalizedR = scolor.R / 255.0d;
        var normalizedG = scolor.G / 255.0d;
        var normalizedB = scolor.B / 255.0d;

        var linearR = RemoveSRGBTransfer(normalizedR);
        var linearG = RemoveSRGBTransfer(normalizedG);
        var linearB = RemoveSRGBTransfer(normalizedB);

        var l0 = 0.4122214708 * linearR;
        var l1 = 0.5363325363 * linearG;
        var l2 = 0.0514459929 * linearB;
        var l = l0 + l1 + l2;

        var m0 = 0.2119034982 * linearR;
        var m1 = 0.6806995451 * linearG;
        var m2 = 0.1073969567 * linearB;
        var m = m0 + m1 + m2;

        var s0 = 0.0883024619 * linearR;
        var s1 = 0.2817188376 * linearG;
        var s2 = 0.6299787005 * linearB;
        var s = s0 + s1 + s2;

        var lRoot = Math.Pow(l, 1.0 / 3.0);
        var mRoot = Math.Pow(m, 1.0 / 3.0);
        var sRoot = Math.Pow(s, 1.0 / 3.0);

        var L0 = 0.2104542553 * lRoot;
        var L1 = 0.7936177850 * mRoot;
        var L2 = -0.0040720468 * sRoot;
        Lightness = L0 + L1 + L2;

        var a0 = 1.9779984951 * lRoot;
        var a1 = -2.4285922050 * mRoot;
        var a2 = 0.4505937099 * sRoot;
        var a = a0 + a1 + a2;

        var b0 = 0.0259040371 * lRoot;
        var b1 = 0.7827717662 * mRoot;
        var b2 = -0.8086757660 * sRoot;
        var b = b0 + b1 + b2;

        Chroma = Math.Sqrt(a * a + b * b);
        Hue = Math.Atan2(b, a) * (180.0 / Math.PI);
        if (Hue < 0)
        {
            Hue += 360.0;
        }
    }

    public SColor ToSColor()
    {
        var low = 0.0;
        var high = Math.Max(0.0, Chroma);

        var (linearR, linearG, linearB) = GetLinearRGBForChroma(high);

        if (!IsInGamut(linearR, linearG, linearB))
        {
            for (var i = 0; i < 32; i++)
            {
                var mid = (low + high) / 2.0;
                var rgb = GetLinearRGBForChroma(mid);

                if (IsInGamut(rgb.LinearR, rgb.LinearG, rgb.LinearB))
                {
                    low = mid;
                    (linearR, linearG, linearB) = rgb;
                }
                else
                {
                    high = mid;
                }
            }
        }

        var red = ApplySRGBTransfer(linearR);
        var green = ApplySRGBTransfer(linearG);
        var blue = ApplySRGBTransfer(linearB);

        return new SColor(
            (byte)Math.Clamp((int)Math.Round(red * 255.0), 0, 255),
            (byte)Math.Clamp((int)Math.Round(green * 255.0), 0, 255),
            (byte)Math.Clamp((int)Math.Round(blue * 255.0), 0, 255),
            255
        );
    }

    private static bool IsInGamut(double r, double g, double b)
    {
        return double.IsFinite(r)
            && double.IsFinite(g)
            && double.IsFinite(b)
            && r is >= 0 and <= 1
            && g is >= 0 and <= 1
            && b is >= 0 and <= 1;
    }

    private static double ApplySRGBTransfer(double channel)
    {
        if (channel < 0.0031308d)
        {
            return 12.92d * channel;
        }
        else
        {
            return 1.055d * Math.Pow(channel, 1.0 / 2.4) - 0.055d;
        }
    }

    private static double RemoveSRGBTransfer(double channel)
    {
        if (channel < 0.0405d)
        {
            return channel / 12.92d;
        }
        else
        {
            return Math.Pow((channel + 0.055d) / 1.055d, 2.4d);
        }
    }

    private (double LinearR, double LinearG, double LinearB) GetLinearRGBForChroma(double chroma)
    {
        var a = chroma * Math.Cos(Hue * (Math.PI / 180.0));
        var b = chroma * Math.Sin(Hue * (Math.PI / 180.0));

        var lPrime = Lightness + 0.3963377774 * a + 0.2158037573 * b;
        var mPrime = Lightness - 0.1055613458 * a - 0.0638541728 * b;
        var sPrime = Lightness - 0.0894841775 * a - 1.2914855480 * b;

        var l = lPrime * lPrime * lPrime;
        var m = mPrime * mPrime * mPrime;
        var s = sPrime * sPrime * sPrime;

        double linearR = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        double linearG = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        double linearB = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;
        return (linearR, linearG, linearB);
    }
}
