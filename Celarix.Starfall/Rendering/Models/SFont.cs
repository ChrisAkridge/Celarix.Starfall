using Celarix.Starfall.Rendering.Converters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    // TODO: Make size nullable since we can let users fit to a bounds
    public abstract class SFont
    {
        public abstract float Size { get; }

        public abstract SKFont ToSKFont();
        public abstract SFont WithSize(float newSize);
    }

    public sealed class SFontFamily : SFont
    {
        public string Family { get; }
        public override float Size { get; }
        public FontWeight FontWeight { get; }
        public FontWidth FontWidth { get; }
        public FontSlant FontSlant { get; }

        public SFontFamily(string family, float size, FontWeight fontWeight, FontWidth fontWidth, FontSlant fontSlant)
        {
            Family = family;
            Size = size;
            FontWeight = fontWeight;
            FontWidth = fontWidth;
            FontSlant = fontSlant;
        }

        public SFontFamily(string family, float size)
            : this(family, size, FontWeight.Normal, FontWidth.Normal, FontSlant.Upright)
        {
        }

        public override SKFont ToSKFont()
        {
            return new SKFont(
                SKTypeface.FromFamilyName(
                    Family,
                    FontWeight.ToSKFontStyleWeight(),
                    FontWidth.ToSKFontStyleWidth(),
                    FontSlant.ToSKFontStyleSlant()),
                    Size);
        }

        public override SFont WithSize(float newSize)
        {
            return new SFontFamily(Family, newSize, FontWeight, FontWidth, FontSlant);
        }
    }

    public sealed class SFontFile : SFont
    {
        public string FilePath { get; }
        public override float Size { get; }
        public int Index { get; } = 0;

        public SFontFile(string filePath, float size, int index = 0)
        {
            FilePath = filePath;
            Size = size;
            Index = index;
        }

        public override SKFont ToSKFont()
        {
            return new SKFont(
                SKTypeface.FromFile(FilePath, Index),
                Size);
        }

        public override SFont WithSize(float newSize)
        {
            return new SFontFile(FilePath, newSize, Index);
        }
    }
}
