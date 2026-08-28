using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public readonly record struct XRange
{
    public BigInteger Minimum { get; }
    public BigInteger Maximum { get; }

    public XRange(BigInteger minimum, BigInteger maximum)
    {
        if (minimum > maximum)
        {
            throw new ArgumentException($"Minimum value {minimum} cannot be greater than maximum value {maximum}.");
        }

        Minimum = minimum;
        Maximum = maximum;
    }

    public IEnumerable<BigInteger> Sample(BigInteger requestedCount)
    {
        if (requestedCount < 0)
        {
            yield break;
        }

        var cardinality = (Maximum - Minimum) + 1;
        var sampleCount = BigInteger.Min(requestedCount, cardinality);

        if (sampleCount == BigInteger.One)
        {
            yield return (Minimum + Maximum) / 2;
            yield break;
        }

        var span = Maximum - Minimum;

        for (var i = BigInteger.Zero; i < sampleCount; i++)
        {
            var offset = (i * span) / (sampleCount - BigInteger.One);
            yield return Minimum + offset;
        }
    }
}
