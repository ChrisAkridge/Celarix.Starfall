using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public abstract record ResolvedDataPoint
{
    public required XRange Range { get; init; }
}

public sealed record EmptyDataPoint : ResolvedDataPoint;

public sealed record IndividualDataPoint : ResolvedDataPoint
{
    public BigInteger X => Range.Minimum;
    public required double Y { get; init; }
}

public sealed record AggregatedDataPoint : ResolvedDataPoint
{
    public required BigInteger Count { get; init; }

    public required double FirstY { get; init; }
    public required double LastY { get; init; }

    public required double MinimumY { get; init; }
    public required double MaximumY { get; init; }

    public required double SumY { get; init; }
    public required double AverageY { get; init; }
}
