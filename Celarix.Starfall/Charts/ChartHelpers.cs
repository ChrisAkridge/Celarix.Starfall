using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Extensions;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts;

public static class ChartHelpers
{
    public static IReadOnlyList<FittedLabel> FitLabelsForAxis(XRange range,
        Func<BigInteger, LibraLayoutResult> labelFactory,
        Func<BigInteger, SRectF> getSlotBounds,
        Side axisSide,
        double minorAxisMargin,
        double labelFitExtentMultiplier = 1d)
    {
        ValidateLabelFitExtentMultiplier(labelFitExtentMultiplier);
        var previousCandidateLabels = new List<FittedLabel>();
        var candidateLabels = new List<FittedLabel>();
        var cardinality = range.Range + 1;

        if (cardinality == 1)
        {
            var index = range.Minimum;
            var label = labelFactory(index);
            var slotBounds = getSlotBounds(index);
            candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            return candidateLabels;
        }

        for (var tickCount = 2; tickCount <= cardinality; tickCount++)
        {
            for (var i = 0; i < tickCount; i++)
            {
                var index = GetEvenlyDistributedIndex(range.Minimum, cardinality, i, tickCount);
                var label = labelFactory(index);
                var slotBounds = getSlotBounds(index);
                candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            }

            if (HasAnyLabelIntersections(candidateLabels, axisSide, labelFitExtentMultiplier))
            {
                return previousCandidateLabels;
            }
            else
            {
                previousCandidateLabels = candidateLabels;
                candidateLabels = new();
            }
        }

        // If we reach this point, it means that even the maximum number of ticks (equal to the range) did
        // not result in any intersections. In this case, we can return the last set of candidate labels.
        return previousCandidateLabels;
    }

    public static IReadOnlyList<FittedLabel> FitLabelsForDoubleAxis(double minimum, double maximum,
        Func<double, LibraLayoutResult> labelFactory,
        Func<double, double> getSlotCenter,
        Side axisSide,
        double minorAxisEdge,
        double minorAxisMargin,
        int maxLabels,
        double labelFitExtentMultiplier = 1d)
    {
        ValidateLabelFitExtentMultiplier(labelFitExtentMultiplier);
        var previousCandidateLabels = new List<FittedLabel>();
        var candidateLabels = new List<FittedLabel>();
        if (maximum == minimum)
        {
            var index = minimum;
            var label = labelFactory(index);
            var slotBounds = axisSide switch
            {
                Side.Top or Side.Bottom => new SRectF(getSlotCenter(index), minorAxisEdge, 0, 0),
                Side.Left or Side.Right => new SRectF(minorAxisEdge, getSlotCenter(index), 0, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
            };
            candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            return candidateLabels;
        }

        for (var tickCount = 2; tickCount <= maxLabels; tickCount++)
        {
            for (var i = 0; i < tickCount; i++)
            {
                var index = minimum + (i * (maximum - minimum) / (tickCount - 1));
                var label = labelFactory(index);
                var slotBounds = axisSide switch
                {
                    Side.Top or Side.Bottom => new SRectF(getSlotCenter(index), minorAxisEdge, 0, 0),
                    Side.Left or Side.Right => new SRectF(minorAxisEdge, getSlotCenter(index), 0, 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
                };
                candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            }
            if (HasAnyLabelIntersections(candidateLabels, axisSide, labelFitExtentMultiplier))
            {
                return previousCandidateLabels;
            }
            else
            {
                previousCandidateLabels = candidateLabels;
                candidateLabels = new();
            }
        }

        // If we reach this point, it means that even the maximum number of ticks (equal to the range) did
        // not result in any intersections. In this case, we can return the last set of candidate labels.
        return previousCandidateLabels;
    }

    public static ChartText FormatCountAndSum(ChartText countText, ChartText sumText)
    {
        var countTextString = !countText.UseLibra ? $"\"{countText.SourceString}\"" : countText.SourceString;
        var sumTextString = !sumText.UseLibra ? $"\"{sumText.SourceString}\"" : sumText.SourceString;

        var sourceString = $";catEm(1, \"count: \", {countTextString}, \"sum: \", {sumTextString})";
        return new ChartText(sourceString, useLibra: true);
    }

    private static BigInteger GetEvenlyDistributedIndex(
            BigInteger minimum,
            BigInteger range,
            int tickIndex,
            int tickCount)
    {
        var numerator = (BigInteger)tickIndex * (range - 1);
        var denominator = (BigInteger)(tickCount - 1);

        var a = numerator + (denominator / 2);
        var b = a / denominator;
        var c = b + minimum;
        return c;
    }

    private static SPointF GetAxisLabelPosition(
        LibraLayoutResult label,
        SRectF slotBounds,
        Side axisSide,
        double minorAxisMargin)
    {
        var labelSize = label.Bounds.Size;
        double labelX, labelY;
        labelX = axisSide switch
        {
            Side.Top or Side.Bottom => slotBounds.Center.X - (labelSize.Width / 2),
            Side.Left or Side.Right => slotBounds.Right - labelSize.Width - minorAxisMargin,
            _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
        };
        labelY = axisSide switch
        {
            Side.Top or Side.Bottom => slotBounds.Bottom + minorAxisMargin,
            Side.Left or Side.Right => slotBounds.Center.Y - (labelSize.Height / 2),
            _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
        };
        return new SPointF(labelX, labelY);
    }

    private static bool HasAnyLabelIntersections(
        IReadOnlyList<FittedLabel> labels,
        Side axisSide,
        double labelFitExtentMultiplier)
    {
        SRectF[] labelBounds = [.. labels.Select(l => ExpandMajorAxisExtent(
            l.LibraLayoutResult.Bounds.At(l.Position), axisSide, labelFitExtentMultiplier))];
        return SRectF.AnyIntersection(labelBounds);
    }

    private static SRectF ExpandMajorAxisExtent(SRectF bounds, Side axisSide, double multiplier)
    {
        return axisSide switch
        {
            Side.Top or Side.Bottom => bounds.Expand((bounds.Width * (multiplier - 1d)) / 2d, 0d),
            Side.Left or Side.Right => bounds.Expand(0d, (bounds.Height * (multiplier - 1d)) / 2d),
            _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
        };
    }

    private static void ValidateLabelFitExtentMultiplier(double multiplier)
    {
        if (!double.IsFinite(multiplier) || multiplier < 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Label fit extent multiplier must be finite and at least 1.");
        }
    }

    private static IOrderedEnumerable<ResolvedDataPoint> SortByX(IEnumerable<ResolvedDataPoint> points)
    {
        return points.OrderBy(p =>
        {
            if (p is IndividualDataPoint individualDataPoint)
            {
                return individualDataPoint.X;
            }
            else if (p is AggregatedDataPoint aggregatedDataPoint)
            {
                return aggregatedDataPoint.Range.Minimum;
            }
            else
            {
                throw new ArgumentException("Unknown ResolvedDataPoint type.", nameof(points));
            }
        });
    }

    private static IOrderedEnumerable<ResolvedDataPoint> SortByY(IEnumerable<ResolvedDataPoint> points)
    {
        return points.OrderBy(p =>
        {
            if (p is IndividualDataPoint individualDataPoint)
            {
                return individualDataPoint.Y;
            }
            else if (p is AggregatedDataPoint aggregatedDataPoint)
            {
                return aggregatedDataPoint.AverageY;
            }
            else
            {
                throw new ArgumentException("Unknown ResolvedDataPoint type.", nameof(points));
            }
        });
    }
}
