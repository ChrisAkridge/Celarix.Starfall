using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;
using static OpenTK.Graphics.OpenGL.GL;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class SingleBinaryViewElement : AtriaElement
    {
        private const float SegmentMarginMultipleOfBitWidth = 0.25f;
        private const float SectionBackgroundMarginMultipleOfBitWidth = 0.1f;
        private const float SectionBackgroundOpacity = 0.25f;
        private const float DescriptionFontSizeMultiple = 0.25f;

        private const string SignDescription = "Sign";
        private const string ExponentDescription = "Exponent (8 bits)";
        private const string MantissaDescription = "Mantissa (23 bits)";
        private string _fontFamily;
        private float _baseFontSize = 36f;
        private SFont _font;
        private SFont _descriptionFont;
        private SSizeF? _bitSize;

        private static readonly SColor SignColor = SColor.Red;
        private static readonly SColor ExponentColor = SColor.Green;
        private static readonly SColor MantissaColor = SColor.Cyan;

        private string SignBit => Value < 0 ? "1" : "0";
        private string ExponentBits
        {
            get
            {
                var bits = BitConverter.SingleToInt32Bits(Value);
                var exponentBits = (bits >> 23) & 0xFF;
                return Convert.ToString(exponentBits, 2).PadLeft(8, '0');
            }
        }

        private string MantissaBits
        {
            get
            {
                var bits = BitConverter.SingleToInt32Bits(Value);
                var mantissaBits = bits & 0x7FFFFF;
                return Convert.ToString(mantissaBits, 2).PadLeft(23, '0');
            }
        }

        private string AsHex
        {
            get
            {
                var bits = BitConverter.SingleToUInt32Bits(Value);
                return $"{bits >> 24:X2} {bits >> 16 & 0xFF:X2} {bits >> 8 & 0xFF:X2} {bits & 0xFF:X2}";
            }
        }

        private string SignValueDescription => Value < 0 ? "NEG" : "POS";
        private string ExponentValueDescription
        {
            get
            {
                var bits = BitConverter.SingleToInt32Bits(Value);
                var exponentBits = (bits >> 23) & 0xFF;
                var exponentValue = exponentBits - 127; // Subtract bias for single-precision
                return $"Raw {exponentBits} - bias 127 = {exponentValue}";
            }
        }
        private string MantissaValueDescription
        {
            get
            {
                var bits = BitConverter.SingleToInt32Bits(Value);
                var exponentBits = (bits >> 23) & 0xFF;

                if (exponentBits == 0)
                {
                    // Subnormal number
                    return "Implied leading 0 bit";
                }
                else if (exponentBits == 255)
                {
                    // Infinity or NaN
                    return "Special case (Infinity or NaN)";
                }
                return "Implied leading 1 bit";
            }
        }

        public float BaseFontSize
        {
            get => _baseFontSize;
            set
            {
                _baseFontSize = value;
                _font = new SFontFamily(_fontFamily, value);
                _descriptionFont = new SFontFamily(_fontFamily, value * DescriptionFontSizeMultiple);
                _bitSize = null; // Reset bit size so it will be recalculated on next render
            }
        }

        public float Value { get; set; }

        public SingleBinaryViewElement(string atriaIdString, string fontFamily)
        {
            Id = AtriaId.Parse(atriaIdString);
            _fontFamily = fontFamily;
            _font = new SFontFamily(fontFamily, _baseFontSize);
            _descriptionFont = new SFontFamily(fontFamily, _baseFontSize * DescriptionFontSizeMultiple);
        }

        public override void Render(IRenderTarget target)
        {
            const int hexSectionLength = 3 + (4 * 3); // " = " + 4 bytes * 3 characters each (2 hex digits + space)
            _bitSize ??= target.MeasureText("0", _font);

            var signSize = _bitSize.Value;
            var exponentSize = new SSizeF(_bitSize.Value.Width * 8f, _bitSize.Value.Height);
            var mantissaSize = new SSizeF(_bitSize.Value.Width * 23f, _bitSize.Value.Height);
            var hexSize = new SSizeF(_bitSize.Value.Width * hexSectionLength, _bitSize.Value.Height);
            var marginSize = new SSizeF(_bitSize.Value.Width * SegmentMarginMultipleOfBitWidth, _bitSize.Value.Height);
            var fullSize = new SSizeF(signSize.Width
                + marginSize.Width
                + exponentSize.Width
                + marginSize.Width
                + mantissaSize.Width
                + hexSize.Width, _bitSize.Value.Height); // no margin after mantissa because it looks too wide
            var fullBounds = AlignmentHelper.Align(Alignment.Center, Bounds, fullSize).WithSize(fullSize);

            var signBounds = new SRectF(fullBounds.X, fullBounds.Y, signSize.Width, signSize.Height);
            var exponentBounds = new SRectF(signBounds.Right + marginSize.Width,
                fullBounds.Y,
                exponentSize.Width,
                exponentSize.Height);
            var mantissaBounds = new SRectF(exponentBounds.Right + marginSize.Width,
                fullBounds.Y,
                mantissaSize.Width,
                mantissaSize.Height);
            var hexBounds = new SRectF(mantissaBounds.Right,
                fullBounds.Y,
                hexSize.Width,
                hexSize.Height);


            // Compute background positioning
            var backgroundMargin = _bitSize.Value.Width * SectionBackgroundMarginMultipleOfBitWidth;
            var backgroundLeft = signBounds.Left - backgroundMargin;
            var backgroundRight = mantissaBounds.Right + backgroundMargin;
            var backgroundTop = fullBounds.Top - backgroundMargin;
            var backgroundBottom = fullBounds.Bottom + backgroundMargin;
            var signBackgroundLeft = backgroundLeft;
            var signBackgroundRight = new BasisLine(signBounds.CenterRight, exponentBounds.CenterLeft).Center.X;
            var exponentBackgroundLeft = signBackgroundRight;
            var exponentBackgroundRight = new BasisLine(exponentBounds.CenterRight, mantissaBounds.CenterLeft).Center.X;
            var mantissaBackgroundLeft = exponentBackgroundRight;
            var mantissaBackgroundRight = backgroundRight;
            var backgroundHeight = backgroundBottom - backgroundTop;
            var signBackgroundBounds = new SRectF(signBackgroundLeft, backgroundTop, signBackgroundRight - signBackgroundLeft, backgroundHeight);
            var exponentBackgroundBounds = new SRectF(exponentBackgroundLeft, backgroundTop, exponentBackgroundRight - exponentBackgroundLeft, backgroundHeight);
            var mantissaBackgroundBounds = new SRectF(mantissaBackgroundLeft, backgroundTop, mantissaBackgroundRight - mantissaBackgroundLeft, backgroundHeight);

            // Compute description text positioning
            var descriptionHeight = target.MeasureText(SignDescription, _descriptionFont).Height;
            var topDescriptionY = signBackgroundBounds.Top - descriptionHeight - backgroundMargin;
            var bottomDescriptionY = signBackgroundBounds.Bottom + descriptionHeight + backgroundMargin;
            var signDescriptionBounds = new SRectF(signBackgroundBounds.Left, topDescriptionY, signBackgroundBounds.Width, descriptionHeight);
            var exponentDescriptionBounds = new SRectF(exponentBackgroundBounds.Left, topDescriptionY, exponentBackgroundBounds.Width, descriptionHeight);
            var mantissaDescriptionBounds = new SRectF(mantissaBackgroundBounds.Left, topDescriptionY, mantissaBackgroundBounds.Width, descriptionHeight);
            var signValueBounds = new SRectF(signBackgroundBounds.Left, bottomDescriptionY, signBackgroundBounds.Width, descriptionHeight);
            var exponentValueBounds = new SRectF(exponentBackgroundBounds.Left, bottomDescriptionY, exponentBackgroundBounds.Width, descriptionHeight);
            var mantissaValueBounds = new SRectF(mantissaBackgroundBounds.Left, bottomDescriptionY, mantissaBackgroundBounds.Width, descriptionHeight);

            target.DrawRectangle(signBackgroundBounds, SignColor.WithOpacity(SectionBackgroundOpacity), SPaintStyle.Fill, SAngle.Zero);
            target.DrawRectangle(exponentBackgroundBounds, ExponentColor.WithOpacity(SectionBackgroundOpacity), SPaintStyle.Fill, SAngle.Zero);
            target.DrawRectangle(mantissaBackgroundBounds, MantissaColor.WithOpacity(SectionBackgroundOpacity), SPaintStyle.Fill, SAngle.Zero);

            target.DrawTextDirectly(SignBit, _font, signBounds, SColor.White, SAngle.Zero);
            target.DrawTextDirectly(ExponentBits, _font, exponentBounds, SColor.White, SAngle.Zero);
            target.DrawTextDirectly(MantissaBits, _font, mantissaBounds, SColor.White, SAngle.Zero);
            target.DrawTextDirectly($" = {AsHex}", _font, hexBounds, SColor.White, SAngle.Zero);

            target.DrawText(SignDescription, _descriptionFont, signDescriptionBounds, SignColor, SAngle.Zero, Alignment.RightCenter);
            target.DrawText(ExponentDescription, _descriptionFont, exponentDescriptionBounds, ExponentColor, SAngle.Zero, Alignment.RightCenter);
            target.DrawText(MantissaDescription, _descriptionFont, mantissaDescriptionBounds, MantissaColor, SAngle.Zero, Alignment.RightCenter);
            
            target.DrawText(SignValueDescription, _descriptionFont, signValueBounds, SignColor, SAngle.Zero, Alignment.RightCenter);
            target.DrawText(ExponentValueDescription, _descriptionFont, exponentValueBounds, ExponentColor, SAngle.Zero, Alignment.RightCenter);
            target.DrawText(MantissaValueDescription, _descriptionFont, mantissaValueBounds, MantissaColor, SAngle.Zero, Alignment.RightCenter);
        }
    }
}
