using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class SBlendModeExtensions
    {
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
