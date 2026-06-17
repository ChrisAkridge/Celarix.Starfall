using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Color;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using static OpenTK.Graphics.OpenGL.GL;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class FloatingPointWindowElement : AtriaElement
    {
        private struct RowBit
        {
            // Single-precision floating point numbers have a 24-bit mantissa where the leading bit is implicit
            // (either 1 for normalized numbers or 0 for subnormals). The maximum exponent is 2^127 and the minimum
            // is 2^-126. When the exponent is -126, we can have up to 23 more bits below it, leading to a smallest
            // possible bit of -149. The complete range is from -149 to +127, which is 277 bits total.
            public const int TotalSinglePrecisionBits = 277;
            public const double SmallFontSizeFactor = 0.25d;
            private const double BitSpacingRatioOfBitHeight = 0.1d;
            public const double SmallTextMarginRatioOfBitHeight = 0.05d;
            
            private static readonly string[] PlaceValueSuffixes = ["K", "M", "B", "T", "P", "E", "Z", "Y", "R", "Q", "KQ", "MQ", "GQ"];
            public static readonly SColor PrimaryTextColor = SColor.White;
            public static readonly SColor SecondaryTextColor = new SColor(0xBF, 0xBF, 0xBF, 255);

            public static SSizeF _bitTextSize;

            // Bit properties
            public bool BitSet { get; set; }
            public int BounceHeight { get; set; }
            public string BitText => BitSet ? "1" : "0";

            // Exponent properties (on top)
            public int Exponent { get; set; }
            public bool ShowExponent => ExponentOpacity != 0d;
            public double ExponentOpacity { get; set; }
            public double ExponentAscentProgress { get; set; }
            public string ExponentText => $"2{string.Concat(Exponent.ToString().Select(c => ToUnicodeSuperscript(c)))}";

            // Place value properties (on bottom)
            public bool ShowPlaceValue => PlaceValueOpacity != 0d;
            public double PlaceValueOpacity { get; set; }
            public double PlaceValueDescentProgress { get; set; }
            public string PlaceValueText
            {
                get
                {
                    var isFraction = Exponent < 0;
                    var absExponent = Math.Abs(Exponent);
                    var exponentValue = BigInteger.Pow(2, absExponent);
                    var log10 = BigInteger.Log10(exponentValue);

                    if (Exponent >= -9 && Exponent <= 13)
                    {
                        // Display between 2^-9 and 2^13 as normal numbers (1/512 to 8192)
                        return isFraction ? $"1/{exponentValue}" : exponentValue.ToString();
                    }

                    var log1000 = log10 - 3;
                    var suffixIndex = (int)(log1000 / 3);
                    if (suffixIndex < PlaceValueSuffixes.Length)
                    {
                        var suffix = PlaceValueSuffixes[suffixIndex];
                        var scaledValue = exponentValue / BigInteger.Pow(1000, suffixIndex + 1);
                        return isFraction ? $"1/{scaledValue}{suffix}" : $"{scaledValue}{suffix}";
                    }
                    else
                    {
                        // For extremely large/small numbers, just show the exponent form
                        return ExponentText;
                    }
                }
            }

            // Computed properties
            public static double BinaryPointWidth => _bitTextSize.Width;

            public static SSizeF BitSize
            {
                get
                {
                    var actualBitSpacing = _bitTextSize.Height * BitSpacingRatioOfBitHeight;

                    // For the width, take the bit text width and add spacing on both sides, specifically
                    // half the spacing ratio on each side. This still adds up to the full spacing ratio.
                    var bitWidth = _bitTextSize.Width + actualBitSpacing;

                    // For the height, even if we aren't drawing the exponent or place value text,
                    // reserve size for it. Let's hope that just multiplying the bit height by the
                    // small font size factor is enough to accommodate the text height.
                    var smallTextHeight = _bitTextSize.Height * SmallFontSizeFactor;
                    var bitHeight = _bitTextSize.Height + (smallTextHeight * 2) + (actualBitSpacing * 2);
                    return new SSizeF(bitWidth, bitHeight);
                }
            }

            public SSizeF Size
            {
                get
                {
                    var bitSize = BitSize;
                    return new SSizeF(bitSize.Width * TotalSinglePrecisionBits, bitSize.Height);
                }
            }

            public SPointF Position
            {
                get
                {
                    // The window has a binary point '.' which is half the width of a bit.
                    // This binary point is defined such that its center is at X = 0. 127 bits to the
                    // left of it and 149 bits to the right of it.
                    //var direction = Exponent >= 0 ? -1 : 1;
                    //var facingBinaryPointEdgeX = (BinaryPointWidth / 2d) * direction;
                    //var fullBitSize = BitSize;

                    //double bitLeftX;
                    //if (Exponent >= 0)
                    //{
                    //    // We're to the left of the binary point. The left edge of the binary point
                    //    // faces this bit, and the higher the exponent, the lower the X coordinate.
                    //    var fullBitsWidth = fullBitSize.Width * -Exponent;
                    //    bitLeftX = fullBitsWidth + facingBinaryPointEdgeX;

                    //    // We also need to include the width of the bit itself. Why?
                    //    // Let's say Exponent == 3. That's -90 pixels, right? Not quite. It's actually -120
                    //    // to account for the 2^0 bit in addition to the other three bits.
                    //    bitLeftX -= fullBitSize.Width;
                    //}
                    //else
                    //{
                    //    // We're to the right of the binary point. The right edge of the binary point faces
                    //    // this bit, and the lower the exponent, the higher the X coordinate.
                    //    var fullBitsWidth = fullBitSize.Width * -Exponent;
                    //    // yes we're inverting the exponent in both cases
                    //    // it's fine - here, the exponent is negative, we need it positive since we're
                    //    // going right (positive X)
                    //    bitLeftX = facingBinaryPointEdgeX + fullBitsWidth;
                    //}
                    //return new SPointF(bitLeftX, 0);
                    var facingCenterX = -Exponent * BitSize.Width;
                    facingCenterX += (BinaryPointWidth / 2d) * (Exponent >= 0 ? -1 : 1);

                    double bitLeftX;
                    if (Exponent >= 0)
                    {
                        // Positive exponent bits go to the left, so we need to add another bit width
                        // to get the left edge.
                        bitLeftX = facingCenterX - BitSize.Width;
                    }
                    else
                    {
                        // Negative exponent bits go to the right, so the side facing the center is the left edge.
                        bitLeftX = facingCenterX;
                    }

                    return new SPointF(bitLeftX, 0);
                }
            }

            public static int? GetBitForXPosition(double xPosition)
            {
                if (Math.Abs(xPosition) < (BinaryPointWidth / 2d))
                {
                    return null; // This is the binary point area, not a bit
                }

                if (xPosition < 0)
                {
                    // Positive exponent and a bit to the left of the binary point
                    double xLeftAligned = (-xPosition - (BinaryPointWidth / 2d));
                    int exponent = (int)(xLeftAligned / BitSize.Width);

                    // Points at exact multiples of BitSize.Width need to have 1 subtracted from them.
                    if (xLeftAligned % BitSize.Width == 0)
                    {
                        exponent -= 1;
                    }

                    return exponent;
                }
                // Technically x == 0 was handled above but C# doesn't realize that, so we need to explicitly allow it in the else if condition
                else if (xPosition >= 0)
                {
                    // Negative exponent and a bit to the right of the binary point
                    return (int)((xPosition - (BinaryPointWidth / 2d)) / BitSize.Width) * -1;
                }

                throw new ArgumentException($"Unreachable: Invalid xPosition: {xPosition}");
            }

            private static char ToUnicodeSuperscript(char digit)
            {
                return digit switch
                {
                    '0' => '⁰',
                    '1' => '¹',
                    '2' => '²',
                    '3' => '³',
                    '4' => '⁴',
                    '5' => '⁵',
                    '6' => '⁶',
                    '7' => '⁷',
                    '8' => '⁸',
                    '9' => '⁹',
                    '-' => '⁻',
                    _ => throw new ArgumentException($"Invalid digit character: {digit}")
                };
            }
        }

        private const double WindowOuterMarginRatioOfBitHeight = 0.01d;
        private const double GravitationalAccelerationPxPerFrameSquared = 0.5d;
        private const double AngularAccelerationPerFrameSquared = 0.01d;
        private const double MaxFallingWindowVelocityPerFrame = 20d;
        private const double MaxFallingWindowAngularVelocityPerFrame = 0.5d;

        private static readonly SColor WindowDefaultColor = new SColor(255, 255, 0, 127);
        private static readonly SColor WindowNaNColor = new SColor(255, 0, 0, 127);
        private static readonly SColor ArrowColor = SColor.White;
        private static readonly SColor NegativeFlagTextColor = SColor.Red;

        private SFont _bitFont = new SFontFamily("Consolas", 72f);
        private GradientProvider _windowBitGradientProvider = new GradientProvider(RowBit.SecondaryTextColor, RowBit.PrimaryTextColor);
        private readonly RowBit[] _bits = new RowBit[RowBit.TotalSinglePrecisionBits];

        private AnimationContext _animationContext = new();
        private bool _showingExponents = false;
        private bool _showingPlaceValues = false;
        private bool _isJitteringWindow;
        private int _framesUntilJitterToggle;
        private Random _random = new();
        private readonly SSizeF _negativeTextSize;

        // Row properties
        private SSizeF RowSize
        {
            get
            {
                var bitSize = RowBit.BitSize;
                return new SSizeF(bitSize.Width * RowBit.TotalSinglePrecisionBits, bitSize.Height);
            }
        }

        // Arrow properties
        public double ArrowOpacity { get; set; }
        public double ArrowCenterX { get; set; }

        // Window properties
        public SColor WindowColor { get; set; } = WindowDefaultColor;
        public int WindowWidthInBits { get; set; }

        public SSizeF WindowSize
        {
            get
            {
                // We'll add the margin in rendering later
                var width = (WindowWidthInBits * RowBit.BitSize.Width);
                var height = RowBit.BitSize.Height;

                var windowLeftBit = RowBit.GetBitForXPosition(WindowLeftX);
                if (windowLeftBit.HasValue && windowLeftBit.Value > -23 && windowLeftBit.Value < 1)
                {
                    // The window contains the binary point, so we need to add extra width to accommodate it.
                    width += RowBit.BinaryPointWidth;
                }

                return new SSizeF(width, height);
            }
        }
        public double WindowLeftX { get; set; }
        public double WindowOpacity { get; set; }
        public bool DoWindowJitter { get; set; }

        // Scrolling properties

        /// <summary>
        /// Gets or sets the X coordinate of the bit row that is being drawn at the center of this element.
        /// </summary>
        public double CenteredX { get; set; }

        // Negative flag properties
        public bool ShowNegativeFlag { get; set; }

        // Falling window properties
        public bool ShowFallingWindowRect { get; set; }
        public SPointF FallingWindowRectCenter { get; set; }
        public SAngle FallingWindowAngle { get; set; }
        public SSizeF FallingWindowRectSize { get; set; }
        public SColor FallingWindowColor { get; set; }
        public SAngle FallingWindowAngularVelocityPerFrame { get; set; }
        public SPointF FallingWindowCenterVelocityPerFrame { get; set; }

        public FloatingPointWindowElement(string atriaIdString, MeasurementService measurementService)
        {
            Id = AtriaId.Parse(atriaIdString);

            // Initialize the bits
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var exponent = 127 - i;
                _bits[i].Exponent = exponent;
            }

            // Pre-measure the bit text size since it's constant and we'll need it for layout
            RowBit._bitTextSize = measurementService.MeasureText("0", _bitFont);
            _negativeTextSize = measurementService.MeasureText("Negative!", _bitFont.WithSize((float)(RowBit._bitTextSize.Height * RowBit.SmallFontSizeFactor)));

            // These should probably be initialized externally?
            WindowWidthInBits = 24;
            WindowOpacity = 1d;
            CenteredX = 0;
            WindowLeftX = _bits[127].Position.X;
        }

        public override void Render(IRenderTarget target)
        {
            var elementRenderRect = GetRowRenderingRectOnElement();
            var yOffset = elementRenderRect.Y;

            var leftmostVisibleBit = RowBit.GetBitForXPosition(CenteredX - (elementRenderRect.Width / 2d));
            var rightmostVisibleBit = RowBit.GetBitForXPosition(CenteredX + (elementRenderRect.Width / 2d));
            var windowLeftBit = RowBit.GetBitForXPosition(WindowLeftX);
            var windowRightBit = windowLeftBit - WindowWidthInBits + 1;

            var bitOutOfWindowColor = RowBit.SecondaryTextColor;
            var bitInWindowColor = _windowBitGradientProvider.Sample(WindowOpacity);
            
            // 1. Draw the window first, if visible, so it's under the bits
            var windowRect = new SRectF(RowXToScreenX(WindowLeftX), 0, WindowSize.Width, WindowSize.Height);
            if (SpanVisible(windowRect.Left, windowRect.Right))
            {
                var xJitter = _isJitteringWindow ? _random.Next(-2, 3) : 0;
                var yJitter = _isJitteringWindow ? _random.Next(-2, 3) : 0;
                windowRect = windowRect.Move(xJitter, yJitter);
                var windowMargin = RowBit.BitSize.Height * WindowOuterMarginRatioOfBitHeight;
                SRectF finalWindowRect = windowRect
                    .Expand(windowMargin * 2, windowMargin * 2)
                    .Move(0, yOffset);
                target.DrawRectangle(finalWindowRect, WindowColor.WithOpacity(WindowOpacity), SPaintStyle.Fill, SAngle.Zero);
            }

            // 2. Draw the bits in the element (and remember the binary point if it's visible)
            var binaryPointHalfWidth = RowBit.BinaryPointWidth / 2d;
            var binaryPointRect = new SRectF(RowXToScreenX(-binaryPointHalfWidth), 0d, RowBit.BinaryPointWidth, RowBit.BitSize.Height);
            if (SpanVisible(binaryPointRect.Left, binaryPointRect.Right))
            {
                var inWindow = WindowSize.Width % 1 != 0;
                target.DrawText(".", _bitFont, binaryPointRect.Move(0, yOffset), inWindow ? RowBit.PrimaryTextColor : RowBit.SecondaryTextColor, SAngle.Zero, Alignment.Center);
            }

            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var bit = _bits[i];
                var exponent = 127 - i;
                var bitLeft = bit.Position.X;

                var bitRect = new SRectF(RowXToScreenX(bitLeft), bit.Position.Y, RowBit.BitSize.Width, RowBit.BitSize.Height);
                if (!SpanVisible(bitRect.Left, bitRect.Right))
                {
                    continue; // Skip bits that aren't visible in the element
                }

                var bitTextHeight = RowBit._bitTextSize.Height;
                var aligned = AlignmentHelper.Align(Alignment.Center, bitRect, new SSizeF(bitRect.Width, bitTextHeight));
                var bitTextRect = new SRectF(aligned.X, aligned.Y, bitRect.Width, bitTextHeight);
                
                // 3. Draw the exponents if visible, under the bit
                if (bit.ShowExponent)
                {
                    var exponentTextSize = RowBit._bitTextSize * RowBit.SmallFontSizeFactor;
                    var exponentAscentBase = RowBit.BitSize.Height / 4d;
                    //var exponentAscentTop = -(exponentTextSize.Height + RowBit.SmallTextMarginRatioOfBitHeight * RowBit.BitSize.Height);
                    var exponentAscentTop = -exponentTextSize.Height;
                    var exponentActualAscent = MathHelpers.Ease(exponentAscentBase, exponentAscentTop, bit.ExponentAscentProgress, Easings.Linear);
                    var exponentRect = new SRectF(bitRect.X, bitTextRect.Y + exponentActualAscent, bitRect.Width, exponentTextSize.Height);
                    target.DrawText(bit.ExponentText,
                        _bitFont.WithSize((float)exponentTextSize.Height),
                        exponentRect.Move(0, yOffset),
                        RowBit.PrimaryTextColor.WithOpacity(bit.ExponentOpacity),
                        SAngle.Zero,
                        Alignment.Center);
                }

                // 4. Draw the place values if visible, under the bit
                if (bit.ShowPlaceValue)
                {
                    var placeValueTextSize = RowBit._bitTextSize * RowBit.SmallFontSizeFactor;
                    var placeValueDescentBase = RowBit.BitSize.Height / 4d;
                    var placeValueDescentBottom = RowBit._bitTextSize.Height + placeValueTextSize.Height + (RowBit.SmallTextMarginRatioOfBitHeight * RowBit.BitSize.Height);
                    var placeValueActualDescent = MathHelpers.Ease(placeValueDescentBase, placeValueDescentBottom, bit.PlaceValueDescentProgress, Easings.Linear);
                    var placeValueRect = new SRectF(bitRect.X, placeValueActualDescent, bitRect.Width, placeValueTextSize.Height);
                    target.DrawText(bit.PlaceValueText,
                        _bitFont.WithSize((float)placeValueTextSize.Height * 0.9f), // CANIMPROVE: This 0.9 is a band-aid for the fact that the place value text is too big and 1/128 and lower have no margin
                        placeValueRect.Move(0, yOffset),
                        RowBit.PrimaryTextColor.WithOpacity(bit.PlaceValueOpacity),
                        SAngle.Zero,
                        Alignment.Center);
                }

                // Draw the actual bit. We do want to vertically center it and it gets drawn at the top
                // of bitRect.
                var inWindow = windowLeftBit.HasValue
                    && exponent <= windowLeftBit.Value
                    && exponent >= windowRightBit!.Value;
                bitTextRect = bitTextRect.Move(0d, -bit.BounceHeight); // Apply bounce offset
                target.DrawTextDirectly(bit.BitText, _bitFont, bitTextRect.Move(0, yOffset), inWindow ? bitInWindowColor : bitOutOfWindowColor, SAngle.Zero);
            }

            // 5. Draw the arrow if visible

            // "Sensei, how do we draw a triangle pointing downward in SkiaSharp?"
            // "▼"
            var arrowEdgeLength = RowBit.BitSize.Width;
            var arrowLeft = ArrowCenterX - (arrowEdgeLength / 2d);
            var arrowRect = new SRectF(RowXToScreenX(arrowLeft),
                (RowBit.BitSize.Height / 2d) + (RowBit._bitTextSize.Height * RowBit.SmallFontSizeFactor) + (RowBit.SmallTextMarginRatioOfBitHeight * RowBit.BitSize.Height),
                arrowEdgeLength, RowBit._bitTextSize.Height);
            // CANIMPROVE: We should really think of this element like a stack of rows: arrow on top, exponents, bits, then place values
            // This way we can just compute the Y position of each row.
            target.DrawTextDirectly("▼", _bitFont.WithSize((float)arrowRect.Height), arrowRect.Move(0, yOffset), ArrowColor.WithOpacity(ArrowOpacity), SAngle.Zero);

            // 6. Draw the falling window rect if visible
            if (ShowFallingWindowRect)
            {
                // The falling window is a rectangle:
                //  - Centered at FallingWindowRectCenter
                //  - With a size of FallingWindowRectSize
                //  - Rotated by FallingWindowAngle
                // So we'll need a little bit of trig to compute the corners of the rectangle for drawing
                // wait what am I doing
                // also no need to convert from row coordinates
                // once initialized these are screen coordinates
                var fallingWindowRect = new SRectF(FallingWindowRectCenter.X - (FallingWindowRectSize.Width / 2d),
                                                   FallingWindowRectCenter.Y - (FallingWindowRectSize.Height / 2d),
                                                   FallingWindowRectSize.Width,
                                                   FallingWindowRectSize.Height);
                target.DrawRectangle(fallingWindowRect, FallingWindowColor, SPaintStyle.Fill, FallingWindowAngle);
            }

            // 7. Draw the negative indicator if necessary
            if (ShowNegativeFlag)
            {
                var elementTopRight = Bounds.TopRight;
                var xPadding = -10d;
                var yPadding = 5d;
                var negativeTextRect = new SRectF(elementTopRight.X + xPadding - _negativeTextSize.Width,
                    elementTopRight.Y + yPadding,
                    _negativeTextSize.Width,
                    _negativeTextSize.Height);
                target.DrawTextDirectly("Negative!", _bitFont.WithSize((float)_negativeTextSize.Height), negativeTextRect, NegativeFlagTextColor, SAngle.Zero);
            }
        }

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        // Public operations:
        // - ScrollCenterBit: Queues a FixedDurationAnimation to scroll a bit to the center of the element
        public void ScrollBitToCenter(int bitExponent)
        {
            double wantedBitCenter;
            if (bitExponent >= 0)
            {
                // Positive or zero exponent, bit to the left of the binary point, X is negative
                var bit0Right = -(RowBit.BinaryPointWidth / 2d);
                var wantedBitLeft = bit0Right - (RowBit.BitSize.Width * bitExponent);
                wantedBitCenter = wantedBitLeft + (RowBit.BitSize.Width / 2d);
            }
            else if (bitExponent < 0)
            {
                // Negative exponent, bit to the right of the binary point, X is positive
                var bitNeg1Left = RowBit.BinaryPointWidth / 2d;
                var wantedBitLeft = bitNeg1Left + (RowBit.BitSize.Width * (bitExponent - 1));
                wantedBitCenter = wantedBitLeft + (RowBit.BitSize.Width / 2d);
            }
            else
            {
                throw new InvalidOperationException("Unreachable: Invalid bitExponent: " + bitExponent);
            }

            var originalCenteredX = CenteredX;
            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(2d),
                p =>
                {
                    var newCenteredX = originalCenteredX + ((wantedBitCenter - originalCenteredX) * Easings.Smoothstep(p));
                    CenteredX = newCenteredX;
                });
            _animationContext.ScheduleAnimation(animation);
        }

        // - SetBit: Sets a bit value and optionally queues a Bounce animation for it
        public void SetBit(int bitExponent, bool value, bool bounce = false)
        {
            var bitIndex = 127 - bitExponent;
            _bits[bitIndex].BitSet = value;
            if (bounce)
            {
                var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.125d),
                    p =>
                    {
                        // Bounce height follows a sine wave pattern, peaking at 10 pixels
                        var bounceHeight = Math.Sin(p * Math.PI) * 10;
                        _bits[bitIndex].BounceHeight = (int)bounceHeight;
                    }, () => _bits[bitIndex].BounceHeight = 0);
                _animationContext.ScheduleAnimation(animation);
            }
        }
        // - SetBitAndAdvanceArrowAndScroll: Sets a bit value, queues a Bounce animation for it, advances the arrow to the next bit, and scrolls that next bit to the center of the element
        // - SetShowExponents: Shows/hides exponents with an ascent and fade-in animation
        public void SetShowExponents(bool show)
        {
            if (_showingExponents == show)
            {
                return; // Already in the desired state, do nothing
            }
            _showingExponents = show;

            var indexesToAnimate = new List<int>();
            var leftmostVisibleBit = RowBit.GetBitForXPosition(CenteredX - (Size.Width / 2d));
            var rightmostVisibleBit = RowBit.GetBitForXPosition(CenteredX + (Size.Width / 2d));
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var exponent = _bits[i].Exponent;
                if (exponent <= leftmostVisibleBit && exponent >= rightmostVisibleBit)
                {
                    indexesToAnimate.Add(i);
                }
                else
                {
                    // Just immediately show the exponents that aren't visible since we won't see the animation for them anyway
                    _bits[i].ExponentAscentProgress = show ? 1d : 0d;
                    _bits[i].ExponentOpacity = show ? 1d : 0d;
                }
            }

            var animations = new Queue<Func<FixedDurationAnimation>>(indexesToAnimate
                .Select(i =>
                {
                    var iCopy = i; // To capture the correct index in the lambda
                    Func<FixedDurationAnimation> factory = () => FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                        p =>
                        {
                            _bits[iCopy].ExponentAscentProgress = Easings.Land(show ? p : 1 - p);
                            _bits[iCopy].ExponentOpacity = Easings.Land(show ? p : 1 - p);
                        });
                    return factory;
                }));
            var animationCount = animations.Count;

            StaggerAnimations(animations, 1);
        }

        // - SetShowPlaceValues: Shows/hides place values with a descent and fade-in animation
        public void SetShowPlaceValues(bool show)
        {
            if (_showingPlaceValues == show)
            {
                return; // Already in the desired state, do nothing
            }
            _showingPlaceValues = show;

            var indexesToAnimate = new List<int>();
            var leftmostVisibleBit = RowBit.GetBitForXPosition(CenteredX - (Size.Width / 2d));
            var rightmostVisibleBit = RowBit.GetBitForXPosition(CenteredX + (Size.Width / 2d));
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var exponent = _bits[i].Exponent;
                if (exponent <= leftmostVisibleBit && exponent >= rightmostVisibleBit)
                {
                    indexesToAnimate.Add(i);
                }
                else
                {
                    // Just immediately show the place values that aren't visible since we won't see the animation for them anyway
                    _bits[i].PlaceValueDescentProgress = show ? 1d : 0d;
                    _bits[i].PlaceValueOpacity = show ? 1d : 0d;
                }
            }

            var animations = new Queue<Func<FixedDurationAnimation>>(indexesToAnimate
                .Select(i =>
                {
                    var iCopy = i; // To capture the correct index in the lambda
                    Func<FixedDurationAnimation> factory = () => FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                        p =>
                        {
                            _bits[iCopy].PlaceValueDescentProgress = Easings.Land(show ? p : 1 - p);
                            _bits[iCopy].PlaceValueOpacity = Easings.Land(show ? p : 1 - p);
                        });
                    return factory;
                }));
            var animationCount = animations.Count;

            StaggerAnimations(animations, 1);
        }
        // - MoveWindowToExponent: Queues a FixedDurationAnimation to move the window left edge to a specific bit
        //  - Also handles the case where the window contains the binary point, ensuring that the window is wide enough to show it and adjusting the target size accordingly
        //      - But only if the target has the binary point in it - if we just move the window over it, it shouldn't change width
        //  - Also handles the case where we're moving out of a cursed NaN window:
        //      - We stop jittering immediately and clear flags and the timer
        //      - We lerp the window color back to the default color
        //      - We lerp the width back to 24 bits (or 24.25 bits)
        // - SetShowNegativeFlag: Shows/hides the negative flag with a fade-in/out animation
        // - SetArrowBit: Queues a FixedDurationAnimation to move the arrow to point at a specific bit
        // - CenterOnArrow: Queues a FixedDurationAnimation to scroll the bits such that the arrow is centered in the element
        // - ComedicallyDropWindow: Hides the window and sets the falling window properties to drop a rectangle from the window's last position with a rotation
        // - ShowCursedNaNWindow: Flashes then holds a 23-bit wide red window at the window position to represent a NaN value, setting it to jitter for extra effect

        // Animation state flags (if any of these are set and we try to reset them (or set them again), we force end the current animation):
        // - Scrolling
        // - ChangingExponentVisibility
        // - ChangingPlaceValueVisibility
        // - MovingWindow
        // - MovingArrow
        // - FadingNegativeFlag
        // - WindowFalling
        // - ShowingCursedNaNWindow
        // maybe just have these as nullable fields and if there's already something here, we force end it and make a new one
        // and have Update check for completed animations and set to null

        private SPointF RowCoordinateToScreenCoordinate(SPointF rowCoordinate)
        {
            // Row is a rectangle with coordinates Xr, Yr and size Wr, Hr.
            // Element is a rectangle with coordinates Xe, Ye and size We, He.
            // Element has a center X at Xe + We/2.
            // This center X is by definition mapped to CenteredX in the row coordinates.
            // So to convert a point from row coordinates to screen coordinates:
            // 1. Find the offset of the point from the centered X in row coordinates: (Xr - CenteredX, Yr)
            // 2. Apply that same offset to the center X in screen coordinates: (Xe + We/2 + (Xr - CenteredX), Ye + Yr)
            var elementCenterX = Position.X + (Size.Width / 2d);
            var screenX = elementCenterX + (rowCoordinate.X - CenteredX);
            var screenY = Position.Y + rowCoordinate.Y;
            return new SPointF(screenX, screenY);
        }

        private double RowXToScreenX(double rowX)
        {
            var xOffset = Bounds.Center.X - CenteredX;
            return rowX + xOffset;
        }

        private bool SpanVisible(double xLo, double xHi)
        {
            var elementLeft = RowXToScreenX(CenteredX - (Size.Width / 2d));
            var elementRight = RowXToScreenX(CenteredX + (Size.Width / 2d));
            return !(xHi < elementLeft || xLo > elementRight);
        }

        private SRectF GetRowRenderingRectOnElement()
        {
            var hCenterLine = Size.Height / 2d;
            var halfBitHeight = RowBit.BitSize.Height / 2d;
            var top = hCenterLine - halfBitHeight;
            return new SRectF(Position.X, Position.Y + top, Size.Width, RowBit.BitSize.Height);
        }

        private SRectF GetRowRenderingRectOnRow()
        {
            var elementRenderSize = GetRowRenderingRectOnElement().Size;
            var halfWidthRenderSize = elementRenderSize.Width / 2d;
            var rowRenderLeft = CenteredX - halfWidthRenderSize;
            return new SRectF(rowRenderLeft, 0, elementRenderSize.Width, elementRenderSize.Height);
        }

        private bool FallingWindowRectFullyOffscreen(double viewportWidth, double viewportHeight)
        {
            var halfWidth = FallingWindowRectSize.Width / 2;
            var halfHeight = FallingWindowRectSize.Height / 2;
            var leftX = FallingWindowRectCenter.X - halfWidth;
            var rightX = FallingWindowRectCenter.X + halfWidth;
            var topY = FallingWindowRectCenter.Y - halfHeight;
            var bottomY = FallingWindowRectCenter.Y + halfHeight;
            return rightX < 0 || leftX > viewportWidth || bottomY < 0 || topY > viewportHeight;
        }

        private void StaggerAnimations(Queue<Func<FixedDurationAnimation>> animationFactories, int frameDelay)
        {
            var globalFrameRemainder = AtriaLayoutEngine.GlobalFrameNumber % frameDelay;
            var animationCount = animationFactories.Count;
            var staggeredAnimation = ContinuingAnimation.StartNow(() =>
            {
                var currentGlobalFrame = AtriaLayoutEngine.GlobalFrameNumber;
                if ((currentGlobalFrame % frameDelay) == globalFrameRemainder)
                {
                    if (animationFactories.Count != 0)
                    {
                        var nextAnimationFactory = animationFactories.Dequeue();
                        Console.WriteLine("Starting animation...");
                        _animationContext.ScheduleAnimation(nextAnimationFactory());
                    }
                }
                return animationFactories.Count != 0;
            });
            Console.WriteLine($"Scheduling staggered animation for {animationCount} bits");
            _animationContext.ScheduleContinuingAnimation(staggeredAnimation);
        }
    }
}
