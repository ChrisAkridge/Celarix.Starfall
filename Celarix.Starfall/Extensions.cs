using NAudio.Wave;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Celarix.Starfall
{
    public static class Extensions
    {
        private static char[] spaceSeparators;

        static Extensions()
        {
            var charInfo = Enumerable.Range(0, 0x110000)
                .Where(c => c < 0x00d800 || c > 0x00dfff) // Exclude surrogate code points
                .Select(char.ConvertFromUtf32)
                .GroupBy(s => char.GetUnicodeCategory(s, 0))
                .ToDictionary(g => g.Key);

            spaceSeparators = [.. charInfo[System.Globalization.UnicodeCategory.SpaceSeparator].SelectMany(s => s)];
        }

        public static string[] TokenizePreservingSpaces(this string line)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool? lastWasSpace = null;

            foreach (var ch in line)
            {
                bool isSpace = spaceSeparators.Contains(ch);

                if (lastWasSpace is null)
                {
                    // first character
                    sb.Append(ch);
                    lastWasSpace = isSpace;
                    continue;
                }

                if (isSpace == lastWasSpace)
                {
                    // same category (space vs non-space) -> accumulate
                    sb.Append(ch);
                }
                else
                {
                    // category changed -> flush current token, start new
                    tokens.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(ch);
                    lastWasSpace = isSpace;
                }
            }

            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
            }

            return tokens.ToArray();
        }

        public static SKImage CreateSKImageFromImageSharp(this Image<Rgba32> image)
        {
            var buffer = new byte[image.Width * image.Height * 4];
            var pixel = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        var pixelData = row[x];
                        buffer[pixel++] = pixelData.R;
                        buffer[pixel++] = pixelData.G;
                        buffer[pixel++] = pixelData.B;
                        buffer[pixel++] = pixelData.A;
                    }
                }
            });

            var skImage = SKImage.FromPixelCopy(
                new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888),
                buffer,
                image.Width * 4);
            return skImage;
        }

        private static readonly Regex WhitespaceMatch = new(@"\s+", RegexOptions.Compiled);
        public static string RemoveWhitespace(this string input)
        {
            return WhitespaceMatch.Replace(input, string.Empty);
        }

        public static int[] IndexesOf(this string input, char searchChar)
        {
            var indices = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == searchChar)
                {
                    indices.Add(i);
                }
            }
            return [.. indices];
        }

        public static SKShaderTileMode ToSKShaderTileMode(this SShaderTileMode tileMode) => tileMode switch
        {
            SShaderTileMode.Clamp => SKShaderTileMode.Clamp,
            SShaderTileMode.Decal => SKShaderTileMode.Decal,
            SShaderTileMode.Repeat => SKShaderTileMode.Repeat,
            SShaderTileMode.Mirror => SKShaderTileMode.Mirror,
            _ => throw new ArgumentOutOfRangeException(nameof(tileMode))
        };

        public static SKBlendMode ToSKBlendMode(this SBlendMode blendMode) => blendMode switch
        {
            SBlendMode.Clear => SKBlendMode.Clear,
            SBlendMode.Src => SKBlendMode.Src,
            SBlendMode.Dst => SKBlendMode.Dst,
            SBlendMode.SrcOver => SKBlendMode.SrcOver,
            SBlendMode.DstOver => SKBlendMode.DstOver,
            SBlendMode.SrcIn => SKBlendMode.SrcIn,
            SBlendMode.DstIn => SKBlendMode.DstIn,
            SBlendMode.SrcOut => SKBlendMode.SrcOut,
            SBlendMode.DstOut => SKBlendMode.DstOut,
            SBlendMode.SrcATop => SKBlendMode.SrcATop,
            SBlendMode.DstATop => SKBlendMode.DstATop,
            SBlendMode.Xor => SKBlendMode.Xor,
            SBlendMode.Plus => SKBlendMode.Plus,
            SBlendMode.Modulate => SKBlendMode.Modulate,
            SBlendMode.Screen => SKBlendMode.Screen,
            SBlendMode.Overlay => SKBlendMode.Overlay,
            SBlendMode.Darken => SKBlendMode.Darken,
            SBlendMode.Lighten => SKBlendMode.Lighten,
            SBlendMode.ColorDodge => SKBlendMode.ColorDodge,
            SBlendMode.ColorBurn => SKBlendMode.ColorBurn,
            SBlendMode.HardLight => SKBlendMode.HardLight,
            SBlendMode.SoftLight => SKBlendMode.SoftLight,
            SBlendMode.Difference => SKBlendMode.Difference,
            SBlendMode.Exclusion => SKBlendMode.Exclusion,
            SBlendMode.Multiply => SKBlendMode.Multiply,
            SBlendMode.Hue => SKBlendMode.Hue,
            SBlendMode.Saturation => SKBlendMode.Saturation,
            SBlendMode.Color => SKBlendMode.Color,
            SBlendMode.Luminosity => SKBlendMode.Luminosity,
            _ => throw new ArgumentOutOfRangeException(nameof(blendMode))
        };
    }
}
