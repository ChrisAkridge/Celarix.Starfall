using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Stats;

public sealed class DoubleStatsInfo
{
    private double[] _data;
    private double[] _sortedData;

    public double CurrentValue => _data.Last();
    public double MinimumValue => _data.Min();
    public double MaximumValue => _data.Max();
    public double Range => MaximumValue - MinimumValue;
    public double Midpoint => (MaximumValue + MinimumValue) / 2;

    public double Mean { get; }
    public double Median { get; }
    public double Mode { get; }
    public double StandardDeviation { get; }

    public DoubleStatsInfo(IEnumerable<double> data)
    {
        _data = [.. data];
        _sortedData = [.. _data];
        Array.Sort(_sortedData);

        if (_data.Length == 0)
        {
            throw new ArgumentException("Data cannot be empty.", nameof(data));
        }

        if (_data.Any(d => double.IsInfinity(d) || double.IsNaN(d)))
        {
            throw new ArgumentException("Data cannot contain infinity or NaN values.", nameof(data));
        }

        var length = _data.Length;
        var sum = _data.Sum();
        Mean = sum / _data.Length;

        if (length % 2 == 0)
        {
            Median = (_sortedData[length / 2 - 1] + _sortedData[length / 2]) / 2;
        }
        else
        {
            Median = _sortedData[length / 2];
        }

        var modeGroups = _data.GroupBy(d => d)
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();
        Mode = modeGroups.First().Value;

        StandardDeviation = ComputeStandardDeviation();
    }

    public string[] GetDisplayText()
    {
        return
        [
            $"Current: {CurrentValue:F2}",
            $"Min: {MinimumValue:F2}",
            $"Max: {MaximumValue:F2}",
            $"Range: {Range:F2}",
            $"Midpoint: {Midpoint:F2}",
            $"Mean: {Mean:F2}",
            $"Median: {Median:F2}",
            $"Mode: {Mode:F2}",
            $"Standard Deviation: {StandardDeviation:F2}"
        ];
    }

    private double ComputeStandardDeviation()
    {
        var mean = Mean;
        var sumOfSquares = _data.Sum(d => Math.Pow(d - mean, 2));
        return Math.Sqrt(sumOfSquares / _data.Length);
    }
}
