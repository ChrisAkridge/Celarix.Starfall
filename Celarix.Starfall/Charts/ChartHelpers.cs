using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Charts.Models;
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
        double minorAxisMargin)
    {
        var previousCandidateLabels = new List<FittedLabel>();
        var candidateLabels = new List<FittedLabel>();

        if (range.Range == 1)
        {
            var index = range.Minimum;
            var label = labelFactory(index);
            var slotBounds = getSlotBounds(index);
            candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            return candidateLabels;
        }

        for (var tickCount = 2; tickCount <= range.Range; tickCount++)
        {
            for (var i = 0; i < tickCount; i++)
            {
                var index = GetEvenlyDistributedIndex(range.Minimum, range.Range, i, tickCount);
                var label = labelFactory(index);
                var slotBounds = getSlotBounds(index);
                candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            }

            if (HasAnyLabelIntersections(candidateLabels))
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

    public static IReadOnlyList<FittedLabel> FitLabelsForDoubleAxis(double max, double min,
        Func<double, LibraLayoutResult> labelFactory,
        Func<double, double> getSlotCenter,
        Side axisSide,
        double minorAxisEdge,
        double minorAxisMargin,
        int maxLabels)
    {
        var previousCandidateLabels = new List<FittedLabel>();
        var candidateLabels = new List<FittedLabel>();
        if (max == min)
        {
            var index = min;
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
                var index = min + (i * (max - min) / (tickCount - 1));
                var label = labelFactory(index);
                var slotBounds = axisSide switch
                {
                    Side.Top or Side.Bottom => new SRectF(getSlotCenter(index), minorAxisEdge, 0, 0),
                    Side.Left or Side.Right => new SRectF(minorAxisEdge, getSlotCenter(index), 0, 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(axisSide), axisSide, null)
                };
                candidateLabels.Add(new FittedLabel(label, GetAxisLabelPosition(label, slotBounds, axisSide, minorAxisMargin)));
            }
            if (HasAnyLabelIntersections(candidateLabels))
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

    private static bool HasAnyLabelIntersections(IReadOnlyList<FittedLabel> labels)
    {
        SRectF[] labelBounds = [.. labels.Select(l => l.LibraLayoutResult.Bounds.At(l.Position))];
        return SRectF.AnyIntersection(labelBounds);
    }
}
