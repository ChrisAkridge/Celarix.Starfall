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
        /// <summary>
        /// Gets a value indicating the size of the font in points. If null, the text will fill the
        /// available space. It will expand until it reaches the size of the available width or height,
        /// whichever is smaller.
        /// </summary>
        public abstract float? Size { get; }

        public abstract SKFont ToSKFont();
        public abstract SFont WithSize(float? newSize);
        public abstract string ToCacheKey();
    }

    public sealed class SFontFamily : SFont
    {
        public string Family { get; }
        public override float? Size { get; }
        public FontWeight FontWeight { get; }
        public FontWidth FontWidth { get; }
        public FontSlant FontSlant { get; }

        public SFontFamily(string family, float? size, FontWeight fontWeight, FontWidth fontWidth, FontSlant fontSlant)
        {
            Family = family;
            Size = size;
            FontWeight = fontWeight;
            FontWidth = fontWidth;
            FontSlant = fontSlant;
        }

        public SFontFamily(string family, float? size = null)
            : this(family, size, FontWeight.Normal, FontWidth.Normal, FontSlant.Upright)
        {
        }

        public override SKFont ToSKFont()
        {
            // SKFont wants a real size, so we'll make one up...
            const float defaultSize = 12f;
            // ...but we will check for null on Size and make sure we instead fill the available space if it is.

            return new SKFont(
                SKTypeface.FromFamilyName(
                    Family,
                    FontWeight.ToSKFontStyleWeight(),
                    FontWidth.ToSKFontStyleWidth(),
                    FontSlant.ToSKFontStyleSlant()),
                    Size ?? defaultSize);
        }

        public override SFont WithSize(float? newSize)
        {
            if (newSize == Size) { return this; }

            return new SFontFamily(Family, newSize, FontWeight, FontWidth, FontSlant);
        }

        public override string ToCacheKey() => $"{Family}|{Size}|{FontWeight}|{FontWidth}|{FontSlant}";
    }

    public sealed class SFontFile : SFont
    {
        public string FilePath { get; }
        public override float? Size { get; }
        public int Index { get; } = 0;

        public SFontFile(string filePath, float? size, int index = 0)
        {
            FilePath = filePath;
            Size = size;
            Index = index;
        }

        public override SKFont ToSKFont()
        {
            const float defaultSize = 12f;
            return new SKFont(
                SKTypeface.FromFile(FilePath, Index),
                Size ?? defaultSize);
        }

        public override SFont WithSize(float? newSize)
        {
            if (newSize == Size) { return this; }
            return new SFontFile(FilePath, newSize, Index);
        }

        public override string ToCacheKey() => $"{FilePath}|{Size}|{Index}";
    }
}
