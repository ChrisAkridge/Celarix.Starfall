using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class FloatingPointFormatDesignElement : AtriaElement
    {
        private static readonly SColor SignColor = new(255, 92, 92, 255);
        private static readonly SColor MantissaColor = new(80, 220, 255, 255);
        private static readonly SColor BaseColor = new(255, 190, 70, 255);
        private static readonly SColor ExponentColor = new(120, 235, 150, 255);
        private static readonly SColor SelectedColor = new(255, 220, 90, 255);
        private static readonly SColor DimColor = new(150, 150, 180, 255);

        public double ScientificPartsOpacity { get; set; }
        public double BaseRemovalProgress { get; set; }
        public double SignBitProgress { get; set; }
        public double SignConventionOpacity { get; set; }
        public double MemoryOptionsOpacity { get; set; }
        public double EightBitOpacity { get; set; }
        public double SixteenBitOpacity { get; set; }
        public double ThirtyTwoBitOpacity { get; set; }
        public double SixtyFourBitOpacity { get; set; }
        public double OneHundredTwentyEightBitOpacity { get; set; }
        public double FormatNamesOpacity { get; set; }
        public double SingleLayoutOpacity { get; set; }

        private double Scale => Math.Min(Bounds.Width / 1280d, Bounds.Height / 720d);

        public FloatingPointFormatDesignElement(string atriaId)
        {
            Id = AtriaId.Parse(atriaId);
            Opacity = 1d;
        }

        public override void Render(IRenderTarget target)
        {
            if (ScientificPartsOpacity > 0d)
            {
                DrawScientificParts(target);
            }

            if (MemoryOptionsOpacity > 0d)
            {
                DrawMemoryOptions(target);
            }

            if (SingleLayoutOpacity > 0d)
            {
                DrawSingleLayout(target);
            }
        }

        private void DrawScientificParts(IRenderTarget target)
        {
            var opacity = ScientificPartsOpacity * Opacity;
            var initialCenters = MathHelpers.EquallySpaceCenteredPoints(Bounds.Left, Bounds.Right, 4);
            var finalCenters = MathHelpers.EquallySpaceCenteredPoints(Bounds.Left, Bounds.Right, 3);
            var signCenter = Lerp(initialCenters[0], finalCenters[0], BaseRemovalProgress);
            var mantissaCenter = Lerp(initialCenters[1], finalCenters[1], BaseRemovalProgress);
            var exponentCenter = Lerp(initialCenters[3], finalCenters[2], BaseRemovalProgress);
            var partCenterY = Bounds.Top + Bounds.Height / 4d;

            DrawPart(target, "Sign", "+", new SPointF(signCenter, partCenterY), SignColor, opacity, isSign: true);
            DrawPart(target, "Mantissa", "1.101", new SPointF(mantissaCenter, partCenterY), MantissaColor, opacity);
            DrawPart(target, "Base", "2", new SPointF(initialCenters[2], partCenterY), BaseColor,
                opacity * (1d - BaseRemovalProgress));
            DrawPart(target, "Exponent", "5", new SPointF(exponentCenter, partCenterY), ExponentColor, opacity);

            if (SignBitProgress > 0d)
            {
                var bitOpacity = opacity * SignBitProgress;
                var boxSize = 64d * Scale;
                var gap = 12d * Scale;
                var left = signCenter - boxSize - gap / 2d;
                var top = partCenterY - boxSize / 2d;
                DrawBitCell(target, new SRectF(left, top, boxSize, boxSize), "0", SignColor, bitOpacity);
                DrawBitCell(target, new SRectF(left + boxSize + gap, top, boxSize, boxSize), "1", SignColor, bitOpacity);
                target.DrawText("1 BIT", Font(24f), new SRectF(signCenter - 90d * Scale,
                        top + boxSize + 12d * Scale, 180d * Scale, 38d * Scale),
                    SignColor.WithOpacity(bitOpacity), SAngle.Zero);
            }

            if (SignConventionOpacity > 0d)
            {
                var conventionOpacity = opacity * SignConventionOpacity;
                var conventionCenters = MathHelpers.EquallySpaceCenteredPoints(
                    signCenter - 150d * Scale, signCenter + 150d * Scale, 2);
                var conventionY = partCenterY + 150d * Scale;
                DrawCenteredText(target, "0 = +", Font(34f), conventionCenters[0], conventionY,
                    130d * Scale, 48d * Scale, SignColor.WithOpacity(conventionOpacity));
                DrawCenteredText(target, "1 = −", Font(34f), conventionCenters[1], conventionY,
                    130d * Scale, 48d * Scale, SignColor.WithOpacity(conventionOpacity));
            }
        }

        private void DrawPart(IRenderTarget target, string label, string value, SPointF center, SColor color,
            double opacity, bool isSign = false)
        {
            if (opacity <= 0d) { return; }

            DrawCenteredText(target, label, Font(25f), center.X, center.Y - 70d * Scale,
                240d * Scale, 42d * Scale, color.WithOpacity(opacity));

            var valueOpacity = isSign ? opacity * (1d - SignBitProgress) : opacity;
            DrawCenteredText(target, value, Font(72f), center.X, center.Y,
                280d * Scale, 100d * Scale, color.WithOpacity(valueOpacity));
        }

        private void DrawMemoryOptions(IRenderTarget target)
        {
            var opacity = MemoryOptionsOpacity * Opacity;
            var rowCenters = MathHelpers.EquallySpaceCenteredPoints(Bounds.Top, Bounds.Bottom, 5);
            DrawMemoryRow(target, 8, rowCenters[0], EightBitOpacity * opacity);
            DrawMemoryRow(target, 16, rowCenters[1], SixteenBitOpacity * opacity);
            DrawMemoryRow(target, 32, rowCenters[2], ThirtyTwoBitOpacity * opacity);
            DrawMemoryRow(target, 64, rowCenters[3], SixtyFourBitOpacity * opacity);
            DrawMemoryRow(target, 128, rowCenters[4], OneHundredTwentyEightBitOpacity * opacity);

            if (FormatNamesOpacity <= 0d) { return; }

            var namesOpacity = FormatNamesOpacity * opacity;
            DrawFormatName(target, "no format", rowCenters[0], DimColor, namesOpacity);
            DrawFormatName(target, "half", rowCenters[1], SColor.White, namesOpacity);
            DrawFormatName(target, "single", rowCenters[2], SelectedColor, namesOpacity);
            DrawFormatName(target, "double", rowCenters[3], SColor.White, namesOpacity);
            DrawFormatName(target, "quadruple", rowCenters[4], SColor.White, namesOpacity);
        }

        private void DrawMemoryRow(IRenderTarget target, int bitCount, double centerY, double opacity)
        {
            if (opacity <= 0d) { return; }

            var color = FormatNamesOpacity > 0d && bitCount == 32 ? SelectedColor : SColor.White;
            var labelCenterX = Bounds.Left + Bounds.Width / 10d;
            var bitsLeft = Bounds.Left + Bounds.Width / 6d;
            var bitsRight = Bounds.Left + Bounds.Width * 0.72d;

            DrawCenteredText(target, bitCount.ToString(), Font(bitCount == 128 ? 34f : 52f),
                labelCenterX, centerY, Bounds.Width / 8d, 76d * Scale, color.WithOpacity(opacity));
            var bitBounds = new SRectF(bitsLeft, centerY - 38d * Scale,
                bitsRight - bitsLeft, 76d * Scale);
            if (bitCount == 64)
            {
                DrawWrappedZeroBits(target, 32, bitBounds, color, opacity);
            }
            else if (bitCount == 128)
            {
                DrawWrappedZeroBits(target, 64, bitBounds, color, opacity);
            }
            else
            {
                DrawZeroBits(target, bitCount, bitBounds, color, opacity);
            }
        }

        private void DrawZeroBits(IRenderTarget target, int bitCount, SRectF bounds, SColor color,
            double opacity)
        {
            var byteCount = bitCount / 8;
            const double byteGapWeight = 0.65d;
            var totalBitWeights = 64d + (7d * byteGapWeight);
            var bitAdvance = bounds.Width / totalBitWeights;
            var fontSize = Math.Min(24d * Scale, bitAdvance * 2.1d);
            var x = bounds.Left;

            for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                target.DrawText("0", Font((float)fontSize), new SRectF(x, bounds.Top, bitAdvance, bounds.Height),
                    color.WithOpacity(opacity), SAngle.Zero);
                x += bitAdvance;
                if ((bitIndex + 1) % 8 == 0 && bitIndex + 1 < bitCount)
                {
                    x += bitAdvance * byteGapWeight;
                }
            }
        }

        private void DrawWrappedZeroBits(IRenderTarget target, int bitsPerLine, SRectF bounds,
            SColor color, double opacity)
        {
            var lineGap = 4d * Scale;
            var lineHeight = (bounds.Height - lineGap) / 2d;
            var firstLine = new SRectF(bounds.Left, bounds.Top, bounds.Width, lineHeight);
            var secondLine = new SRectF(bounds.Left, firstLine.Bottom + lineGap, bounds.Width, lineHeight);
            DrawZeroBits(target, bitsPerLine, firstLine, color, opacity);
            DrawZeroBits(target, bitsPerLine, secondLine, color, opacity);
        }

        private void DrawFormatName(IRenderTarget target, string name, double centerY, SColor color, double opacity)
        {
            var left = Bounds.Left + Bounds.Width * 0.76d;
            target.DrawText(name, Font(34f), new SRectF(left, centerY - 28d * Scale,
                    Bounds.Right - left, 56d * Scale), color.WithOpacity(opacity), SAngle.Zero,
                Alignment.LeftCenter);
        }

        private void DrawSingleLayout(IRenderTarget target)
        {
            var opacity = SingleLayoutOpacity * Opacity;
            var bitRowLeft = Bounds.Left + Bounds.Width / 12d;
            var bitRowRight = Bounds.Right - Bounds.Width / 12d;
            var bitRowWidth = bitRowRight - bitRowLeft;
            var byteGapWeight = 0.8d;
            var bitAdvance = bitRowWidth / (32d + 3d * byteGapWeight);
            var bitFontSize = Math.Min(48d * Scale, bitAdvance * 1.7d);
            var rowCenterY = Bounds.Top + Bounds.Height / 2d;
            var rowHeight = 88d * Scale;
            var x = bitRowLeft;
            double signCenterX = 0d;

            for (var bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                var byteIndex = bitIndex / 8;

                var isSign = bitIndex == 0;
                var color = isSign ? SignColor : SColor.White;
                var bitBounds = new SRectF(x, rowCenterY - rowHeight / 2d, bitAdvance, rowHeight);
                target.DrawText("0", Font((float)bitFontSize), bitBounds, color.WithOpacity(opacity), SAngle.Zero);
                if (isSign) { signCenterX = bitBounds.Center.X; }

                x += bitAdvance;
                if ((bitIndex + 1) % 8 == 0 && bitIndex + 1 < 32)
                {
                    x += bitAdvance * byteGapWeight;
                }
            }

            DrawCenteredText(target, "+", Font(52f), signCenterX, rowCenterY - 100d * Scale,
                70d * Scale, 60d * Scale, SignColor.WithOpacity(opacity));
            target.DrawLine(new SPointF(signCenterX, rowCenterY - 62d * Scale),
                new SPointF(signCenterX, rowCenterY - 35d * Scale),
                SignColor.WithOpacity(opacity), (float)(4d * Scale));
        }

        private void DrawBitCell(IRenderTarget target, SRectF bounds, string text, SColor color, double opacity)
        {
            target.DrawRectangle(bounds, color.WithOpacity(opacity), SPaintStyle.Stroke, SAngle.Zero);
            target.DrawText(text, Font(52f), bounds, color.WithOpacity(opacity), SAngle.Zero);
        }

        private void DrawCenteredText(IRenderTarget target, string text, SFont font, double centerX,
            double centerY, double width, double height, SColor color)
        {
            target.DrawText(text, font, new SRectF(centerX - width / 2d, centerY - height / 2d, width, height),
                color, SAngle.Zero);
        }

        private SFont Font(float baseSize) => new SFontFamily("Consolas", (float)(baseSize * Scale));

        private static double Lerp(double from, double to, double progress) =>
            from + (to - from) * progress;
    }
}
