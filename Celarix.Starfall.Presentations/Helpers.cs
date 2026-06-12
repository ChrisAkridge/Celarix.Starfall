using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Presentations
{
    internal static class Helpers
    {
        // Source - https://stackoverflow.com/a/73779387
        // Posted by user17789309
        // Retrieved 2026-06-12, License - CC BY-SA 4.0
        public static string ExactStringSingle(float value)
        {
            const int valueBits = sizeof(float) * 8;
            const int fractionBits = 23; // excludes implicit leading 1 in normal values

            const int exponentBits = valueBits - fractionBits - 1;
            const uint signMask = 1U << (valueBits - 1);
            const uint fractionMask = (1U << fractionBits) - 1;

            var bits = BitConverter.SingleToUInt32Bits(value);
            var result = new StringBuilder();

            if ((bits & signMask) != 0) { result.Append('-'); }

            var biasedExponent = (int)((bits & ~signMask) >> fractionBits);
            var fraction = bits & fractionMask;

            // Maximum possible value of the biased exponent: infinities and NaNs
            const int maxExponent = (1 << exponentBits) - 1;

            if (biasedExponent == maxExponent)
            {
                if (fraction == 0)
                {
                    result.Append("inf");
                }
                else
                {
                    // NaN type is stored in the most significant bit of the fraction
                    const uint nanTypeMask = 1U << (fractionBits - 1);
                    // NaN payload
                    const uint nanPayloadMask = nanTypeMask - 1;
                    // NaN type, valid for x86, x86-64, 68000, ARM, SPARC
                    var isQuiet = (fraction & nanTypeMask) != 0;
                    var nanPayload = fraction & nanPayloadMask;
                    result.Append(isQuiet
                        ? FormattableString.Invariant($"qNaN(0x{nanPayload:x})")
                        : FormattableString.Invariant($"sNaN(0x{nanPayload:x})"));
                }

                return result.ToString();
            }

            // Minimum value of biased exponent above which no fractional part will exist
            const int noFractionThreshold = (1 << (exponentBits - 1)) + fractionBits - 1;

            if (biasedExponent == 0)
            {
                // zeroes and subnormal numbers
                // shift for the denominator of the rational part of a subnormal number
                const int denormalDenominatorShift = noFractionThreshold - 1;
                WriteRational(fraction, BigInteger.One << denormalDenominatorShift, result);
                return result.ToString();
            }

            // implicit leading one in the fraction part
            const uint implicitLeadingOne = 1U << fractionBits;
            var numerator = (BigInteger)(fraction | implicitLeadingOne);
            if (biasedExponent >= noFractionThreshold)
            {
                numerator <<= biasedExponent - noFractionThreshold;
                result.Append(numerator.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                var denominator = BigInteger.One << (noFractionThreshold - (int)biasedExponent);
                WriteRational(numerator, denominator, result);
            }

            return result.ToString();
        }

        static void WriteRational(BigInteger numerator, BigInteger denominator, StringBuilder result)
        {
            // precondition: denominator contains only factors of 2 and 5
            var intPart = BigInteger.DivRem(numerator, denominator, out numerator);
            result.Append(intPart.ToString(CultureInfo.InvariantCulture));
            if (numerator.IsZero) { return; }
            result.Append('.');
            do
            {
                numerator *= 10;
                var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
                denominator /= gcd;
                intPart = BigInteger.DivRem(numerator / gcd, denominator, out numerator);
                result.Append(intPart.ToString(CultureInfo.InvariantCulture));
            } while (!numerator.IsZero);
        }
    }
}
