using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System.Numerics;

namespace Celarix.Starfall.Charts;

/// <summary>
/// Maintains a value-anchored, measured label lattice for an integral axis.
/// Materialized Libra layouts and the selected stride survive viewport translations.
/// </summary>
public sealed class IntegralAxisLabelLayout
{
    private const int MaximumDensityProbeLabels = 2048;
    private const int MaximumCachedLayouts = 8192;

    private readonly Dictionary<BigInteger, LibraLayoutResult> _layoutCache = [];
    private HashSet<BigInteger> _previousValues = [];
    private BigInteger? _stride;

    public BigInteger? Stride => _stride;
    public int CachedLayoutCount => _layoutCache.Count;

    public void InvalidateMeasurements()
    {
        _layoutCache.Clear();
        InvalidateDensity();
    }

    public void InvalidateDensity()
    {
        _stride = null;
        _previousValues.Clear();
    }

    public IReadOnlyList<FittedAxisLabel<BigInteger>> Update(
        XRange visibleRange,
        Func<BigInteger, LibraLayoutResult> labelFactory,
        Func<BigInteger, SRectF> getSlotBounds,
        Side axisSide,
        double minorAxisMargin,
        double labelFitExtentMultiplier,
        bool recomputeDensity)
    {
        ArgumentNullException.ThrowIfNull(labelFactory);
        ArgumentNullException.ThrowIfNull(getSlotBounds);
        if (!double.IsFinite(labelFitExtentMultiplier) || labelFitExtentMultiplier < 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(labelFitExtentMultiplier));
        }

        if (recomputeDensity || _stride is null)
        {
            _stride = FindStride(visibleRange, labelFactory, getSlotBounds, axisSide,
                minorAxisMargin, labelFitExtentMultiplier);
            _previousValues.Clear();
        }

        var candidates = BuildCandidates(visibleRange, _stride.Value, labelFactory,
            getSlotBounds, axisSide, minorAxisMargin);
        IReadOnlyList<FittedAxisLabel<BigInteger>> result;
        if (_previousValues.Count == 0 || recomputeDensity)
        {
            result = candidates;
        }
        else
        {
            result = PreserveRetainedLabels(candidates, axisSide, labelFitExtentMultiplier);
        }

        _previousValues = [.. result.Select(label => label.Value)];
        PruneCache(_previousValues);
        return result;
    }

    private BigInteger FindStride(
        XRange range,
        Func<BigInteger, LibraLayoutResult> labelFactory,
        Func<BigInteger, SRectF> getSlotBounds,
        Side axisSide,
        double minorAxisMargin,
        double multiplier)
    {
        var cardinality = range.Range + 1;
        var stride = BigInteger.Max(BigInteger.One,
            CeilingDivide(cardinality, MaximumDensityProbeLabels));

        while (true)
        {
            var candidates = BuildCandidates(range, stride, labelFactory, getSlotBounds,
                axisSide, minorAxisMargin);
            if (!HasIntersections(candidates, axisSide, multiplier) || candidates.Count <= 1)
            {
                return stride;
            }
            stride *= 2;
        }
    }

    private List<FittedAxisLabel<BigInteger>> BuildCandidates(
        XRange range,
        BigInteger stride,
        Func<BigInteger, LibraLayoutResult> labelFactory,
        Func<BigInteger, SRectF> getSlotBounds,
        Side axisSide,
        double minorAxisMargin)
    {
        var firstVisible = CeilingToMultiple(range.Minimum, stride);
        var first = firstVisible - stride;
        var last = range.Maximum + stride;
        var result = new List<FittedAxisLabel<BigInteger>>();
        for (var value = first; value <= last; value += stride)
        {
            if (!_layoutCache.TryGetValue(value, out var layout))
            {
                layout = labelFactory(value);
                _layoutCache[value] = layout;
            }
            var position = GetPosition(layout, getSlotBounds(value), axisSide, minorAxisMargin);
            result.Add(new FittedAxisLabel<BigInteger>(value, layout, position));
        }
        return result;
    }

    private IReadOnlyList<FittedAxisLabel<BigInteger>> PreserveRetainedLabels(
        IReadOnlyList<FittedAxisLabel<BigInteger>> candidates,
        Side axisSide,
        double multiplier)
    {
        var retained = candidates.Where(label => _previousValues.Contains(label.Value));
        var entering = candidates.Where(label => !_previousValues.Contains(label.Value));
        var accepted = new List<FittedAxisLabel<BigInteger>>();
        foreach (var label in retained.Concat(entering))
        {
            var bounds = GetFitBounds(label, axisSide, multiplier);
            if (accepted.All(existing => !SRectF.Intersects(bounds, GetFitBounds(existing, axisSide, multiplier))))
            {
                accepted.Add(label);
            }
        }
        accepted.Sort((left, right) => left.Value.CompareTo(right.Value));
        return accepted;
    }

    private void PruneCache(IReadOnlySet<BigInteger> retainedValues)
    {
        if (_layoutCache.Count <= MaximumCachedLayouts) return;
        foreach (var key in _layoutCache.Keys.Where(key => !retainedValues.Contains(key)).ToArray())
        {
            _layoutCache.Remove(key);
            if (_layoutCache.Count <= MaximumCachedLayouts) break;
        }
    }

    private static bool HasIntersections(
        IReadOnlyList<FittedAxisLabel<BigInteger>> labels,
        Side side,
        double multiplier)
    {
        var bounds = labels.Select(label => GetFitBounds(label, side, multiplier)).ToArray();
        return SRectF.AnyIntersection(bounds);
    }

    private static SRectF GetFitBounds(FittedAxisLabel<BigInteger> label, Side side, double multiplier)
    {
        var bounds = label.LibraLayoutResult.Bounds.At(label.Position);
        return side switch
        {
            Side.Top or Side.Bottom => bounds.Expand((bounds.Width * (multiplier - 1d)) / 2d, 0d),
            Side.Left or Side.Right => bounds.Expand(0d, (bounds.Height * (multiplier - 1d)) / 2d),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    private static SPointF GetPosition(
        LibraLayoutResult label,
        SRectF slotBounds,
        Side side,
        double minorAxisMargin) => side switch
    {
        Side.Top or Side.Bottom => new SPointF(
            slotBounds.Center.X - (label.Bounds.Width / 2d),
            slotBounds.Bottom + minorAxisMargin),
        Side.Left or Side.Right => new SPointF(
            slotBounds.Right - label.Bounds.Width - minorAxisMargin,
            slotBounds.Center.Y - (label.Bounds.Height / 2d)),
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
    };

    private static BigInteger CeilingToMultiple(BigInteger value, BigInteger stride)
    {
        var quotient = BigInteger.DivRem(value, stride, out var remainder);
        return remainder == 0 || value < 0 ? quotient * stride : (quotient + 1) * stride;
    }

    private static BigInteger CeilingDivide(BigInteger value, BigInteger divisor) =>
        (value + divisor - 1) / divisor;
}
