using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public sealed class DataSeries : IEnumerable<KeyValuePair<BigInteger, double>>
{
    public event EventHandler? DataChanged;
    private bool _suppressEvents;

    private SortedDictionary<BigInteger, double?> _data;
    private bool _warnedAboutNonFiniteValue;

    // Statistical properties. Keep them as fields so recalculating them every time isn't necessary.
    private SortedDictionary<double, int> _sortedValues;
    private int _count;
    private double? _min;
    private double? _max;
    private double? _sum;
    private double _mean;
    private double _m2;

    // Some properties are pretty cheap, though.
    private double? Range => _max - _min;
    private double? Midpoint
    {
        get
        {
            if (_min is null || _max is null)
            {
                return null;
            }
            return (_min + _max) / 2;
        }
    }

    // Collection properties.
    public DataSeriesPoint this[BigInteger x]
    {
        get
        {
            if (_data.TryGetValue(x, out double? y))
            {
                return new DataSeriesPoint(x, y);
            }
            return new DataSeriesPoint(x, null);
        }
    }

    public int Count => _count;
    public int PointCount => _data.Count;
    public double? Min => _min;
    public double? Max => _max;
    public double? Sum => _sum;
    public double? SumOfSquares => _count == 0 ? null : _m2 + (_count * _mean * _mean);
    public double? M2 => _count == 0 ? null : _m2;

    public double Mean
    {
        get
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            return _mean;
        }
    }

    public double Median => NthPercentile(50);

    public double Mode => _sortedValues.OrderByDescending(kvp => kvp.Value).First().Key;

    public double PopulationVariance
    {
        get
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            return _m2 / Count;
        }
    }

    public double PopulationStandardDeviation =>
        Math.Sqrt(Math.Max(0, PopulationVariance));

    public double SampleVariance
    {
        get
        {
            if (Count < 2)
            {
                throw new InvalidOperationException(
                    "Sample variance requires at least two values.");
            }

            return _m2 / (Count - 1);
        }
    }

    public DataSeries(IEnumerable<DataSeriesPoint> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data = [];
        _sortedValues = [];
        _count = 0;
        _min = null;
        _max = null;
        _sum = null;
        foreach (var point in data)
        {
            if (!_data.TryAdd(point.X, Normalize(point.X, point.Y)))
            {
                throw new ArgumentException($"A point with X={point.X} already exists in the series.");
            }
        }
        RebuildStatistics();
    }

    public void AddPoint(DataSeriesPoint point)
    {
        AddPointImpl(point);
        OnDataChanged();
    }

    private void AddPointImpl(DataSeriesPoint point)
    {
        var normalizedY = Normalize(point.X, point.Y);
        if (!_data.TryAdd(point.X, normalizedY))
        {
            throw new ArgumentException($"A point with X={point.X} already exists in the series.");
        }

        // Keep the statistical properties up to date.
        if (normalizedY != null)
        {
            var y = normalizedY.Value;
            if (_sortedValues.TryGetValue(y, out int value))
            {
                _sortedValues[y] = ++value;
            }
            else
            {
                _sortedValues[y] = 1;
            }
            _count++;
            var delta = y - _mean;
            _mean += delta / _count;
            _m2 += delta * (y - _mean);
            _min = Math.Min(_min ??= y, y);
            _max = Math.Max(_max ??= y, y);
            _sum = (_sum ?? 0) + y;
        }
    }

    private double? Normalize(BigInteger x, double? y)
    {
        if (y is null || double.IsFinite(y.Value)) return y;
        if (!_warnedAboutNonFiniteValue)
        {
            Console.WriteLine($"Chart data series normalized non-finite value {y.Value} at X={x} to zero.");
            _warnedAboutNonFiniteValue = true;
        }
        return 0d;
    }

    public bool TryRemovePoint(BigInteger x)
    {
        if (_data.TryGetValue(x, out double? y))
        {
            _data.Remove(x);
            // Keep the statistical properties up to date.
            if (y != null)
            {
                if (_sortedValues.TryGetValue(y.Value, out int value))
                {
                    if (value == 1)
                    {
                        _sortedValues.Remove(y.Value);
                    }
                    else
                    {
                        _sortedValues[y.Value] = --value;
                    }
                }
                var oldCount = _count;
                _count--;
                _sum -= y.Value;
                if (_count == 0)
                {
                    _mean = 0d;
                    _m2 = 0d;
                }
                else
                {
                    var oldMean = _mean;
                    _mean = ((oldCount * oldMean) - y.Value) / _count;
                    _m2 -= (y.Value - oldMean) * (y.Value - _mean);
                    if (_m2 < 0d)
                    {
                        var tolerance = 1e-12 * Math.Max(1d, Math.Abs(oldMean));
                        if (_m2 >= -tolerance) _m2 = 0d;
                        else RebuildStatistics();
                    }
                }
                // Recalculate min and max if necessary.
                if (y == _min || y == _max)
                {
                    if (_sortedValues.Count > 0)
                    {
                        _min = _sortedValues.Keys.Min();
                        _max = _sortedValues.Keys.Max();
                    }
                    else
                    {
                        _min = null;
                        _max = null;
                    }
                }

                if (_count == 0)
                {
                    _min = null;
                    _max = null;
                    _sum = null;
                    _mean = 0d;
                    _m2 = 0d;
                }
            }
            OnDataChanged();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RemovePoint(BigInteger x)
    {
        if (!_data.ContainsKey(x))
        {
            throw new ArgumentException($"No point with X={x} exists in the series.");
        }
        return TryRemovePoint(x);
    }

    public void SetPoint(BigInteger x, double? y)
    {
        if (_data.ContainsKey(x))
        {
            // Only fire the DataChanged event once.
            _suppressEvents = true;
            try
            {
                TryRemovePoint(x);
                AddPointImpl(new DataSeriesPoint(x, y));
            }
            finally
            {
                _suppressEvents = false;
                OnDataChanged();
            }
        }
        else
        {
            AddPoint(new DataSeriesPoint(x, y));
        }
    }

    public bool ValueExistsAtX(BigInteger x)
    {
        return _data.ContainsKey(x);
    }

    public IReadOnlyList<DataPoint> GetPointsInRange(XRange range)
    {
        return [.. _data
            .Where(kvp => kvp.Key >= range.Minimum && kvp.Key <= range.Maximum && kvp.Value.HasValue)
            .Select(kvp => new DataPoint(kvp.Key, kvp.Value!.Value))];
    }

    public void RecalculateStatistics() => RebuildStatistics();

    private void RebuildStatistics()
    {
        _sortedValues.Clear();
        _count = 0;
        _min = null;
        _max = null;
        _sum = null;
        _mean = 0d;
        _m2 = 0d;
        foreach (var y in _data.Values.Where(v => v.HasValue).Select(v => v!.Value))
        {
            _sortedValues[y] = _sortedValues.GetValueOrDefault(y) + 1;
            _count++;
            var delta = y - _mean;
            _mean += delta / _count;
            _m2 += delta * (y - _mean);
            _sum = (_sum ?? 0d) + y;
            _min = Math.Min(_min ?? y, y);
            _max = Math.Max(_max ?? y, y);
        }
    }

    public (double Lower, double Upper) GetPopulationSigmaRange(double sigma)
    {
        var amount = PopulationStandardDeviation * sigma;
        return (Mean - amount, Mean + amount);
    }

    public (double Lower, double Upper) GetSampleSigmaRange(double sigma)
    {
        var amount = Math.Sqrt(SampleVariance) * sigma;
        return (Mean - amount, Mean + amount);
    }

    public double NthPercentile(double n)
    {
        if (n < 0 || n > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Percentile must be between 0 and 100.");
        }
        if (Count == 0)
        {
            throw new InvalidOperationException("The collection is empty.");
        }
        double rank = (n / 100) * (Count - 1);
        int lowerIndex = (int)Math.Floor(rank);
        int upperIndex = (int)Math.Ceiling(rank);
        if (lowerIndex == upperIndex)
        {
            return GetValueAtSortedIndex(lowerIndex);
        }
        else
        {
            double lowerValue = GetValueAtSortedIndex(lowerIndex);
            double upperValue = GetValueAtSortedIndex(upperIndex);
            return lowerValue + (upperValue - lowerValue) * (rank - lowerIndex);
        }
    }

    private double GetValueAtSortedIndex(int index)
    {
        if (index >= Count || index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        var seenValues = 0;
        foreach (var kvp in _sortedValues)
        {
            seenValues += kvp.Value;
            if (seenValues > index)
            {
                return kvp.Key;
            }
        }
        throw new InvalidOperationException("Index not found.");
    }

    public IEnumerator<KeyValuePair<BigInteger, double>> GetEnumerator()
    {
        var sortedData = _data
            .Where(kvp => kvp.Value.HasValue)
            .OrderBy(kvp => kvp.Key);
        foreach (var kvp in sortedData)
        {
            yield return new KeyValuePair<BigInteger, double>(kvp.Key, kvp.Value!.Value);
        }
    }

    private void OnDataChanged()
    {
        if (!_suppressEvents)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
