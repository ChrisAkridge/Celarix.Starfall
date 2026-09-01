using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public abstract class DataSourceBase : IDataSource
{
    public event EventHandler? DataChanged;
    private readonly IResolutionStrategy _resolutionStrategy;

    protected DataSourceBase(IResolutionStrategy resolutionStrategy)
    {
        _resolutionStrategy = resolutionStrategy;
    }

    public ResolvedDataPoint ResolveBucket(XRange range)
    {
        var observation = GetObservation(range);

        return _resolutionStrategy.Resolve(observation);
    }

    protected abstract BucketObservation GetObservation(XRange bucket);

    protected void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}
