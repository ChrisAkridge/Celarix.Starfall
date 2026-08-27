using Celarix.Starfall.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public readonly struct DataDistributionBucket
{
    public readonly double LowerBound { get; }
    public readonly double BucketSize { get; }
    public readonly double UpperBound => LowerBound + BucketSize;

    public readonly int Count { get; }

    public DataDistributionBucket(double lowerBound, double bucketSize, int count)
    {
        bucketSize.ThrowIfNotPositive(nameof(bucketSize));
        count.ThrowIfNotPositive(nameof(count));

        LowerBound = lowerBound;
        BucketSize = bucketSize;
        Count = count;
    }
}
