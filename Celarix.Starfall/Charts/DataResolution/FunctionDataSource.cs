using System;

using System.Collections.Generic;

using System.Numerics;

using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class FunctionDataSource : DataSourceBase
{
    private readonly Func<BigInteger, double> _function;
    private readonly int _samplesPerBucket;

    public FunctionDataSource(Func<BigInteger, double> function,
        int samplesPerBucket,
        IResolutionStrategy resolutionStrategy) : base(resolutionStrategy)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(samplesPerBucket);

        _function = function;
        _samplesPerBucket = samplesPerBucket;
    }

    protected override BucketObservation GetObservation(XRange bucket)
    {
        var xValues = bucket.Sample(_samplesPerBucket);
        var points = xValues.Select(x => new DataPoint(x, _function(x))).ToArray();
        return new BucketObservation(bucket, points);
    }
}
