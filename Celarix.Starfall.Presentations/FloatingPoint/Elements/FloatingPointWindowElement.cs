using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Color;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

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
            private const double SmallFontSizeFactor = 0.25d;
            private const double BitSpacingRatioOfBitHeight = 0.1d;
            private const double SmallTextMarginRatioOfBitHeight = 0.05d;
            
            private static readonly string[] PlaceValueSuffixes = ["", "K", "M", "B", "T", "P", "E", "Z", "Y", "R", "Q", "KQ", "MQ", "GQ"];
            public static readonly SColor PrimaryTextColor = SColor.White;
            public static readonly SColor SecondaryTextColor = new SColor(0xBF, 0xBF, 0xBF, 255);

            public static SSizeF _bitTextSize;

            // Bit properties
            public bool BitSet { get; set; }
            public int BounceHeight { get; set; }
            public string BitText => BitSet ? "1" : "0";

            // Exponent properties (on top)
            public int Exponent { get; set; }
            public bool ShowExponent { get; set; }
            public double ExponentOpacity { get; set; }
            public double ExponentAscentProgress { get; set; }
            public string ExponentText => $"2^{string.Concat(Exponent.ToString().Select(c => ToUnicodeSuperscript(c)))}";

            // Place value properties (on bottom)
            public bool ShowPlaceValue { get; set; }
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

                    if (Math.Abs(log10) < 4)
                    {
                        // Display numbers between 1 to 10,000 in full
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
            private static double BinaryPointWidth => _bitTextSize.Width / 2d;

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
                    var direction = Exponent >= 0 ? -1 : 1;
                    var facingBinaryPointEdgeX = (BinaryPointWidth / 2d) * direction;
                    var fullBitSize = BitSize;

                    double bitLeftX;
                    if (Exponent >= 0)
                    {
                        // We're to the left of the binary point. The left edge of the binary point
                        // faces this bit, and the higher the exponent, the lower the X coordinate.
                        var fullBitsWidth = fullBitSize.Width * -Exponent;
                        bitLeftX = fullBitsWidth + facingBinaryPointEdgeX;

                        // We also need to include the width of the bit itself. Why?
                        // Let's say Exponent == 3. That's -90 pixels, right? Not quite. It's actually -120
                        // to account for the 2^0 bit in addition to the other three bits.
                        bitLeftX -= fullBitSize.Width;
                    }
                    else
                    {
                        // We're to the right of the binary point. The right edge of the binary point faces
                        // this bit, and the lower the exponent, the higher the X coordinate.
                        var fullBitsWidth = fullBitSize.Width * -Exponent;
                        // yes we're inverting the exponent in both cases
                        // it's fine - here, the exponent is negative, we need it positive since we're
                        // going right (positive X)
                        bitLeftX = facingBinaryPointEdgeX + fullBitsWidth;
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
                    return (int)((-xPosition - (BinaryPointWidth / 2d)) / BitSize.Width);
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

        private const double WindowOuterMarginRatioOfBitHeight = 0.1d;
        private const double GravitationalAccelerationPxPerFrameSquared = 0.5d;
        private const double AngularAccelerationPerFrameSquared = 0.01d;
        private const double MaxFallingWindowVelocityPerFrame = 20d;
        private const double MaxFallingWindowAngularVelocityPerFrame = 0.5d;

        private static readonly SColor WindowDefaultColor = new SColor(255, 255, 0, 127);
        private static readonly SColor WindowNaNColor = new SColor(255, 0, 0, 127);
        private static readonly SColor ArrowColor = SColor.White;
        private static readonly SColor NegativeFlagTextColor = SColor.Red;

        private SFont _bitFont = new SFontFamily("Consolas", 36f);
        private GradientProvider _windowBitGradientProvider = new GradientProvider(RowBit.SecondaryTextColor, RowBit.PrimaryTextColor);
        private readonly RowBit[] _bits = new RowBit[RowBit.TotalSinglePrecisionBits];

        private bool _isJitteringWindow;
        private int _framesUntilJitterToggle;

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
                var windowMargin = RowBit._bitTextSize.Height * WindowOuterMarginRatioOfBitHeight;
                // TODO: make this account for possibly containing the binary point! It might need to be 1/4 a bit wider in some cases
                var width = (WindowWidthInBits * RowBit.BitSize.Width) + (windowMargin * 2);
                var height = RowBit.BitSize.Height + (windowMargin * 2);
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

            // Pre-measure the bit text size since it's constant and we'll need it for layout
            RowBit._bitTextSize = measurementService.MeasureText("0", _bitFont);
        }

        public override void Render(IRenderTarget target)
        {
            var elementRenderRect = GetRowRenderingRectOnElement();
            var yOffset = elementRenderRect.Y;

            var leftmostVisibleBit = RowBit.GetBitForXPosition(CenteredX - (elementRenderRect.Width / 2d));
            var rightmostVisibleBit = RowBit.GetBitForXPosition(CenteredX + (elementRenderRect.Width / 2d));
            var windowLeftBit = RowBit.GetBitForXPosition(WindowLeftX);
            var windowRightBit = windowLeftBit + WindowWidthInBits - 1;

            var bitOutOfWindowColor = RowBit.SecondaryTextColor;
            var bitInWindowColor = _windowBitGradientProvider.Sample(WindowOpacity);

            // 1. Draw the window first, if visible, so it's under the bits
            // 2. Draw the bits in the element (and remember the binary point if it's visible)
            // 3. Draw the exponents if visible
            // 4. Draw the place values if visible
            // 5. Draw the arrow if visible
            // 6. Draw the falling window rect if visible
            // 7. Draw the negative indicator if necessary
        }

        // Public operations:
        // - ScrollCenterBit: Queues a FixedDurationAnimation to scroll a bit to the center of the element
        // - SetBit: Sets a bit value and optionally queues a Bounce animation for it
        // - SetBitAndAdvanceArrowAndScroll: Sets a bit value, queues a Bounce animation for it, advances the arrow to the next bit, and scrolls that next bit to the center of the element
        // - SetShowExponents: Shows/hides exponents with an ascent and fade-in animation
        // - SetShowPlaceValues: Shows/hides place values with a descent and fade-in animation
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
    }
}
