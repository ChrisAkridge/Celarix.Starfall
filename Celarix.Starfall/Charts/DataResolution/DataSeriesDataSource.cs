using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class DataSeriesDataSource : DataSourceBase
{
    private readonly DataSeries _series;

    public DataSeriesDataSource(
        DataSeries series,
        IResolutionStrategy resolutionStrategy)
        : base(resolutionStrategy)
    {
        _series = series;
    }

    protected override BucketObservation GetObservation(XRange bucket)
    {
        var observations = _series.GetPointsInRange(bucket);
        return new BucketObservation(bucket, observations);
    }
}
