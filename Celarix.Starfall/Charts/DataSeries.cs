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

    private Dictionary<BigInteger, double?> _data;

    // Statistical properties. Keep them as fields so recalculating them every time isn't necessary.
    private SortedDictionary<double, int> _sortedValues;
    private int _count;
    private double? _min;
    private double? _max;
    private double? _sum;
    private double? _sumOfSquares;

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
    public double? SumOfSquares => _sumOfSquares;

    public double Mean
    {
        get
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            return _sum!.Value / Count;
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

            var mean = Mean;
            return (_sumOfSquares!.Value / Count) - (mean * mean);
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

            var sum = _sum!.Value;

            return (_sumOfSquares!.Value - (sum * sum / Count))
                / (Count - 1);
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
        _sumOfSquares = null;
        foreach (var point in data)
        {
            AddPointImpl(point);
        }
    }

    public void AddPoint(DataSeriesPoint point)
    {
        AddPointImpl(point);
        OnDataChanged();
    }

    private void AddPointImpl(DataSeriesPoint point)
    {
        if (!_data.TryAdd(point.X, point.Y))
        {
            throw new ArgumentException($"A point with X={point.X} already exists in the series.");
        }

        // Keep the statistical properties up to date.
        if (point.Y != null)
        {
            if (_sortedValues.TryGetValue(point.Y.Value, out int value))
            {
                _sortedValues[point.Y.Value] = ++value;
            }
            else
            {
                _sortedValues[point.Y.Value] = 1;
            }
            _count += 1;
            _min = Math.Min(_min ??= point.Y.Value, point.Y.Value);
            _max = Math.Max(_max ??= point.Y.Value, point.Y.Value);
            _sum = (_sum ?? 0) + point.Y.Value;
            _sumOfSquares = (_sumOfSquares ?? 0) + point.Y.Value * point.Y.Value;
        }
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
                _count -= 1;
                _sum -= y.Value;
                _sumOfSquares -= y.Value * y.Value;
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
                    _sumOfSquares = null;
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
        var result = new List<DataPoint>();
        for (var i = range.Minimum; i <= range.Maximum; i++)
        {
            if (_data.TryGetValue(i, out double? y) && y.HasValue)
            {
                result.Add(new DataPoint { X = i, Y = y.Value });
            }
        }
        return result;
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
