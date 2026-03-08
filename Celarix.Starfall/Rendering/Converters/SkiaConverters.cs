using Celarix.Starfall.Rendering.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Converters
{
    internal static class SkiaConverters
    {
        public static SKRect ToSKRect(this SRectF rect)
        {
            return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }

        public static SKFontStyleWeight ToSKFontStyleWeight(this FontWeight fontWeight)
        {
            return fontWeight switch
            {
                FontWeight.Invisible => SKFontStyleWeight.Invisible,
                FontWeight.Thin => SKFontStyleWeight.Thin,
                FontWeight.ExtraLight => SKFontStyleWeight.ExtraLight,
                FontWeight.Light => SKFontStyleWeight.Light,
                FontWeight.Normal => SKFontStyleWeight.Normal,
                FontWeight.Medium => SKFontStyleWeight.Medium,
                FontWeight.SemiBold => SKFontStyleWeight.SemiBold,
                FontWeight.Bold => SKFontStyleWeight.Bold,
                FontWeight.ExtraBold => SKFontStyleWeight.ExtraBold,
                FontWeight.Black => SKFontStyleWeight.Black,
                FontWeight.ExtraBlack => SKFontStyleWeight.ExtraBlack,
                _ => throw new ArgumentOutOfRangeException(nameof(fontWeight), $"Unsupported font weight: {fontWeight}")
            };
        }

        public static SKFontStyleWidth ToSKFontStyleWidth(this FontWidth fontWidth)
        {
            return fontWidth switch
            {
                FontWidth.UltraCondensed => SKFontStyleWidth.UltraCondensed,
                FontWidth.ExtraCondensed => SKFontStyleWidth.ExtraCondensed,
                FontWidth.Condensed => SKFontStyleWidth.Condensed,
                FontWidth.SemiCondensed => SKFontStyleWidth.SemiCondensed,
                FontWidth.Normal => SKFontStyleWidth.Normal,
                FontWidth.SemiExpanded => SKFontStyleWidth.SemiExpanded,
                FontWidth.Expanded => SKFontStyleWidth.Expanded,
                FontWidth.ExtraExpanded => SKFontStyleWidth.ExtraExpanded,
                FontWidth.UltraExpanded => SKFontStyleWidth.UltraExpanded,
                _ => throw new ArgumentOutOfRangeException(nameof(fontWidth), $"Unsupported font width: {fontWidth}")
            };
        }

        public static SKFontStyleSlant ToSKFontStyleSlant(this FontSlant fontSlant)
        {
            return fontSlant switch
            {
                FontSlant.Upright => SKFontStyleSlant.Upright,
                FontSlant.Oblique => SKFontStyleSlant.Oblique,
                FontSlant.Italic => SKFontStyleSlant.Italic,
                _ => throw new ArgumentOutOfRangeException(nameof(fontSlant), $"Unsupported font slant: {fontSlant}")
            };
        }
    }
}
