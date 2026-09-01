using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class DataSeriesDataSource : DataSourceBase
{
    private readonly DataSeries _series;
    private bool _connected;

    public DataSeriesDataSource(
        DataSeries series,
        IResolutionStrategy resolutionStrategy)
        : base(resolutionStrategy)
    {
        _series = series;
        Connect();
    }

    public void Connect()
    {
        if (_connected) return;
        _series.DataChanged += Series_DataChanged;
        _connected = true;
    }

    public void Disconnect()
    {
        if (!_connected) return;
        _series.DataChanged -= Series_DataChanged;
        _connected = false;
    }

    private void Series_DataChanged(object? sender, EventArgs e) => OnDataChanged();

    protected override BucketObservation GetObservation(XRange bucket)
    {
        var observations = _series.GetPointsInRange(bucket);
        return new BucketObservation(bucket, observations);
    }
}
