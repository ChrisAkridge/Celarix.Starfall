using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public static class DataResolver
{
    public static IReadOnlyList<ResolvedDataPoint> Resolve(
        IDataSource dataSource,
        XRange xRange,
        int bucketCount)
    {
        ArgumentNullException.ThrowIfNull(dataSource, nameof(dataSource));

        return [.. CreateBuckets(xRange, bucketCount).Select(b => dataSource.ResolveBucket(b))];
    }

    private static IEnumerable<XRange> CreateBuckets(XRange xRange, int bucketCount)
    {
        var cardinality = (xRange.Maximum - xRange.Minimum) + 1;
        var trueBucketCount = BigInteger.Min(cardinality, bucketCount);

        for (var i = BigInteger.Zero; i < bucketCount; i++)
        {
            var startOffset = i * cardinality / trueBucketCount;
            var endOffset = ((i + 1) * cardinality / trueBucketCount) - 1;
            var start = xRange.Minimum + startOffset;
            var end = xRange.Minimum + endOffset;
            yield return new XRange(start, end);
        }
    }
}
