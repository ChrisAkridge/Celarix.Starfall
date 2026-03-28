using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public struct SColor
    {
        private readonly byte red;
        private readonly byte green;
        private readonly byte blue;
        private readonly byte alpha;

        public readonly byte R => red;
        public readonly byte G => green;
        public readonly byte B => blue;
        public readonly byte A => alpha;
        public readonly int PackedArgb => (alpha << 24) | (red << 16) | (green << 8) | blue;

        public SColor(byte red, byte green, byte blue, byte alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        public override readonly string ToString() => $"#{R:X2}{G:X2}{B:X2} (alpha {A:X2})";

        public readonly SKColor ToSKColor() => new(R, G, B, A);

        public readonly HSV ToHSV()
        {
            double r = red / 255.0;
            double g = green / 255.0;
            double b = blue / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double h = 0;
            if (max == min)
            {
                h = 0; // achromatic
            }
            else if (max == r)
            {
                h = (g - b) / (max - min);
                if (g < b) { h += 6; }
            }
            else if (max == g)
            {
                h = (b - r) / (max - min) + 2;
            }
            else
            {
                h = (r - g) / (max - min) + 4;
            }
            h *= 60;
            double s = max == 0 ? 0 : (max - min) / max;
            double v = max;
            return new HSV(h, s, v);
        }

        public static SColor FromArgb(byte alpha, byte red, byte green, byte blue)
        {
            return new SColor(red, green, blue, alpha);
        }

        public static SColor FromPackedArgb(int packedArgb)
        {
            byte alpha = (byte)((packedArgb >> 24) & 0xFF);
            byte red = (byte)((packedArgb >> 16) & 0xFF);
            byte green = (byte)((packedArgb >> 8) & 0xFF);
            byte blue = (byte)(packedArgb & 0xFF);
            return new SColor(red, green, blue, alpha);
        }

        public static SColor FromHSV(double h, double s, double v)
        {
            h = h % 360;
            if (h < 0) { h += 360; };
            s = Math.Clamp(s, 0, 1);
            v = Math.Clamp(v, 0, 1);
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;
            double rPrime;
            double bPrime;
            if (h < 60)
            {
                rPrime = c;
                bPrime = 0;
            }
            else if (h < 120)
            {
                rPrime = x;
                bPrime = 0;
            }
            else if (h < 180)
            {
                rPrime = 0;
                bPrime = x;
            }
            else if (h < 240)
            {
                rPrime = 0;
                bPrime = c;
            }
            else if (h < 300)
            {
                rPrime = x;
                bPrime = c;
            }
            else
            {
                rPrime = c;
                bPrime = x;
            }
            byte red = (byte)((rPrime + m) * 255);
            byte green = (byte)((0 + m) * 255);
            byte blue = (byte)((bPrime + m) * 255);
            return new SColor(red, green, blue, 255);
        }
        
        public static SColor FromHSV(HSV hsv)
        {
            return FromHSV(hsv.H, hsv.S, hsv.V);
        }

        public static SColor FromHtmlAttribute(string value, SColor fallback)
        {
            // Trim the leading # if any
            if (value.StartsWith("#"))
            {
                value = value[1..];
            }

            // Check all remaining characters are valid hex digits
            if (!value.All(Uri.IsHexDigit))
            {
                return fallback;
            }

            // No "chucknorris" parsing here, just a simple hex code
            var hexLength = value.Length;
            var hasAlpha = hexLength is 4 or 8;
            var isWebSafe = hexLength <= 4;

            if (isWebSafe)
            {
                // Interleave characters with '0'
                var webSafeValue = new StringBuilder();
                foreach (var c in value)
                {
                    webSafeValue.Append(c);
                    webSafeValue.Append('0');
                }
                value = webSafeValue.ToString();
            }

            if (!hasAlpha)
            {
                // Assume full opacity if alpha is not provided
                value += "FF";
            }

            // Parse order is RGBA
            var redPart = value[0..2];
            var greenPart = value[2..4];
            var bluePart = value[4..6];
            var alphaPart = value[6..8];
            
            var red = byte.Parse(redPart, System.Globalization.NumberStyles.HexNumber);
            var green = byte.Parse(greenPart, System.Globalization.NumberStyles.HexNumber);
            var blue = byte.Parse(bluePart, System.Globalization.NumberStyles.HexNumber);
            var alpha = byte.Parse(alphaPart, System.Globalization.NumberStyles.HexNumber);
            
            return new SColor(red, green, blue, alpha);
        }

        public static SColor FromName(string name, SColor fallback)
        {
            return name.ToLowerInvariant() switch
            {
                "transparent" => Transparent,
                "black" => Black,
                "white" => White,
                "red" => Red,
                "green" => Green,
                "blue" => Blue,
                "rebeccapurple" => RebeccaPurple,
                _ => throw new ArgumentException($"Unknown color name: {name}"),
            };
        }

        #region Defined Colors
        public static readonly SColor Transparent = new(0, 0, 0, 0);
        public static readonly SColor Black = new(0, 0, 0, 255);
        public static readonly SColor White = new(255, 255, 255, 255);
        public static readonly SColor Red = new(255, 0, 0, 255);
        public static readonly SColor Green = new(0, 255, 0, 255);
        public static readonly SColor Blue = new(0, 0, 255, 255);
        public static readonly SColor RebeccaPurple = new(0x33, 0x66, 0x99, 255);
        #endregion
    }
}
