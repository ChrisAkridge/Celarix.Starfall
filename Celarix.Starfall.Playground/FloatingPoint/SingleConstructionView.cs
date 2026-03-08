using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Playground.FloatingPoint
{
    public sealed class SingleConstructionView
    {
        // This is a way to visualize the construction of a IEEE-754 single-precision floating point number.
        // The float is visualized as a 24-bit (or 23-bit) window into a range of 277 bits. The 277 bits
        // represent the actual number being constructed, with the window representing which bits the mantissa
        // actually can use. The window is positioned based on the exponent of the target value, and
        // the bits are set based on the mantissa of the target value.

        // The exponent range is from -126 to +127. The bits are laid out left-to-right, with +127
        // on the left. This leads to 127 bits to the left of the binary point, and 126 bits to the
        // right, with an extra bit for the 2^0 place. At an exponent of -126, we still have 23 bits
        // of subnormal mantissa to work with, so that takes us from (127 + 1 + 126) = 254 bits to
        // (254 + 23) = 277 bits total.
        private const int TotalBits = 277;
        private const int ExponentOfFirstBit = 127;
        private const int ExponentOfLastBit = (-126 - SubnormalWindowWidth);
        private const int BinaryPointAfterIndex = 127;
        private const int NormalWindowWidth = 24;
        private const int SubnormalWindowWidth = 23;

        private BitArray bits;
        private float targetValue;
        private int currentBitIndex;
        private int windowWidth;
        private int windowLeftIndex;

        private int WindowRightIndex => windowLeftIndex + windowWidth - 1;
        private float CurrentPlaceValue => (float)Math.Pow(2, BinaryPointAfterIndex - currentBitIndex);
        private float CurrentValue
        {
            get
            {
                float value = 0;
                for (int i = windowLeftIndex; i < (windowLeftIndex + windowWidth); i++)
                {
                    if (bits[i])
                    {
                        value += (float)Math.Pow(2, BinaryPointAfterIndex - i);
                    }
                }
                return value;
            }
        }

        public SingleConstructionView(float targetValue)
        {
            if (targetValue < 0 || float.IsNaN(targetValue) || float.IsInfinity(targetValue))
            {
                throw new ArgumentException("Only non-negative finite values are supported.");
            }

            bits = new BitArray(TotalBits);
            this.targetValue = targetValue;
            
            var targetAsInt = BitConverter.SingleToInt32Bits(targetValue);
            var exponentPart = (targetAsInt >> 23) & 0xFF;
            var mantissaPart = targetAsInt & 0x7FFFFF;
            var unbiasedExponent = exponentPart - ExponentOfFirstBit;
            var isSubnormal = exponentPart == 0;
            windowWidth = isSubnormal ? SubnormalWindowWidth : NormalWindowWidth;

            if (!isSubnormal)
            {
                // The unbiased exponent determines the leftmost bit of the mantissa, which is where
                // the left edge of the window will be.
                windowLeftIndex = ExponentOfFirstBit - unbiasedExponent;

                // Normal numbers have an implicit leading 1, so we know we start building the value from that bit.
                currentBitIndex = windowLeftIndex;
            }
            else
            {
                // The window is always right up to the end of the bits when subnormal.
                windowLeftIndex = TotalBits - 23; // 23 bits for the mantissa, so the left edge is at TotalBits - 23

                // Subnormal numbers don't have an implicit leading 1, so we start building the value from the highest set bit
                // in the mantissa.
                currentBitIndex = (TotalBits - 1) - (mantissaPart as IBinaryInteger<int>).GetShortestBitLength();
            }
        }

        public bool MoveNext()
        {
            if (CurrentValue == targetValue)
            {
                return false;
            }

            var targetAsInt = BitConverter.SingleToInt32Bits(targetValue);
            var mantissaPart = targetAsInt & 0x7FFFFF;
            var exponentPart = (targetAsInt >> 23) & 0xFF;
            if (exponentPart > 0)
            {
                mantissaPart |= 1 << 23; // Add the implicit leading 1 for normal numbers.
            }
            var currentBitInWindow = WindowRightIndex - currentBitIndex;
            var bitSetInMantissa = (mantissaPart & (1 << currentBitInWindow)) != 0;

            if (bitSetInMantissa)
            {
                bits[currentBitIndex] = true;
            }

            currentBitIndex++;
            return true;
        }

        public FloatingPointWindowElement MakeWindowElement()
        {
            var element = new FloatingPointWindowElement(TotalBits, windowWidth, 2, ExponentOfFirstBit, BinaryPointAfterIndex);
            for (int i = 0; i < TotalBits; i++)
            {
                element.SetDigit(i, bits[i] ? '1' : '0');
            }
            element.WindowLeftIndex = windowLeftIndex;
            element.SelectedIndex = currentBitIndex;
            return element;
        }
    }
}
