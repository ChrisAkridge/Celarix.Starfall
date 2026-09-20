using Celarix.Starfall.Mathematics;
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

    public IndividualDataPoint Ease(IndividualDataPoint other, Easing easing, double progress)
    {
        var easedProgress = easing(progress);
        var newY = Y + (other.Y - Y) * easedProgress;
        return new IndividualDataPoint
        {
            Range = new XRange(Range.Minimum, other.Range.Maximum),
            Y = newY
        };
    }
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

    public AggregatedDataPoint Ease(AggregatedDataPoint other, Easing easing, double progress)
    {
        var easedProgress = easing(progress);
        var newCount = (BigInteger)Math.Round((double)Count + (double)(other.Count - Count) * easedProgress);
        var newFirstY = FirstY + (other.FirstY - FirstY) * easedProgress;
        var newLastY = LastY + (other.LastY - LastY) * easedProgress;
        var newMinimumY = MinimumY + (other.MinimumY - MinimumY) * easedProgress;
        var newMaximumY = MaximumY + (other.MaximumY - MaximumY) * easedProgress;
        var newSumY = SumY + (other.SumY - SumY) * easedProgress;
        var newAverageY = AverageY + (other.AverageY - AverageY) * easedProgress;

        return new AggregatedDataPoint
        {
            Range = new XRange(Range.Minimum, other.Range.Maximum),
            Count = newCount,
            FirstY = newFirstY,
            LastY = newLastY,
            MinimumY = newMinimumY,
            MaximumY = newMaximumY,
            SumY = newSumY,
            AverageY = newAverageY
        };
    }
}
