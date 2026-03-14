using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.FloatingPoint
{
    public sealed class FloatingPointWindowElement : HeliumElement
    {
        private char[] digits;
        private int selectedIndex;
        private int windowWidth;
        private int windowLeftIndex;
        private readonly int normalWindowWidth;
        private readonly int basePointIndex;
        private readonly int placeValueExponentBase;
        private readonly int highestBitPlaceValueExponent;

        // Text rendering fields
        private double digitAspectRatio;
        private double widestPlaceValueAspectRatio;
        private SFont digitFont;
        private SFont placeValueFont;

        public override IReadOnlyList<HeliumElement> Children => [];

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

        private int SubnormalWindowWidth => normalWindowWidth - 1;

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < 0 || value >= digits.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                selectedIndex = value;
            }
        }

        public int WindowLeftIndex
        {
            get => windowLeftIndex;
            set
            {
                if (value < 0 || value > digits.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                // Clamp the window left index so that the window doesn't go out of bounds.
                var maxWindowLeftIndex = digits.Length - (windowWidth - 1);

                // Why did we let the window go up to one digit off the right?
                var isSubnormal = (value + windowWidth) == digits.Length;
                if (isSubnormal)
                {
                    // So we can shrink the window if we need to, to get rid of the implicit bit for subnormal numbers.
                    windowWidth = SubnormalWindowWidth;
                }
                else
                {
                    // If we're not subnormal, make sure the window is at its normal width.
                    windowWidth = normalWindowWidth;
                }

                windowLeftIndex = Math.Clamp(value, 0, maxWindowLeftIndex);
            }
        }

        public int WindowRightIndex => windowLeftIndex + (windowWidth - 1);

        /// <summary>
        /// Constructs a new instance of the <see cref="FloatingPointWindowElement"/> class.
        /// </summary>
        /// <param name="totalDigits">The number of digits that the floating point format can fully express.</param>
        /// <param name="windowWidth">The number of digits of precision that the floating point format can store.</param>
        /// <param name="placeValueExponentBase">Place values are written as base^exponent - this parameter specified the base. Often 2 or 10.</param>
        /// <param name="highestBitPlaceValueExponent">The exponent of the left-most, highest-value digit.</param>
        /// <param name="basePointIndex">The index of the digit representing the place value of 1 (i.e. the "binary point" or "decimal point").</param>
        /// <example>
        /// For the IEEE-754 single-precision format, the parameters would be:
        /// - totalDigits: 277 (Highest bit position is 2^127, lowest is 2^-149
        ///   (from the minimum exponent of -126 minus 23 mantissa bits), so 127 + 149 + 1 (for 2^0
        ///   itself) = 277.)
        /// - windowWidth: 24 (23 bits of precision, plus one for the implicit leading bit)... unless the exponent
        ///   is at its minimum of -126, in which case the implicit bit is removed, so the window width here is only 23.
        /// - placeValueExponentBase: 2 (place values are powers of 2)
        /// - highestBitPlaceValueExponent: 127 (the leftmost bit is 2^127)
        /// - basePointIndex: 127 (the "binary point" is between the bits representing 2^0 and 2^-1, so the bit representing 2^0 is at index 127)
        /// </example>
        public FloatingPointWindowElement(int totalDigits, int windowWidth, int placeValueExponentBase,
            int highestBitPlaceValueExponent, int basePointIndex)
        {
            digits = [.. Enumerable.Repeat('0', totalDigits)];
            normalWindowWidth = windowWidth;
            this.windowWidth = windowWidth;
            this.placeValueExponentBase = placeValueExponentBase;
            this.highestBitPlaceValueExponent = highestBitPlaceValueExponent;
            this.basePointIndex = basePointIndex;
            selectedIndex = basePointIndex;
            windowLeftIndex = selectedIndex - (windowWidth - 1);

            digitFont = new SFontFamily("Consolas");
            placeValueFont = new SFontFamily("Calibri");
        }

        public void SetDigit(int index, char digit)
        {
            if (index < 0 || index >= digits.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            digits[index] = digit;
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            // No children, so nothing to arrange.
        }

        public override HeliumElement Clone()
        {
            return new FloatingPointWindowElement(digits.Length,
                windowWidth,
                placeValueExponentBase,
                highestBitPlaceValueExponent,
                basePointIndex)
            {
                digits = (char[])digits.Clone()
            };
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            // Alright. This is where the fun stuff happens. Let's describe the layout.
            // This element is meant to be a row of digits with place values above them and
            // an arrow above that, pointing to the selected digit. Each slot is a stack of
            // three things: the arrow, place value, then digit. The container's height is
            // divided into five parts and the three elements take ratios 1:1:3.
            var containerHeight = ActualSize!.Value.Height;
            var slotHeightPart = containerHeight / 5d;
            var arrowHeight = slotHeightPart;
            var placeValueHeight = slotHeightPart;
            var digitHeight = slotHeightPart * 3d;

            // We want each slot to be the same width, and that width is the width of the widest
            // of all slots. The slot's width is the widest of the three elements. Let's figure out
            // the width of the digit first, given its height.
            var digitWidth = digitAspectRatio * digitHeight;
            var widestPlaceValueWidth = widestPlaceValueAspectRatio * placeValueHeight;
            var slotWidth = Math.Max(digitWidth, widestPlaceValueWidth);

            // The arrow has a square aspect ratio, so it is always narrower than either the digit
            // or place value, so we don't need to consider it when calculating the slot width. We
            // do want a margin, though.
            const double MarginWidthRatio = 0.4d;
            var marginWidth = slotWidth * MarginWidthRatio;
            var totalInternalWidth = (slotWidth + marginWidth) * digits.Length;

            // The arrow is always somewhere visible in the container. It is either:
            //  - Somewhere left-of-center, indicating it's pointing at one of the highest digits
            //  - Somewhere right-of-center, indicating it's pointing at one of the lowest digits
            //  - Centered, indicating it's pointing at one of the digits in the middle
            // Let's start by always centering it, laying out the slots below it, and seeing if there's
            // empty space on the left or right.
            var outerPosition = ActualPosition!.Value;
            var outerSize = ActualSize!.Value;
            var outerBounds = new SRectF(outerPosition, outerSize);
            var arrowX = outerSize.Width / 2d;

            // The arrow is centered within the slot it's pointing at.
            var arrowPointingAtSlotX = slotWidth / 2d;

            // Figure out how many slots we'll need to draw before the selected slot.
            // (i.e. LLLLLLLSRRRRRRR)
            var slotsBeforeSelected = selectedIndex;    // This works because if slot 0 is selected, there are 0 slots before it, if
                                                        // slot 1 is selected, there is 1 slot before it, etc.
            
            // Figure out where the leftmost slot will be.
            var slotsBeforeSelectedWidth = slotsBeforeSelected * (slotWidth + marginWidth);
            var selectedSlotWidthLeft = slotWidth / 2d;
            var widthLeftOfArrow = slotsBeforeSelectedWidth + selectedSlotWidthLeft;
            var leftmostSlotX = arrowX - widthLeftOfArrow;
            var rightmostSlotRight = leftmostSlotX + totalInternalWidth;

            // Figure out if the arrow can stay centered, or if we need to shift everything to close a gap on the left or right.
            var leftGapExists = leftmostSlotX > 0;
            var rightGapExists = rightmostSlotRight < outerSize.Width;
            var gapFillOffset = 0d;
            if (leftGapExists == rightGapExists)
            {
                // There aren't even enough slots to fill the container, so just center everything.
                // (or somehow the outer size and the inner size are exactly the same)
                gapFillOffset = 0d;
            }
            else if (leftGapExists)
            {
                // Shift everything to the left to close the gap on the left.
                gapFillOffset = -leftmostSlotX;
            }
            else if (rightGapExists)
            {
                // Shift everything to the right to close the gap on the right.
                gapFillOffset = outerSize.Width - rightmostSlotRight;
            }

            arrowX += gapFillOffset;
            leftmostSlotX += gapFillOffset;

            // okay we can actually start drawing stuff now
            var arrowRenderable = new TextRenderable
            {
                Text = "▼",
                Font = new SFontFamily("Segoe UI"),
                Rotation = SAngle.Zero,
                Color = SColor.Red,
                Bounds = new SRectF(arrowX - (arrowHeight / 2d), outerPosition.Y, arrowHeight, arrowHeight),
                Id = "arrow"
            };

            var placeValueRenderables = new List<TextRenderable>();
            var digitRenderables = new List<TextRenderable>();

            for (int i = 0; i < digits.Length; i++)
            {
                var slotX = leftmostSlotX + i * (slotWidth + marginWidth);
                var placeValueY = outerPosition.Y + arrowHeight;
                var digitY = outerPosition.Y + arrowHeight + placeValueHeight;

                var exponent = highestBitPlaceValueExponent - i;
                var exponentString = exponent.ToString();
                var superscriptedExponent = new string(exponentString.Select(ToUnicodeSuperscript).ToArray());
                var placeValueRenderable = new TextRenderable
                {
                    Text = $"{placeValueExponentBase}{superscriptedExponent}",
                    Font = placeValueFont,
                    Color = SColor.White,
                    Bounds = new SRectF(slotX, placeValueY, slotWidth, placeValueHeight),
                    Rotation = SAngle.Zero,
                    Id = $"placeValue_{exponent}"
                };
                placeValueRenderable.AddClass("placeValue");

                var digit = digits[i];
                var digitRenderable = new TextRenderable
                {
                    Text = digit.ToString(),
                    Font = digitFont,
                    Color = SColor.White,
                    Bounds = new SRectF(slotX, digitY, slotWidth, digitHeight),
                    Rotation = SAngle.Zero,
                    Id = $"digit_{exponent}"
                };
                digitRenderable.AddClass("digit");

                // Draw one additional slot width on either side of the container, even if we can't
                // see these slots. This lets transitions select slots just outside the container if
                // they want to slide things over.
                var outerMargin = slotWidth + marginWidth;
                var extendedBounds = new SRectF(slotX - outerMargin,
                    placeValueY,
                    slotWidth + (2 * outerMargin),
                    outerBounds.Height);

                if (SRectF.Intersects(placeValueRenderable.Bounds, extendedBounds))
                {
                    // Don't worry about drawing anything fully outside the extended bounds.
                    placeValueRenderables.Add(placeValueRenderable);
                }

                if (SRectF.Intersects(digitRenderable.Bounds, extendedBounds))
                {
                    // Don't worry about drawing anything fully outside the extended bounds.
                    digitRenderables.Add(digitRenderable);
                }
            }

            // okay now the window. It spans from the left edge of its leftmost slot to the right edge
            // of its rightmost slot, and is as tall as the place value and digit combined.
            var windowLeftX = windowLeftIndex * (slotWidth + marginWidth) + leftmostSlotX;
            var windowActualWidth = windowWidth * (slotWidth + marginWidth) - marginWidth;
            var windowTopY = outerPosition.Y + (0.2d * outerSize.Height);
            var windowHeight = placeValueHeight + digitHeight;
            var windowRenderable = new RectangleRenderable
            {
                Color = new SColor(255, 255, 0, 75),   // Semi-transparent yellow
                Bounds = new SRectF(windowLeftX, windowTopY, windowActualWidth, windowHeight),
                Id = "window",
                PaintStyle = SPaintStyle.Fill
            };

            return [windowRenderable, arrowRenderable, .. placeValueRenderables, .. digitRenderables];
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            // This element always fills its available space, so we don't need to do any measuring here.
            ActualSize = availableSize;

            if (digitAspectRatio != 0 && widestPlaceValueAspectRatio != 0)
            {
                // We've already measured the text, so we can skip this step.
                return;
            }

            digitAspectRatio = measurementService.AspectRatioForText("0", digitFont);

            // The largest place value position is the rightmost one, with the lowest exponent, so we measure that one to get the widest place value.
            var lowestExponent = highestBitPlaceValueExponent - (digits.Length - 1);
            var exponentString = lowestExponent.ToString();
            var superscriptedExponent = new string(exponentString.Select(ToUnicodeSuperscript).ToArray());
            widestPlaceValueAspectRatio = measurementService.AspectRatioForText($"{placeValueExponentBase}{superscriptedExponent}", placeValueFont);
        }

        public static char ToUnicodeSuperscript(char digit)
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
}
