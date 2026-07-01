using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class PrecisionGridElement : AtriaElement
    {
        private const double UnlitDotRadiusMultipleOfCellWidth = 0.1d;
        private const double LitDotRadiusMultipleOfCellWidth = 0.3d;
        private const int FramesBetweenUpdates = 4;
        private const double SecondsBetweenDoublingsTarget = 5d;

        private static readonly SColor _unlitDotColor = new SColor(0x05, 0x3b, 0x00, 0xFF);
        private static readonly SColor _litDotColor = new SColor(0x0f, 0xb8, 0x00, 0xFF);
        private static readonly SColor _windowColor = SColor.Yellow.WithOpacity(0.5d);

        private readonly bool[,] _dots = new bool[277, 100];
        private bool _windowInitialPositionSet = false;
        private int _windowLeftExponent = 0;
        private double _leftXOf2ToThe0Bit = 0d;
        private double _drawnWindowLeftX;
        private double? _desiredWindowLeftX;
        private AnimationContext _animationContext = new AnimationContext();
        private readonly Random _random = new Random();

        public double DotsOnRow { get; set; }
        public bool AnimationRunning { get; set; }

        public PrecisionGridElement(string atriaIdString)
        {
            Id = AtriaId.Parse(atriaIdString);
        }

        public override void Render(IRenderTarget target)
        {
            if (Slide == null)
            {
                return;
            }

            var slideWidth = Slide.Size.Width;
            var cellWidth = slideWidth / DotsOnRow;
            var dotCenterXs = new List<double>();
            var lastX = 0d;
            while (lastX < slideWidth)
            {
                dotCenterXs.Add(lastX + cellWidth / 2d);
                lastX += cellWidth;
            }

            var dotCenterYs = new List<double>();
            var lastY = 0d;
            while (lastY < Slide.Size.Height)
            {
                dotCenterYs.Add(lastY + cellWidth / 2d);
                lastY += cellWidth;
            }

            // We're storing the full 277-bit range of a single-precision float, but we're only going to
            // be showing a subset of that range. Pick the dot closest to the ~~center~~ 1/3rd point without going over
            // the ~~center~~ 1/3rd point and declare that the 2^0 bit by fiat.
            // (the center was too far over)
            var centerXDotIndex = -1;
            for (var i = 0; i < dotCenterXs.Count; i++)
            {
                if (dotCenterXs[i] > slideWidth / 3d) { break; }
                centerXDotIndex = i;
            }

            // Now that we know the index of the 2^0 bit, let's figure out how many bits we draw to the left...
            var dotsBeforeCenterDot = centerXDotIndex;
            // ...and the index into _dots that corresponds to the leftmost dot we will draw...
            var leftmostDotIndex = ExponentToBitIndex(0) - dotsBeforeCenterDot;

            // sigh...
            if (!_windowInitialPositionSet)
            {
                var centerDotX = dotCenterXs[centerXDotIndex];
                _drawnWindowLeftX = centerDotX - (cellWidth / 2d);
                _leftXOf2ToThe0Bit = dotCenterXs[centerXDotIndex] - (cellWidth / 2d);
                _windowInitialPositionSet = true;
            }

            // Draw the window first so it's under the bits.
            var windowWidth = cellWidth * 24;
            var windowHeight = cellWidth;
            target.DrawRectangle(new SRectF(_drawnWindowLeftX, 0d, windowWidth, windowHeight), _windowColor.WithOpacity(Opacity), SPaintStyle.Fill, SAngle.Zero);

            // Now draw the dots.
            for (var y = 0; y < dotCenterYs.Count; y++)
            {
                for (var x = 0; x < dotCenterXs.Count; x++)
                {
                    var dotXIndex = leftmostDotIndex + x;
                    var dotYIndex = y;
                    if (dotXIndex < 0 || dotXIndex >= _dots.GetLength(0) || dotYIndex < 0 || dotYIndex >= _dots.GetLength(1))
                    {
                        continue;
                    }
                    var isLit = _dots[dotXIndex, dotYIndex];
                    var dotColor = isLit ? _litDotColor : _unlitDotColor;
                    var dotRadius = cellWidth * (isLit ? LitDotRadiusMultipleOfCellWidth : UnlitDotRadiusMultipleOfCellWidth);
                    target.DrawEllipse(new SPointF(dotCenterXs[x], dotCenterYs[y]), new SSizeF(dotRadius * 2d, dotRadius * 2d), dotColor.WithOpacity(Opacity), SPaintStyle.Fill);
                }
            }
        }

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);

            if (!AnimationRunning || (AtriaLayoutEngine.GlobalFrameNumber % FramesBetweenUpdates != 0)) { return; }

            var accumulator = BuildFloatFromRow(0);
            var nextAddend = BuildFloatFromRow(1);
            var newAccumulator = accumulator + nextAddend;
            WriteFloatToRow(newAccumulator, 0, out var newExponent);

            if (newExponent != _windowLeftExponent && newAccumulator != 0f)
            {
                _windowLeftExponent = newExponent;
                _desiredWindowLeftX = ExponentToScreenX(_windowLeftExponent);
                var oldWindowLeftX = _drawnWindowLeftX;
                _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(FramesBetweenUpdates, p =>
                {
                    _drawnWindowLeftX = MathHelpers.Ease(oldWindowLeftX, _desiredWindowLeftX.Value, p, Easings.Linear);
                }, () => _desiredWindowLeftX = null));
            }

            // Move every upcoming addend up one row.
            for (var y = 1; y < 99; y++)
            {
                for (var x = 0; x < 277; x++)
                {
                    _dots[x, y] = _dots[x, y + 1];
                }
            }

            // Make a new addend and put it in the bottom row.
            var newAddend = GetNextAddend();
            Console.WriteLine($"New addend: {newAddend}");
            WriteFloatToRow(newAddend, 99, out _);
        }

        private int BitIndexToExponent(int bitIndex)
        {
            // f(0) = 127
            // f(1) = 126
            // f(2) = 125
            // ...
            // f(127) = 0
            // f(128) = -1
            // ...
            // f(253) = -126
            // So f(x) = 127 - x
            return 127 - bitIndex;
        }

        private int ExponentToBitIndex(int exponent)
        {
            // Bleh, this kind of math I've never been good at.
            // f(127) = 0
            // f(126) = 1
            // f(125) = 2
            // ...
            // f(0) = 127
            // f(-1) = 128
            // ...
            // f(-126) = 253
            // So f(x) = 127 - x
            return 127 - exponent;
        }

        private double ExponentToScreenX(int exponent)
        {
            if (Slide == null) { return float.NaN; }
            var bitIndex = ExponentToBitIndex(exponent);

            var bitsFrom2ToThe0 = -exponent;
            var cellWidth = Slide.Size.Width / DotsOnRow;
            return _leftXOf2ToThe0Bit + (bitsFrom2ToThe0 * cellWidth);
        }

        private float BuildFloatFromRow(int row)
        {
            int? firstSetBitIndex = null;
            for (var x = 0; x < 277; x++)
            {
                if (_dots[x, row])
                {
                    firstSetBitIndex = x;
                    break;
                }
            }

            if (firstSetBitIndex == null) { return 0f; }
            var mantissaLowBitIndex = Math.Min(firstSetBitIndex.Value + 24, 276);
            var isNormal = mantissaLowBitIndex - firstSetBitIndex == 24;

            int mantissa = 0;
            if (isNormal)
            {
                for (var b = 0; b < 24; b++)
                {
                    mantissa |= (_dots[firstSetBitIndex.Value + b, row] ? 1 : 0) << (23 - b);
                }
            }
            else
            {
                for (var b = 0; b < mantissaLowBitIndex - firstSetBitIndex.Value; b++)
                {
                    mantissa |= (_dots[firstSetBitIndex.Value + b, row] ? 1 : 0) << (23 - b);
                }
            }

            var unbiasedExponent = isNormal ? BitIndexToExponent(firstSetBitIndex.Value) : -126;
            var exponent = unbiasedExponent + 127;
            var intRepresentation = (exponent << 23) | (mantissa & 0x007F_FFFF);
            return BitConverter.Int32BitsToSingle(intRepresentation);
        }

        private void WriteFloatToRow(float value, int row, out int resultExponent)
        {
            for (var i = 0; i < 277; i++)
            {
                _dots[i, row] = false;
            }

            var intRepresentation = BitConverter.SingleToInt32Bits(value);
            var exponent = (intRepresentation >> 23) & 0xFF;
            var unbiasedExponent = exponent - 127;
            var mantissa = intRepresentation & 0x007F_FFFF;
            var isNormal = exponent != 0;
            var firstSetBitIndex = isNormal ? ExponentToBitIndex(unbiasedExponent) : ExponentToBitIndex(-126);
            if (isNormal) { mantissa |= 0x0080_0000; }
            for (var b = 0; b < 24; b++)
            {
                var bitValue = (mantissa >> (23 - b)) & 1;
                _dots[firstSetBitIndex + b, row] = bitValue == 1;
            }
            resultExponent = unbiasedExponent;
        }

        private float GetNextAddend()
        {
            // We want to generate random addends such that it takes about SecondsBetweenDoublingsTarget
            // seconds for the accumulator to have the window move left one bit. Since we update every
            // FramesBetweenUpdates frames, we need to figure out the rough updates per desired doubling.
            var updatesPerDoubling = SecondsBetweenDoublingsTarget * AnimationContext.SecondsToFrames(1d) / FramesBetweenUpdates;
            var accumulator = BuildFloatFromRow(0);
            var addendRequiredToDouble = (accumulator == 0) ? 1 : accumulator;
            // Fixes an issue where the initial addends are pretty big but when we make the accumulator
            // non-zero, we start generating much smaller addends that take a bit to scroll up. This
            // ensures the addends don't get smaller for no real reason.
            addendRequiredToDouble = (float)Math.Max(1d, addendRequiredToDouble); 
            var addendPerUpdate = addendRequiredToDouble / (float)updatesPerDoubling;

            // Now make it look more interesting by randomly adding or subtracting up to 5% from it.
            var randomFactor = 1d + (_random.NextDouble() * 0.1d - 0.05d);
            return addendPerUpdate * (float)randomFactor;
        }
    }
}
