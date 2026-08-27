using Celarix.Starfall.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts;

public sealed class DataDistribution : IEnumerable<DataDistributionBucket>
{
    public event EventHandler? DistributionChanged;

    private readonly DataSeries _series;
    private readonly SortedDictionary<int, int> _buckets;
    private double _bucketSize;

    public double BucketSize
    {
        get => _bucketSize;
        set
        {
            value.ThrowIfNotPositive();
            if (_bucketSize != value)
            {
                _bucketSize = value;
                DistributionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public DataDistribution(DataSeries series, double bucketSize)
    {
        _series = series;
        _bucketSize = bucketSize;
        _buckets = new SortedDictionary<int, int>();
        _series.DataChanged += OnDataChanged;
        RecalculateDistribution();
    }

    public DataDistributionBucket GetBucket(int bucketIndex)
    {
        if (_buckets.TryGetValue(bucketIndex, out int count))
        {
            return new DataDistributionBucket(bucketIndex * _bucketSize, _bucketSize, count);
        }
        return new DataDistributionBucket(bucketIndex * _bucketSize, _bucketSize, 0);
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        RecalculateDistribution();
        DistributionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RecalculateDistribution()
    {
        _buckets.Clear();
        foreach (var point in _series)
        {
            int bucketIndex = (int)Math.Floor(point.Value / _bucketSize);
            if (_buckets.TryGetValue(bucketIndex, out int value))
            {
                _buckets[bucketIndex] = ++value;
            }
            else
            {
                _buckets[bucketIndex] = 1;
            }
        }
    }

    public IEnumerator<DataDistributionBucket> GetEnumerator()
    {
        foreach (var kvp in _buckets)
        {
            // I 💖 sorted dictionaries
            yield return new DataDistributionBucket(kvp.Key * _bucketSize, _bucketSize, kvp.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
