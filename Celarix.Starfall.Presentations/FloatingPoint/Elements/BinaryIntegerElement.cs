using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Color;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    /// <summary>
    /// Displays a fixed-width binary integer as a row of power/bit/place-value triplets.
    /// </summary>
    internal sealed class BinaryIntegerElement : AtriaElement
    {
        private static readonly SColor SetBitColor = SColor.White;
        private static readonly SColor UnsetBitColor = new(128, 128, 148, 255);

        private readonly int[] _bitPlaceExponents;
        private readonly double[] _labelRevealProgresses;
        private readonly ReadOnlyCollection<int> _readOnlyBitPlaceExponents;
        private readonly MeasurementService _measurementService;
        private BigInteger _bitPattern;

        public IReadOnlyList<int> BitPlaceExponents => _readOnlyBitPlaceExponents;

        /// <summary>
        /// Gets or sets the nonnegative bit pattern displayed by the element. The pattern
        /// must fit in the configured number of bits.
        /// </summary>
        public BigInteger BitPattern
        {
            get => _bitPattern;
            set
            {
                var maximumPattern = (BigInteger.One << _bitPlaceExponents.Length) - 1;
                if (value < BigInteger.Zero || value > maximumPattern)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"The bit pattern must be between 0 and {maximumPattern}.");
                }

                _bitPattern = value;
            }
        }

        /// <summary>
        /// Gets the numeric value represented by the current bit pattern and place values.
        /// </summary>
        public BigInteger NumericValue => CurrentSummands
            .Where(summand => summand.HasValue)
            .Aggregate(BigInteger.Zero, (sum, summand) => sum + summand!.Value);

        /// <summary>
        /// Gets one entry per displayed bit. Set bits contain their numeric place value;
        /// unset bits contain null.
        /// </summary>
        public IReadOnlyList<BigInteger?> CurrentSummands
        {
            get
            {
                var summands = new BigInteger?[_bitPlaceExponents.Length];
                for (var bitIndex = 0; bitIndex < summands.Length; bitIndex++)
                {
                    summands[bitIndex] = IsBitSet(bitIndex) ? GetPlaceValue(bitIndex) : null;
                }

                return Array.AsReadOnly(summands);
            }
        }

        private double Scale => Math.Min(Bounds.Width / 1280d, Bounds.Height / 720d);
        private SFont BitFont => Font(120f);

        public BinaryIntegerElement(string atriaId, IEnumerable<int> bitPlaceExponents,
            MeasurementService measurementService)
        {
            Id = AtriaId.Parse(atriaId);
            _bitPlaceExponents = bitPlaceExponents.ToArray();
            if (_bitPlaceExponents.Length == 0)
            {
                throw new ArgumentException("At least one bit place exponent is required.",
                    nameof(bitPlaceExponents));
            }

            _readOnlyBitPlaceExponents = Array.AsReadOnly(_bitPlaceExponents);
            _labelRevealProgresses = new double[_bitPlaceExponents.Length];
            _measurementService = measurementService;
            Opacity = 1d;
        }

        public static BinaryIntegerElement CreateUnsigned8(string atriaId, MeasurementService measurementService) => CreateUnsigned(atriaId, 8, measurementService);
        public static BinaryIntegerElement CreateUnsigned16(string atriaId, MeasurementService measurementService) => CreateUnsigned(atriaId, 16, measurementService);
        public static BinaryIntegerElement CreateUnsigned32(string atriaId, MeasurementService measurementService) => CreateUnsigned(atriaId, 32, measurementService);
        public static BinaryIntegerElement CreateUnsigned64(string atriaId, MeasurementService measurementService) => CreateUnsigned(atriaId, 64, measurementService);

        public static BinaryIntegerElement CreateTwosComplement8(string atriaId, MeasurementService measurementService) => CreateTwosComplement(atriaId, 8, measurementService);
        public static BinaryIntegerElement CreateTwosComplement16(string atriaId, MeasurementService measurementService) => CreateTwosComplement(atriaId, 16, measurementService);
        public static BinaryIntegerElement CreateTwosComplement32(string atriaId, MeasurementService measurementService) => CreateTwosComplement(atriaId, 32, measurementService);
        public static BinaryIntegerElement CreateTwosComplement64(string atriaId, MeasurementService measurementService) => CreateTwosComplement(atriaId, 64, measurementService);

        public static BinaryIntegerElement CreateUnsigned(string atriaId, int bitCount,
            MeasurementService measurementService) =>
            new(atriaId, DescendingPlaceExponents(bitCount, signed: false), measurementService);

        public static BinaryIntegerElement CreateTwosComplement(string atriaId, int bitCount,
            MeasurementService measurementService) =>
            new(atriaId, DescendingPlaceExponents(bitCount, signed: true), measurementService);

        public void SetLabelRevealProgress(int bitIndex, double progress)
        {
            if (bitIndex < 0 || bitIndex >= _labelRevealProgresses.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bitIndex));
            }

            _labelRevealProgresses[bitIndex] = Math.Clamp(progress, 0d, 1d);
        }

        /// <summary>
        /// Gets the numeric contribution made by a set bit at the specified displayed index.
        /// A negative exponent denotes a negative power-of-two place value, so [-7, 6, ..., 0]
        /// describes an eight-bit two's-complement integer.
        /// </summary>
        public BigInteger GetPlaceValue(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= _bitPlaceExponents.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bitIndex));
            }

            var placeExponent = _bitPlaceExponents[bitIndex];
            var magnitude = BigInteger.One << Math.Abs(placeExponent);
            return placeExponent < 0 ? -magnitude : magnitude;
        }

        public bool IsBitSet(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= _bitPlaceExponents.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bitIndex));
            }

            var shift = _bitPlaceExponents.Length - bitIndex - 1;
            return ((_bitPattern >> shift) & BigInteger.One) == BigInteger.One;
        }

        public override void Render(IRenderTarget target)
        {
            // Keep these proportions identical to FloatingPointWindowElement.RowBit.
            const double smallFontSizeFactor = 0.20d;
            const double bitSpacingRatioOfBitHeight = 0.10d;
            const double smallTextMarginRatioOfBitHeight = 0.05d;

            var bitTextSize = _measurementService.MeasureText("0", BitFont);
            var bitSpacing = bitTextSize.Height * bitSpacingRatioOfBitHeight;
            var bitAdvance = bitTextSize.Width + bitSpacing;
            var annotationHeight = bitTextSize.Height * smallFontSizeFactor;
            var tripletHeight = bitTextSize.Height + annotationHeight * 2d + bitSpacing * 2d;
            var tripletTop = Bounds.Center.Y - tripletHeight / 2d;
            var rowCenterX = Bounds.Left + Bounds.Width * 0.38d;
            var rowLeft = rowCenterX - bitAdvance * _bitPlaceExponents.Length / 2d;
            var bitTextTop = tripletTop + (tripletHeight - bitTextSize.Height) / 2d;
            var bitCenterY = bitTextTop + bitTextSize.Height / 2d;
            // FloatingPointWindowElement ascends from the centered bit text's top edge,
            // rather than from the top of the complete exponent/bit/place-value triplet.
            var exponentTargetTop = bitTextTop - annotationHeight;
            var exponentTargetCenterY = exponentTargetTop + annotationHeight / 2d;
            var placeValueTargetTop = tripletTop + bitTextSize.Height + annotationHeight
                + smallTextMarginRatioOfBitHeight * tripletHeight;
            var placeValueTargetCenterY = placeValueTargetTop + annotationHeight / 2d;
            var exponentFont = BitFont.WithSize((float)annotationHeight);
            var placeValueFont = BitFont.WithSize((float)(annotationHeight * 0.9d));

            for (var bitIndex = 0; bitIndex < _bitPlaceExponents.Length; bitIndex++)
            {
                var color = IsBitSet(bitIndex) ? SetBitColor : UnsetBitColor;
                var placeValue = GetPlaceValue(bitIndex);
                var exponentText = GetExponentText(_bitPlaceExponents[bitIndex]);
                var bitCenterX = rowLeft + bitAdvance * (bitIndex + 0.5d);
                var revealProgress = _labelRevealProgresses[bitIndex];
                var exponentCenterY = MathHelpers.Ease(bitCenterY, exponentTargetCenterY,
                    revealProgress, Easings.Linear);
                var placeValueCenterY = MathHelpers.Ease(bitCenterY, placeValueTargetCenterY,
                    revealProgress, Easings.Linear);
                var labelColor = color.WithOpacity(Opacity * revealProgress);

                DrawCenteredText(target, exponentText, exponentFont, bitCenterX,
                    exponentCenterY, bitAdvance, annotationHeight, labelColor);
                DrawCenteredText(target, IsBitSet(bitIndex) ? "1" : "0", BitFont,
                    bitCenterX, bitCenterY, bitAdvance, bitTextSize.Height,
                    color.WithOpacity(Opacity));
                DrawCenteredText(target, placeValue.ToString(), placeValueFont,
                    bitCenterX, placeValueCenterY, bitAdvance, annotationHeight, labelColor);
            }
        }

        private void DrawCenteredText(IRenderTarget target, string text, SFont font,
            double centerX, double centerY, double width, double height, SColor color)
        {
            target.DrawText(text, font,
                new SRectF(centerX - width / 2d, centerY - height / 2d, width, height),
                color, SAngle.Zero);
        }

        private SFont Font(float baseSize) =>
            new SFontFamily("Consolas", (float)(baseSize * Scale));

        private static string GetExponentText(int placeExponent)
        {
            var digits = Math.Abs(placeExponent).ToString();
            return $"{(placeExponent < 0 ? "−" : string.Empty)}2{string.Concat(digits.Select(FPHelpers.ToUnicodeSuperscript))}";
        }

        private static int[] DescendingPlaceExponents(int bitCount, bool signed)
        {
            if (bitCount < (signed ? 2 : 1))
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount,
                    signed
                        ? "A two's-complement integer requires at least two bits."
                        : "An unsigned integer requires at least one bit.");
            }

            var exponents = Enumerable.Range(0, bitCount)
                .Select(index => bitCount - index - 1)
                .ToArray();
            if (signed)
            {
                exponents[0] = -exponents[0];
            }

            return exponents;
        }
    }

    /// <summary>
    /// Renders the addition problem associated with a BinaryIntegerElement. Keeping this
    /// separate lets a slide fade the stack through the normal AtriaElement.Opacity property.
    /// </summary>
    internal sealed class BinaryIntegerSummandStackElement : AtriaElement
    {
        private readonly BinaryIntegerElement _binaryInteger;
        private readonly MeasurementService _measurementService;
        private double Scale => Math.Min(Bounds.Width / 1280d, Bounds.Height / 720d);
        private SFont AdditionFont => new SFontFamily("Consolas", (float)(40d * Scale));

        public BinaryIntegerSummandStackElement(string atriaId, BinaryIntegerElement binaryInteger,
            MeasurementService measurementService)
        {
            Id = AtriaId.Parse(atriaId);
            _binaryInteger = binaryInteger;
            _measurementService = measurementService;
        }

        public SRectF GetSumTextBounds(string text)
        {
            var layoutBounds = GetSumLayoutBounds();
            var textSize = _measurementService.MeasureText(text, AdditionFont);
            var alignedPosition = AlignmentHelper.Align(Alignment.RightCenter, layoutBounds, textSize);
            return new SRectF(alignedPosition, textSize);
        }

        public override void Render(IRenderTarget target)
        {
            var stackLeft = Bounds.Left + Bounds.Width * 0.79d;
            var stackRight = Bounds.Right - Bounds.Width * 0.045d;
            var stackTop = Bounds.Top + Bounds.Height * 0.09d;
            var resultHeight = 62d * Scale;
            var ruleGap = 8d * Scale;
            var availableHeight = Bounds.Height * 0.72d;
            var summands = _binaryInteger.CurrentSummands;
            var rowHeight = (availableHeight - resultHeight - ruleGap) / summands.Count;

            for (var bitIndex = 0; bitIndex < summands.Count; bitIndex++)
            {
                var summand = summands[bitIndex];
                if (!summand.HasValue) { continue; }

                var text = bitIndex == summands.Count - 1
                    ? $"+{summand.Value,3}"
                    : $"{summand.Value,4}";
                var rowBounds = new SRectF(stackLeft, stackTop + rowHeight * bitIndex,
                    stackRight - stackLeft, rowHeight);
                target.DrawText(text, AdditionFont, rowBounds,
                    SColor.White.WithOpacity(Opacity), SAngle.Zero, Alignment.RightCenter);
            }

            var ruleY = stackTop + rowHeight * summands.Count;
            target.DrawLine(new SPointF(stackLeft, ruleY), new SPointF(stackRight, ruleY),
                SColor.White.WithOpacity(Opacity), (float)(3d * Scale));
            target.DrawText(_binaryInteger.NumericValue.ToString(), AdditionFont, GetSumLayoutBounds(),
                SColor.White.WithOpacity(Opacity), SAngle.Zero, Alignment.RightCenter);
        }

        private SRectF GetSumLayoutBounds()
        {
            var stackLeft = Bounds.Left + Bounds.Width * 0.79d;
            var stackRight = Bounds.Right - Bounds.Width * 0.045d;
            var stackTop = Bounds.Top + Bounds.Height * 0.09d;
            var resultHeight = 62d * Scale;
            var ruleGap = 8d * Scale;
            var availableHeight = Bounds.Height * 0.72d;
            var rowHeight = (availableHeight - resultHeight - ruleGap)
                / _binaryInteger.BitPlaceExponents.Count;
            var ruleY = stackTop + rowHeight * _binaryInteger.BitPlaceExponents.Count;
            return new SRectF(stackLeft, ruleY + ruleGap,
                stackRight - stackLeft, resultHeight);
        }
    }

    /// <summary>
    /// Collects the values visited by a BinaryIntegerElement into a background grid.
    /// </summary>
    internal sealed class BinaryIntegerValueHistoryElement : AtriaElement
    {
        private sealed record ValueText
        {
            public required string Text { get; set; }
            public SRectF Bounds { get; set; }
            public SColor Color { get; set; }
        }

        private const double FontSizeToBoundsHeightRatio = 40d / 62d;
        private static readonly SColor BackgroundValueColor = new(105, 125, 165, 255);

        private readonly BinaryIntegerSummandStackElement _summandStack;
        private readonly List<ValueText> _values = [];
        private readonly GradientProvider _colorGradient = new(SColor.White, BackgroundValueColor);
        private double Scale => Math.Min(Bounds.Width / 1280d, Bounds.Height / 720d);

        public double EquationOpacity { get; private set; }

        public BinaryIntegerValueHistoryElement(string atriaId,
            BinaryIntegerSummandStackElement summandStack)
        {
            Id = AtriaId.Parse(atriaId);
            _summandStack = summandStack;
            Opacity = 1d;
        }

        public void Spawn(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            var text = value.ToString();
            var startBounds = _summandStack.GetSumTextBounds(text);
            var targetBounds = GetTargetBounds(value);
            var valueText = new ValueText
            {
                Text = text,
                Bounds = startBounds,
                Color = SColor.White
            };
            _values.Add(valueText);

            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(0.75d), p =>
                {
                    valueText.Bounds = MathHelpers.Ease(startBounds, targetBounds, p, Easings.Smoothstep);
                    valueText.Color = _colorGradient.Sample(Easings.Smoothstep(p));
                }));
        }

        public void PromoteAllValues(double durationSeconds)
        {
            AnimateValueColors(_ => SColor.White, durationSeconds);
        }

        public void ShowZeroAsExponent(double durationSeconds)
        {
            var zero = _values.Single(value => value.Text == "0");
            var zeroStartBounds = zero.Bounds;
            var zeroTargetBounds = GetZeroExponentBounds();
            var zeroColorGradient = new GradientProvider(zero.Color, SColor.Yellow);
            var otherValues = _values.Where(value => value != zero)
                .Select(value => (Value: value,
                    Gradient: new GradientProvider(value.Color, BackgroundValueColor)))
                .ToArray();

            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds), p =>
                {
                    var eased = Easings.Smoothstep(p);
                    zero.Bounds = MathHelpers.Ease(zeroStartBounds, zeroTargetBounds, p,
                        Easings.Smoothstep);
                    zero.Color = zeroColorGradient.Sample(eased);
                    foreach (var (value, gradient) in otherValues)
                    {
                        value.Color = gradient.Sample(eased);
                    }

                    EquationOpacity = Easings.Land(p);
                }));
        }

        public void RestoreGrid(double durationSeconds)
        {
            var zero = _values[0];
            var zeroStartBounds = zero.Bounds;
            var zeroTargetBounds = GetTargetBounds(0);
            var colorAnimations = _values
                .Select((value, storedValue) => (Value: value,
                    Gradient: new GradientProvider(value.Color,
                        storedValue == 0 ? SColor.Yellow : SColor.White)))
                .ToArray();

            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds), p =>
                {
                    var eased = Easings.Smoothstep(p);
                    zero.Bounds = MathHelpers.Ease(zeroStartBounds, zeroTargetBounds, p,
                        Easings.Smoothstep);
                    foreach (var (value, gradient) in colorAnimations)
                    {
                        value.Color = gradient.Sample(eased);
                    }

                    EquationOpacity = 1d - Easings.Land(p);
                }));
        }

        public void ApplyBias(int bias, double durationSeconds)
        {
            if (bias < 0 || bias > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(bias));
            }

            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds), p =>
                {
                    var currentBias = (int)Math.Round(Easings.Smoothstep(p) * bias);
                    for (var storedValue = 0; storedValue < _values.Count; storedValue++)
                    {
                        var displayedValue = storedValue - currentBias;
                        var value = _values[storedValue];
                        value.Text = FormatSignedValue(displayedValue);
                        value.Color = displayedValue == 0 ? SColor.Yellow : SColor.White;
                    }
                }));
        }

        public void HighlightSpecialExponents(double durationSeconds)
        {
            var lowGradient = new GradientProvider(_values[0].Color, SColor.Red);
            var highGradient = new GradientProvider(_values[255].Color, SColor.Red);
            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds), p =>
                {
                    var eased = Easings.Smoothstep(p);
                    _values[0].Color = lowGradient.Sample(eased);
                    _values[255].Color = highGradient.Sample(eased);
                }));
        }

        public override void Render(IRenderTarget target)
        {
            foreach (var value in _values)
            {
                var fontSize = (float)(value.Bounds.Height * FontSizeToBoundsHeightRatio);
                target.DrawText(value.Text, new SFontFamily("Consolas", fontSize), value.Bounds,
                    value.Color.WithOpacity(Opacity), SAngle.Zero);
            }

            if (EquationOpacity > 0d)
            {
                DrawEquation(target);
            }
        }

        private void AnimateValueColors(Func<ValueText, SColor> targetColor,
            double durationSeconds)
        {
            var animations = _values
                .Select(value => (Value: value,
                    Gradient: new GradientProvider(value.Color, targetColor(value))))
                .ToArray();

            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds), p =>
                {
                    var eased = Easings.Smoothstep(p);
                    foreach (var (value, gradient) in animations)
                    {
                        value.Color = gradient.Sample(eased);
                    }
                }));
        }

        private void DrawEquation(IRenderTarget target)
        {
            var baseBounds = GetEquationBaseBounds();
            var resultBounds = new SRectF(
                baseBounds.Right + 48d * Scale,
                Bounds.Center.Y - 65d * Scale,
                220d * Scale,
                130d * Scale);
            var color = SColor.White.WithOpacity(EquationOpacity * Opacity);
            target.DrawText("2", new SFontFamily("Consolas", (float)(82d * Scale)),
                baseBounds, color, SAngle.Zero);
            target.DrawText("= 1", new SFontFamily("Consolas", (float)(82d * Scale)),
                resultBounds, color, SAngle.Zero, Alignment.LeftCenter);
        }

        private SRectF GetEquationBaseBounds() => new(
            Bounds.Center.X - 105d * Scale,
            Bounds.Center.Y - 70d * Scale,
            100d * Scale,
            140d * Scale);

        private SRectF GetZeroExponentBounds()
        {
            var baseBounds = GetEquationBaseBounds();
            // The history text's font scales with its bounds height. A 76-pixel-tall
            // box produces a superscript around 60% of the 82-pixel base/result font.
            var exponentSize = new SSizeF(54d * Scale, 76d * Scale);
            return exponentSize.CenterAt(new SPointF(
                baseBounds.Right + 8d * Scale,
                baseBounds.Top + 17d * Scale));
        }

        private SRectF GetTargetBounds(int value)
        {
            const int gridSize = 16;
            var marginX = 30d * Scale;
            var marginY = 24d * Scale;
            var xCenters = MathHelpers.EquallySpaceCenteredPoints(
                Bounds.Left + marginX, Bounds.Right - marginX, gridSize);
            var yCenters = MathHelpers.EquallySpaceCenteredPoints(
                Bounds.Top + marginY, Bounds.Bottom - marginY, gridSize);
            var column = value % gridSize;
            var row = value / gridSize;
            var targetSize = new SSizeF(44d * Scale, 24d * Scale);
            return targetSize.CenterAt(new SPointF(xCenters[column], yCenters[row]));
        }

        private static string FormatSignedValue(int value) => value switch
        {
            > 0 => $"+{value}",
            _ => value.ToString()
        };
    }
}
