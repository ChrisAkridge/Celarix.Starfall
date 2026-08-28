using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class StandardResolutionStrategy : IResolutionStrategy
{
    public ResolvedDataPoint Resolve(BucketObservation bucket)
    {
        if (bucket.Points.Count == 0)
        {
            return new EmptyDataPoint
            {
                Range = bucket.Range
            };
        }
        else if (bucket.Points.Count == 1)
        {
            return new IndividualDataPoint
            {
                Range = bucket.Range,
                Y = bucket.Points[0].Y,
            };
        }

        BigInteger count = bucket.Points.Count;
        var firstY = bucket.Points[0].Y;
        var lastY = bucket.Points[bucket.Points.Count - 1].Y;

        double minY = double.PositiveInfinity;
        double maxY = double.NegativeInfinity;
        double sumY = 0;

        for (var i = 0; i < bucket.Points.Count; i++)
        {
            var point = bucket.Points[i];
            if (point.Y < minY)
            {
                minY = point.Y;
            }
            if (point.Y > maxY)
            {
                maxY = point.Y;
            }
            sumY += point.Y;
        }

        double averageY = sumY / (double)count;

        return new AggregatedDataPoint
        {
            Range = bucket.Range,
            MinimumY = minY,
            MaximumY = maxY,
            AverageY = averageY,
            FirstY = firstY,
            LastY = lastY,
            Count = count,
            SumY = sumY,
        };
    }
}
