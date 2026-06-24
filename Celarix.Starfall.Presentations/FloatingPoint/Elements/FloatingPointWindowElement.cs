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
            public const double SmallFontSizeFactor = 0.20d;
            private const double BitSpacingRatioOfBitHeight = 0.1d;
            public const double SmallTextMarginRatioOfBitHeight = 0.05d;
            
            private static readonly string[] PlaceValueSuffixes = ["K", "M", "B", "T", "P", "E", "Z", "Y", "R", "Q", "KQ", "MQ", "GQ"];
            public static readonly SColor PrimaryTextColor = SColor.White;
            public static readonly SColor SecondaryTextColor = new SColor(0xBF, 0xBF, 0xBF, 255);

            public static SSizeF _bitTextSize;

            public int? BaseOverride { get; set; }

            // Bit properties
            public bool BitSet { get; set; }
            public int BounceHeight { get; set; }
            public string BitText => BitSet ? "1" : "0";

            // Exponent properties (on top)
            public int Exponent { get; set; }
            public bool ShowExponent => ExponentOpacity != 0d;
            public double ExponentOpacity { get; set; }
            public double ExponentAscentProgress { get; set; }
            public string ExponentText => BaseOverride.HasValue
                ? $"{BaseOverride.Value}{string.Concat(Exponent.ToString().Select(c => FPHelpers.ToUnicodeSuperscript(c)))}"
                : $"2{string.Concat(Exponent.ToString().Select(c => FPHelpers.ToUnicodeSuperscript(c)))}";

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
                    var exponentValue = BigInteger.Pow(BaseOverride.HasValue ? BaseOverride.Value : 2, absExponent);
                    var log10 = BigInteger.Log10(exponentValue);

                    int lowExponent = BaseOverride.HasValue ? -2 : -9;
                    int highExponent = BaseOverride.HasValue ? 3 : 13;
                    if (Exponent >= lowExponent && Exponent <= highExponent)
                    {
                        // Display between 2^-9 and 2^13 as normal numbers (1/512 to 8192)
                        return isFraction ? $"1/{exponentValue}" : exponentValue.ToString();
                    }

                    // BigInteger.Log10 of 1 billion is 8.999999 or so, not 9. Detect when we're within
                    // an epsilon of 0.001 of an integer and round up.
                    var differenceHigh = Math.Abs(Math.Truncate(log10) - log10);
                    var differenceLow = Math.Abs(1 - differenceHigh);
                    if (differenceHigh < 0.001d || differenceLow < 0.001d)
                    {
                        log10 = Math.Ceiling(log10);
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
                    var facingCenterX = Exponent >= 0
                        ? -Exponent * BitSize.Width
                        : -(Exponent + 1) * BitSize.Width;
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
                    return -(int)Math.Ceiling((xPosition - (BinaryPointWidth / 2d)) / BitSize.Width);
                }

                throw new ArgumentException($"Unreachable: Invalid xPosition: {xPosition}");
            }
        }

        private const double WindowOuterMarginRatioOfBitHeight = 0.01d;
        private const double GravitationalAccelerationPxPerFrameSquared = 0.12d;
        private const int FramesUntilFiringWindowFallingOffscreenEvent = 90;   // 1.5 seconds @ 60fps
        private const double AngularAccelerationPerFrameSquared = 0.01d;
        private const double MaxFallingWindowVelocityPerFrame = 20d;
        private const double MaxFallingWindowAngularVelocityPerFrame = 0.5d;

        private static readonly SColor WindowDefaultColor = new SColor(255, 255, 0, 127);
        private static readonly SColor WindowNaNColor = new SColor(255, 0, 0, 127);
        private static readonly SColor ArrowColor = SColor.White;
        private static readonly SColor NegativeFlagTextColor = SColor.Red;

        private SFont _bitFont => new SFontFamily("Consolas", BaseFontSize);
        private GradientProvider _windowBitGradientProvider = new GradientProvider(RowBit.SecondaryTextColor, RowBit.PrimaryTextColor);
        private readonly RowBit[] _bits = new RowBit[RowBit.TotalSinglePrecisionBits];

        private AnimationContext _animationContext = new();
        private bool _showingExponents = false;
        private bool _showingPlaceValues = false;
        private bool _isJitteringWindow;
        private int _framesUntilJitterToggle;
        private int? _framesUntilFiringWindowFallingOffscreenEvent;
        private Random _random = new();
        private readonly SSizeF _negativeTextSize;
        private readonly MeasurementService _measurementService;
        private float _baseFontSize;

        public float BaseFontSize
        {
            get => _baseFontSize;
            set
            {
                _baseFontSize = value;
                RowBit._bitTextSize = _measurementService.MeasureText("0", _bitFont);
            }
        }

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
        public int ArrowExponent => RowBit.GetBitForXPosition(ArrowCenterX) ?? 0;

        // Window properties
        public SColor WindowColor { get; set; } = WindowDefaultColor;

        /// <summary>
        /// The width of the window in bits (can be fractional during animations).
        /// </summary>
        public double WindowWidthInBits { get; set; }

        /// <summary>
        /// Factor (0.0 to 1.0) for adding binary point width. Allows smooth transitions.
        /// </summary>
        public double BinaryPointWidthFactor { get; set; }

        public SSizeF WindowSize
        {
            get
            {
                var bitWidth = RowBit.BitSize.Width;
                var width = WindowWidthInBits * bitWidth;
                
                // Add binary point width based on the factor (allows smooth lerping)
                width += RowBit.BinaryPointWidth * BinaryPointWidthFactor;
                
                var height = RowBit.BitSize.Height;
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
        public double NegativeFlagOpacity { get; set; }
        public float NegativeFlagFontSize => (float)(RowBit._bitTextSize.Height * RowBit.SmallFontSizeFactor);

        // Falling window properties
        public bool ShowFallingWindowRect { get; set; }
        public SPointF FallingWindowRectCenter { get; set; }
        public SAngle FallingWindowAngle { get; set; }
        public SSizeF FallingWindowRectSize { get; set; }
        public SColor FallingWindowColor { get; set; }
        public SAngle FallingWindowAngularVelocityPerFrame { get; set; }
        public SPointF FallingWindowCenterAccelerationPerFrame { get; set; }
        public SPointF FallingWindowCenterVelocityPerFrame { get; set; }
        public event EventHandler? FallingWindowRectExited;

        public FloatingPointWindowElement(string atriaIdString, MeasurementService measurementService)
        {
            Id = AtriaId.Parse(atriaIdString);
            _measurementService = measurementService;

            // Initialize the bits
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var exponent = 127 - i;
                _bits[i].Exponent = exponent;
            }

            // Pre-measure the bit text size since it's constant and we'll need it for layout
            _baseFontSize = 120f;
            RowBit._bitTextSize = measurementService.MeasureText("0", _bitFont);
            _negativeTextSize = measurementService.MeasureText("Negative!", _bitFont.WithSize(NegativeFlagFontSize));

            // These should probably be initialized externally?
            WindowWidthInBits = 24;
            WindowOpacity = 1d;
            CenteredX = 0;
            WindowLeftX = _bits[127].Position.X;
            ArrowCenterX = _bits[127].Position.WithSize(RowBit._bitTextSize).Center.X;
            ArrowOpacity = 1;
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
                target.DrawRectangle(finalWindowRect, WindowColor.WithOpacity(WindowOpacity * Opacity), SPaintStyle.Fill, SAngle.Zero);
            }

            // 2. Draw the bits in the element (and remember the binary point if it's visible)
            var binaryPointHalfWidth = RowBit.BinaryPointWidth / 2d;
            var binaryPointRect = new SRectF(RowXToScreenX(-binaryPointHalfWidth), 0d, RowBit.BinaryPointWidth, RowBit.BitSize.Height);
            if (SpanVisible(binaryPointRect.Left, binaryPointRect.Right))
            {
                var inWindow = WindowSize.Width % 1 != 0;
                SColor color = inWindow ? RowBit.PrimaryTextColor : RowBit.SecondaryTextColor;
                target.DrawText(".", _bitFont, binaryPointRect.Move(0, yOffset), color.WithOpacity(Opacity), SAngle.Zero, Alignment.Center);
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
                        RowBit.PrimaryTextColor.WithOpacity(bit.ExponentOpacity * Opacity),
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
                        RowBit.PrimaryTextColor.WithOpacity(bit.PlaceValueOpacity * Opacity),
                        SAngle.Zero,
                        Alignment.Center);
                }

                // Draw the actual bit. We do want to vertically center it and it gets drawn at the top
                // of bitRect.
                var bitRight = bitLeft + RowBit.BitSize.Width;
                var windowRight = WindowLeftX + WindowSize.Width;
                var inWindow = bitRight > WindowLeftX && bitLeft < windowRight;
                bitTextRect = bitTextRect.Move(0d, -bit.BounceHeight); // Apply bounce offset
                SColor color = inWindow ? bitInWindowColor : bitOutOfWindowColor;
                target.DrawTextDirectly(bit.BitText, _bitFont, bitTextRect.Move(0, yOffset), color.WithOpacity(Opacity), SAngle.Zero);
            }

            // 5. Draw the arrow if visible

            // "Sensei, how do we draw a triangle pointing downward in SkiaSharp?"
            // "▼"
            var arrowEdgeLength = RowBit.BitSize.Width;
            var arrowLeft = ArrowCenterX - (arrowEdgeLength / 2d);
            var arrowRect = new SRectF(RowXToScreenX(arrowLeft), -arrowEdgeLength, arrowEdgeLength, RowBit._bitTextSize.Height);
            // CANIMPROVE: We should really think of this element like a stack of rows: arrow on top, exponents, bits, then place values
            // This way we can just compute the Y position of each row.
            target.DrawText("▼", _bitFont.WithSize((float)arrowRect.Height), arrowRect.Move(0, yOffset), ArrowColor.WithOpacity(ArrowOpacity * Opacity), SAngle.Zero);

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
                target.DrawRectangle(fallingWindowRect, FallingWindowColor.WithOpacity(Opacity), SPaintStyle.Fill, FallingWindowAngle);
            }

            // 7. Draw the negative indicator if necessary
            if (ShowNegativeFlag && NegativeFlagOpacity > 0d)
            {
                var elementTopRight = Bounds.TopRight;
                var xPadding = -10d;
                var yPadding = 5d;
                var negativeTextRect = new SRectF(elementTopRight.X + xPadding - _negativeTextSize.Width,
                    elementTopRight.Y + yPadding,
                    _negativeTextSize.Width,
                    _negativeTextSize.Height);
                target.DrawTextDirectly("Negative!",
                    _bitFont.WithSize(NegativeFlagFontSize),
                    negativeTextRect,
                    NegativeFlagTextColor.WithOpacity(NegativeFlagOpacity * Opacity),
                    SAngle.Zero);
            }
        }

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);

            if (ShowFallingWindowRect)
            {
                // Update the falling window's position and angle based on its velocity and angular velocity
                FallingWindowCenterVelocityPerFrame = FallingWindowCenterVelocityPerFrame.Move(FallingWindowCenterAccelerationPerFrame.X, FallingWindowCenterAccelerationPerFrame.Y);
                FallingWindowRectCenter = FallingWindowRectCenter.Move(FallingWindowCenterVelocityPerFrame.X, FallingWindowCenterVelocityPerFrame.Y);
                FallingWindowAngle = FallingWindowAngle + FallingWindowAngularVelocityPerFrame;

                // Check if the falling window is completely off-screen. If so, stop showing it.
                if (!SRectF.RotatedIntersects(Slide!.Size.At(SPointF.Zero), FallingWindowRectSize.CenterAt(FallingWindowRectCenter),
                    SAngle.Zero, FallingWindowAngle))
                {
                    ShowFallingWindowRect = false;
                    _framesUntilFiringWindowFallingOffscreenEvent = FramesUntilFiringWindowFallingOffscreenEvent;
                }
            }

            if (_framesUntilFiringWindowFallingOffscreenEvent.HasValue)
            {
                _framesUntilFiringWindowFallingOffscreenEvent--;
                if (_framesUntilFiringWindowFallingOffscreenEvent <= 0)
                {
                    _framesUntilFiringWindowFallingOffscreenEvent = null;
                    FallingWindowRectExited?.Invoke(this, EventArgs.Empty);
                }
            }

            if (DoWindowJitter)
            {
                _framesUntilJitterToggle--;
                if (_framesUntilJitterToggle <= 0)
                {
                    _isJitteringWindow = !_isJitteringWindow;
                    _framesUntilJitterToggle = _random.Next(5, 16); // Randomly toggle jitter every 5 to 15 frames
                }
            }
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
                var wantedBitLeft = bitNeg1Left + (RowBit.BitSize.Width * -(bitExponent - 1));
                wantedBitCenter = wantedBitLeft + (RowBit.BitSize.Width / 2d);
            }
            else
            {
                throw new InvalidOperationException("Unreachable: Invalid bitExponent: " + bitExponent);
            }

            var originalCenteredX = CenteredX;
            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
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
        public void SetBitAndAdvanceArrowAndScroll(int bitExponent, bool value)
        {
            SetBit(bitExponent, value, bounce: true);
            var nextBitExponent = bitExponent - 1;
            var nextBitIndex = 127 - nextBitExponent;
            var nextBitPosition = _bits[nextBitIndex].Position;
            //var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
            //    p =>
            //    {
            //        // Move the arrow center X from its current position to the center of the next bit
            //        var wantedArrowCenterX = nextBitPosition.X + (RowBit.BitSize.Width / 2d);
            //        ArrowCenterX = ArrowCenterX + ((wantedArrowCenterX - ArrowCenterX) * Easings.Smoothstep(p));
            //    });
            //_animationContext.ScheduleAnimation(animation);
            SetArrowBit(nextBitExponent);
            ScrollBitToCenter(nextBitExponent);
        }

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
        public void MoveWindowToExponent(int targetExponent, int mantissaBits = 24, double durationSeconds = 1.0)
        {

            // Calculate target position
            var targetLeftX = _bits.First(b => b.Exponent == targetExponent).Position.X;
            
            // Calculate target width (handles binary point automatically)
            var (targetWidth, targetBinaryPointFactor) = CalculateWindowWidthForPosition(targetLeftX, mantissaBits);
            
            // Capture starting values
            var startLeftX = WindowLeftX;
            var startWidth = WindowWidthInBits;
            var startBinaryPointFactor = BinaryPointWidthFactor;

            if (Math.Abs(WindowOpacity) < 0.001d)
            {
                // We can't see the window, just instantly move it without animating.
                WindowLeftX = targetLeftX;
                WindowWidthInBits = targetWidth;
                BinaryPointWidthFactor = targetBinaryPointFactor;
                return;
            }

            var animation = FixedDurationAnimation.StartNow(
                AnimationContext.SecondsToFrames(durationSeconds),
                p =>
                {
                    WindowLeftX = startLeftX + (targetLeftX - startLeftX) * p;
                    WindowWidthInBits = startWidth + (targetWidth - startWidth) * p;
                    BinaryPointWidthFactor = startBinaryPointFactor + (targetBinaryPointFactor - startBinaryPointFactor) * p;
                });
            
            _animationContext.ScheduleAnimation(animation);
        }

        // - SetShowNegativeFlag: Shows/hides the negative flag with a fade-in/out animation
        public void SetShowNegativeFlag(bool show)
        {
            if (ShowNegativeFlag == show)
            {
                return; // Already in the desired state, do nothing
            }
            ShowNegativeFlag = show;
            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    NegativeFlagOpacity = Easings.Land(show ? p : 1 - p);
                });
            _animationContext.ScheduleAnimation(animation);
        }

        public void SetShowArrow(bool show)
        {
            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    ArrowOpacity = Easings.Land(show ? p : 1 - p);
                });
            _animationContext.ScheduleAnimation(animation);
        }

        // - SetArrowBit: Queues a FixedDurationAnimation to move the arrow to point at a specific bit
        public void SetArrowBit(int exponent)
        {
            var bitIndex = 127 - exponent;
            var bitRect = _bits[bitIndex].Position.WithSize(RowBit._bitTextSize);
            var wantedArrowCenterX = bitRect.Center.X;
            var originalArrowCenterX = ArrowCenterX;

            if (Math.Abs(ArrowOpacity) < 0.001d)
            {
                // We can't see the arrow, just instantly move it without animating.
                ArrowCenterX = wantedArrowCenterX;
                return;
            }

            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.8d),
                p =>
                {
                    var newArrowCenterX = originalArrowCenterX + ((wantedArrowCenterX - originalArrowCenterX) * Easings.Smoothstep(p));
                    ArrowCenterX = newArrowCenterX;
                });
            _animationContext.ScheduleAnimation(animation);
        }

        // - CenterOnArrow: Queues a FixedDurationAnimation to scroll the bits such that the arrow is centered in the element
        public void CenterOnArrow()
        {
            var wantedCenteredX = RowXToScreenX(ArrowCenterX);
            var originalCenteredX = CenteredX;
            var animation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.8d),
                p =>
                {
                    var newCenteredX = originalCenteredX + ((wantedCenteredX - originalCenteredX) * Easings.Smoothstep(p));
                    CenteredX = newCenteredX;
                });
            _animationContext.ScheduleAnimation(animation);
        }

        // - ComedicallyDropWindow: Hides the window and sets the falling window properties to drop a rectangle from the window's last position with a rotation
        public void ComedicallyDropWindow()
        {
            // Set the falling window properties to drop a rectangle from the window's last position with a rotation
            FallingWindowRectCenter = new SPointF(RowXToScreenX(WindowLeftX + (WindowSize.Width / 2d)), GetRowRenderingRectOnElement().Y + WindowSize.Height / 2d);
            FallingWindowRectSize = WindowSize;
            FallingWindowColor = WindowColor;
            FallingWindowAngle = SAngle.Zero;
            FallingWindowAngularVelocityPerFrame = SAngle.FromDegrees(_random.NextDouble() * 2 - 1); // Random angular velocity between -1 and 1 degrees per frame
            FallingWindowCenterAccelerationPerFrame = new SPointF(0, GravitationalAccelerationPxPerFrameSquared);
            FallingWindowCenterVelocityPerFrame = new SPointF(0, 0);
            ShowFallingWindowRect = true;
            // Hide the window
            WindowOpacity = 0;
        }
        // - ShowCursedNaNWindow: Flashes then holds a 23-bit wide red window at the window position to represent a NaN value, setting it to jitter for extra effect

        public void SetDisplayedExponentBase(int newBase)
        {
            // Doesn't actually make the bits support the new base, but is fine for display as long as
            // you don't set anything
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                _bits[i].BaseOverride = newBase;
            }
        }

        public void ClearAllSetBits(bool bounce)
        {
            for (var i = 0; i < RowBit.TotalSinglePrecisionBits; i++)
            {
                var bit = _bits[i];
                if (!bit.BitSet) { continue; }

                var exponent = bit.Exponent;
                SetBit(exponent, false, bounce: bounce);
            }
        }

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

        private (double widthInBits, double binaryPointFactor) CalculateWindowWidthForPosition(double leftX, int mantissaBits)
        {
            var leftBit = RowBit.GetBitForXPosition(leftX);
            var rightBit = leftBit - mantissaBits + 1;

            // Check if the binary point (between bit 0 and bit -1) falls within this range
            bool containsBinaryPoint = leftBit.HasValue && leftBit.Value >= 0 && rightBit < 0;

            return (mantissaBits, containsBinaryPoint ? 1.0 : 0.0);
        }
    }
}
