using Celarix.Starfall.Charts.Models;
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

    public override InfoPanelData GetInfoPanelData(IEnumerable<decimal> percentiles)
    {
        const double nan = double.NaN;
        if (_series.Count == 0)
        {
            return new InfoPanelData(nan, nan, nan, nan, nan, nan, nan, nan, nan, nan, null, BigInteger.Zero, 0d);
        }

        var percentilesDictionary = new Dictionary<decimal, double>();

        var firstX = _series.FirstX!.Value;
        var lastX = _series.LastX!.Value;

        var currentValue = _series[lastX].Y!.Value;
        var minimum = _series.Min!.Value;
        var maximum = _series.Max!.Value;
        var range = maximum - minimum;
        var midpoint = (minimum + maximum) / 2d;
        var mean = _series.Mean;
        var median = _series.Median;
        var mode = _series.Mode;
        var populationStandardDeviation = _series.PopulationStandardDeviation;
        var sampleStandardDeviation = _series.SampleStandardDeviation;
        var sum = _series.Sum!.Value;

        foreach (var percentile in percentiles)
        {
            percentilesDictionary[percentile] = _series.NthPercentile((double)percentile);
        }
        var percentilesList = percentilesDictionary.Select(kvp => new InfoPanelPercentileData(kvp.Key, kvp.Value)).ToList();

        return new InfoPanelData(currentValue, minimum, maximum, range, midpoint, mean, median, mode,
            populationStandardDeviation, sampleStandardDeviation ?? double.NaN, percentilesList, _series.Count, sum);
    }
}
