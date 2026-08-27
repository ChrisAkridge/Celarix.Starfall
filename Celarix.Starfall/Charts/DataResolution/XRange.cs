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
}
