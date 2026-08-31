using Celarix.Starfall.Charts.DataResolution;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Libra.Metrics;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Color;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public sealed class BarChartDisplay : IChartDisplay
{
    // maybe make this a property later
    private const double TickLength = 15d;

    private readonly MeasurementService _measurementService;
    private readonly DataSeries _dataSeries;
    private IReadOnlyList<ResolvedDataPoint> _barData;
    private bool _needStaticInvalidation;

    // Okay, this one requires some justification. This represents the fractional offset of where
    // the slots are rendered vs. where they "naturally" are. For example, if we've scrolled by half
    // a slot, we want to render bars half a slot-width to the left, then clipped by the Y-axis. But
    // why UInt32? Well, we need a number that is strictly between 0 and 1 and has wraparound behavior.
    // We don't want to have to bother with keeping everything in the range [0, 1) manually. So this
    // represents the fractional offset as a UInt32, where 0 is 0.0 and UInt32.MaxValue is just under 1.0.
    private uint _animMinSlotScrollOffset;
    private uint _animMaxSlotScrollOffset;
    private double AnimMinSlotScrollOffset => _animMinSlotScrollOffset / (double)uint.MaxValue;
    private double AnimMaxSlotScrollOffset => _animMaxSlotScrollOffset / (double)uint.MaxValue;
    private double _xAxisEm;
    private double _yAxisEm;

    // Libra fields.
    private readonly LibraRenderingContext _xAxisLabelContext;
    private readonly LibraRenderingContext _yAxisLabelContext;

    // Animation slots and their referents.
    private IReadOnlyList<FittedLabel> _xAxisLabels;
    private IReadOnlyList<FittedLabel> _nextXAxisLabels;
    private IReadOnlyList<FittedLabel> _yAxisLabels;
    private IReadOnlyList<FittedLabel> _nextYAxisLabels;
    private IReadOnlyList<(SRectF Bar, SColor color)> _barRenderables;
    private IReadOnlyList<(SRectF Bar, SColor color)> _nextBarRenderables;
    private AnimationSlot _moveRangeAnimation;
    private AnimationSlot _changeDataAnimation;

    public BarChartProperties Properties { get; }
    public AxisProperties<BigInteger> XAxisProperties { get; }
    public AxisProperties<double> YAxisProperties { get; }
    public AnimationContext? AnimationContext { get; set; }

    public BarChartDisplay(
        MeasurementService measurementService,
        DataSeries dataSeries,
        BarChartProperties properties,
        AxisProperties<BigInteger> xAxisProperties,
        AxisProperties<double> yAxisProperties)
    {
        _measurementService = measurementService;
        _dataSeries = dataSeries;
        Properties = properties;
        XAxisProperties = xAxisProperties;
        YAxisProperties = yAxisProperties;

        _dataSeries.DataChanged += (sender, args) => _needStaticInvalidation = true;
        Properties.PropertiesChanged += (sender, args) => _needStaticInvalidation = true;
        XAxisProperties.PropertiesChanged += (sender, args) => _needStaticInvalidation = true;
        YAxisProperties.PropertiesChanged += (sender, args) => _needStaticInvalidation = true;

        _xAxisLabelContext = new LibraRenderingContext(measurementService,
            XAxisProperties.LabelFont, LibraMetrics.Default, FenceRenderingMode.Automatic);
        _yAxisLabelContext = new LibraRenderingContext(measurementService,
            YAxisProperties.LabelFont, LibraMetrics.Default, FenceRenderingMode.Automatic);

        _needStaticInvalidation = true;
    }

    // Rendering
    public void Render(IRenderTarget target, SRectF displayBounds)
    {
        _moveRangeAnimation ??= new AnimationSlot(AnimationContext!, "BarChartDisplay.MoveRangeAnimation");
        _changeDataAnimation ??= new AnimationSlot(AnimationContext!, "BarChartDisplay.ChangeDataAnimation");

        if (_needStaticInvalidation)
        {
            Invalidate(displayBounds);
        }

        // Let's draw this all piece-by-piece.
        var barChartBounds = GetBarChartBounds(displayBounds);
        var xAxisBounds = GetXAxisBounds(displayBounds, barChartBounds);
        var yAxisBounds = GetYAxisBounds(displayBounds, barChartBounds);
        var yRange = Properties.YMaximum - Properties.YMinimum;

        DrawYEquals0Line(target, barChartBounds, yRange);
        DrawXAxis(target, xAxisBounds, barChartBounds);
        DrawYAxis(target, yAxisBounds, barChartBounds);
        DrawBars(target);
    }

    private void DrawYEquals0Line(IRenderTarget target, SRectF barChartBounds, double yRange)
    {
        // First, let's draw the line at Y = 0.
        if (YAxisProperties.GridlineStyle == GridlineStyle.None)
        {
            return;
        }

        var zeroY = GetYBaseline(barChartBounds);
        target.DrawLine(new SPointF(barChartBounds.Left, (float)zeroY), new SPointF(barChartBounds.Right, (float)zeroY), SColor.White,
            (float)YAxisProperties.GridlineThickness);
    }

    private void DrawXAxis(IRenderTarget target, SRectF xAxisBounds, SRectF barChartBounds)
    {
        // Draw the main gridline at the bottom of the bar chart. Use the label color for this line.
        var mainGridlineColor = XAxisProperties.LabelColor;
        var mainGridlineY = xAxisBounds.Top;
        target.DrawLine(new SPointF(xAxisBounds.Left, mainGridlineY), new SPointF(xAxisBounds.Right, mainGridlineY), mainGridlineColor,
            (float)XAxisProperties.GridlineThickness);

        // Check to see if we can draw the ticks/gridlines.
        var canDrawTicks = !IsDense(barChartBounds) && XAxisProperties.GridlineStyle != GridlineStyle.None;
        if (canDrawTicks)
        {
            var totalSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
            var slotWidth = barChartBounds.Width * MathHelpers.BigIntegerRatioToDouble(BigInteger.One, totalSlots);
            var minSlotWidthToDrawTicks = XAxisProperties.GridlineThickness * 2d;
            if (slotWidth < minSlotWidthToDrawTicks)
            {
                canDrawTicks = false;
            }
        }
        if (canDrawTicks)
        {
            // Draw the ticks/gridlines for each slot.
            var lineLength = XAxisProperties.GridlineStyle switch
            {
                GridlineStyle.Tick => TickLength,
                GridlineStyle.Gridline => barChartBounds.Height,
                _ => throw new NotImplementedException($"Gridline style {XAxisProperties.GridlineStyle} is not implemented.")
            };
            for (var x = Properties.XMinimum; x <= Properties.XMaximum; x++)
            {
                var slotBounds = GetXSlotBounds(x, barChartBounds);
                var tickX = slotBounds.Center.X;
                var tickStartY = mainGridlineY;
                var tickEndY = mainGridlineY - lineLength;
                target.DrawLine(new SPointF(tickX, tickStartY), new SPointF(tickX, tickEndY), mainGridlineColor,
                    (float)XAxisProperties.GridlineThickness);
            }
        }

        // Draw the labels for each slot.
        foreach (var label in _xAxisLabels)
        {
            var position = label.Position;
            foreach (var renderable in label.LibraLayoutResult.Renderables)
            {
                renderable.RenderAt(target, position, 1d);
            }
        }
    }

    private void DrawYAxis(IRenderTarget target, SRectF yAxisBounds, SRectF barChartBounds)
    {
        // Draw the main gridline at the left of the bar chart. Use the label color for this line.
        var mainGridlineColor = YAxisProperties.LabelColor;
        var mainGridlineX = yAxisBounds.Right;
        target.DrawLine(new SPointF(mainGridlineX, yAxisBounds.Top), new SPointF(mainGridlineX, yAxisBounds.Bottom), mainGridlineColor,
            (float)YAxisProperties.GridlineThickness);

        // Draw the ticks/gridlines given the gridline style and the spacing.
        if (YAxisProperties.GridlineStyle != GridlineStyle.None)
        {
            var (minMultiple, maxMultiple) = GetYGridlines(barChartBounds);

            for (int multiple = minMultiple; multiple <= maxMultiple; multiple++)
            {
                var y = multiple * YAxisProperties.GridlineGap;
                DrawTickOrGridline(y);
            }
        }

        // Draw the labels for each slot.
        foreach (var label in _yAxisLabels)
        {
            var position = label.Position;
            foreach (var renderable in label.LibraLayoutResult.Renderables)
            {
                renderable.RenderAt(target, position, 1d);
            }
        }

        void DrawTickOrGridline(double y)
        {
            var ySlotCenter = GetYSlotCenter(y, barChartBounds);
            var lineLength = YAxisProperties.GridlineStyle switch
            {
                GridlineStyle.Tick => TickLength,
                GridlineStyle.Gridline => barChartBounds.Width,
                _ => throw new NotImplementedException($"Gridline style {YAxisProperties.GridlineStyle} is not implemented.")
            };
            var tickStartX = mainGridlineX;
            var tickEndX = mainGridlineX + lineLength;
            target.DrawLine(new SPointF(tickStartX, ySlotCenter), new SPointF(tickEndX, ySlotCenter), mainGridlineColor,
                (float)YAxisProperties.GridlineThickness);
        }
    }

    private void DrawBars(IRenderTarget target)
    {
        foreach (var (bar, color) in _barRenderables)
        {
            target.DrawRectangle(bar, color, SPaintStyle.Fill, SAngle.Zero);
        }
    }

    // Animation
    public void AnimateScrollToXRange(BigInteger newXMinimum, BigInteger newXMaximum, double duration, Easing? easing = null)
    {
        const int InterpolationFractionBits = 53;
        const int ScrollOffsetBits = 32;

        if (newXMinimum > newXMaximum)
        {
            throw new ArgumentException("newXMinimum must be less than or equal to newXMaximum.");
        }

        easing ??= Easings.Linear;

        var oldXMinimum = Properties.XMinimum;
        var oldXMaximum = Properties.XMaximum;
        var xMinRange = newXMinimum - oldXMinimum;
        var xMaxRange = newXMaximum - oldXMaximum;

        var fractionScale = BigInteger.One << InterpolationFractionBits;
        var fractionMask = fractionScale - 1;

        Action<double> updateAction = d =>
        {
            var fraction = (BigInteger)(d * (1L << InterpolationFractionBits));

            var minFixed = xMinRange * fraction;
            var maxFixed = xMaxRange * fraction;

            var minInteger = minFixed >> InterpolationFractionBits;
            var maxInteger = maxFixed >> InterpolationFractionBits;

            var minFraction = minFixed & fractionMask;
            var maxFraction = maxFixed & fractionMask;

            _animMinSlotScrollOffset =
                (uint)(minFraction >> (InterpolationFractionBits - ScrollOffsetBits));

            _animMaxSlotScrollOffset =
                (uint)(maxFraction >> (InterpolationFractionBits - ScrollOffsetBits));

            var oldRaiseEvents = Properties.RaiseEventOnChanged;

            Properties.XMinimum = oldXMinimum + minInteger;
            Properties.XMaximum = oldXMaximum + maxInteger;
        };

        Action onCompleted = () =>
        {
            Properties.XMinimum = newXMinimum;
            Properties.XMaximum = newXMaximum;
            _animMinSlotScrollOffset = 0;
            _animMaxSlotScrollOffset = 0;
        };

        _moveRangeAnimation.Replace(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(duration),
            updateAction, onCompleted), AnimationSlotReplacementBehavior.CancelExisting);
    }

    // Invalidation
    private void Invalidate(SRectF displayBounds)
    {
        var barChartBounds = GetBarChartBounds(displayBounds);
        var totalSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
        int trueSlots = totalSlots < (BigInteger)barChartBounds.Width ? (int)totalSlots : (int)barChartBounds.Width;
        var source = new DataSeriesDataSource(_dataSeries, new StandardResolutionStrategy());
        _barData = DataResolver.Resolve(source, new(Properties.XMinimum, Properties.XMaximum), trueSlots);
        var yGridLines = GetYGridlines(barChartBounds);

        // TODO: Animation slots!
        _xAxisEm = _measurementService.MeasureText("M", XAxisProperties.LabelFont).Width;
        _xAxisLabels = ChartHelpers.FitLabelsForAxis(new(Properties.XMinimum, Properties.XMaximum),
            x => XAxisProperties.TickFormatter(x).Layout(_xAxisLabelContext, XAxisProperties.LabelColor),
            x => GetXSlotBounds(x, barChartBounds),
            Side.Bottom, XAxisProperties.LabelMarginEm * _xAxisEm);

        _yAxisEm = _measurementService.MeasureText("M", YAxisProperties.LabelFont).Width;
        _yAxisLabels = ChartHelpers.FitLabelsForDoubleAxis(Properties.YMinimum,
            Properties.YMaximum,
            y => YAxisProperties.TickFormatter(y).Layout(_yAxisLabelContext, YAxisProperties.LabelColor),
            y => GetYSlotCenter(y, barChartBounds),
            Side.Left, barChartBounds.Left,
            YAxisProperties.LabelMarginEm * _yAxisEm,
            yGridLines.MaxMultiple - yGridLines.MinMultiple + 1);

        BuildRenderables(barChartBounds);

        _needStaticInvalidation = false;
    }

    private void BuildRenderables(SRectF barChartBounds)
    {
        var yBaseline = GetYBaseline(barChartBounds);
        var barRenderables = new List<(SRectF Bar, SColor color)>();
        foreach (var dataPoint in _barData)
        {
            if (dataPoint is IndividualDataPoint individualDataPoint)
            {
                var slotBounds = GetXSlotBounds(individualDataPoint.X, barChartBounds);
                var yCenter = GetYSlotCenter(individualDataPoint.Y, barChartBounds);
                var barLeft = slotBounds.Left;
                var barTop = Math.Min(yCenter, yBaseline);
                var barBottom = Math.Max(yCenter, yBaseline);
                var barHeight = barBottom - barTop;
                var barWidth = slotBounds.Width;
                if (ShouldInsetBarWidths(barChartBounds))
                {
                    barWidth *= Properties.BarWidthRatioOfSlotWidth;
                    barLeft += (slotBounds.Width - barWidth) / 2f;
                }

                var barRect = new SRectF(barLeft, barTop, barWidth, barHeight);
                barRenderables.Add((barRect, Properties.BarColorFormatter(individualDataPoint.Y)));
            }
            else if (dataPoint is AggregatedDataPoint aggregatedDataPoint)
            {
                var slotBounds = GetXSlotBounds(aggregatedDataPoint.Range.Minimum, barChartBounds);
                var meanYBarCenter = GetYSlotCenter(aggregatedDataPoint.AverageY, barChartBounds);
                var barMinColor = Properties.BarColorFormatter(aggregatedDataPoint.AverageY);
                var barShades = ColorHelpers.LightnessRamp(barMinColor, 0.1d, 3).ToArray();
                var barMeanColor = barShades[1];
                var barMaxColor = barShades[2];

                var minBarYCenter = GetYSlotCenter(aggregatedDataPoint.MinimumY, barChartBounds);
                var maxBarYCenter = GetYSlotCenter(aggregatedDataPoint.MaximumY, barChartBounds);

                var minBarTop = Math.Min(minBarYCenter, yBaseline);
                var minBarBottom = Math.Max(minBarYCenter, yBaseline);
                var minBarHeight = minBarBottom - minBarTop;
                var minBarRect = new SRectF(slotBounds.Left, minBarTop, slotBounds.Width, minBarHeight);

                var meanBarTop = Math.Min(meanYBarCenter, yBaseline);
                var meanBarBottom = Math.Max(meanYBarCenter, yBaseline);
                var meanBarHeight = meanBarBottom - meanBarTop;
                var meanBarRect = new SRectF(slotBounds.Left, meanBarTop, slotBounds.Width, meanBarHeight);

                var maxBarTop = Math.Min(maxBarYCenter, yBaseline);
                var maxBarBottom = Math.Max(maxBarYCenter, yBaseline);
                var maxBarHeight = maxBarBottom - maxBarTop;
                var maxBarRect = new SRectF(slotBounds.Left, maxBarTop, slotBounds.Width, maxBarHeight);
                barRenderables.Add((minBarRect, barMinColor));
                barRenderables.Add((meanBarRect, barMeanColor));
                barRenderables.Add((maxBarRect, barMaxColor));
            }
        }
        _barRenderables = barRenderables;
    }

    private SRectF GetBarChartBounds(SRectF displayBounds)
    {
        var xAxisHeight = displayBounds.Height * XAxisProperties.SizeRatioOfParent;
        var yAxisWidth = displayBounds.Width * YAxisProperties.SizeRatioOfParent;
        return new SRectF(
            displayBounds.X + yAxisWidth,
            displayBounds.Y,
            displayBounds.Width - yAxisWidth,
            displayBounds.Height - xAxisHeight
        );
    }

    private SRectF GetXAxisBounds(SRectF displayBounds, SRectF barChartBounds)
    {
        // The X-axis is strictly below only the bar chart, so it starts more leftward than the display area.
        // Pass in barChartBounds because we just calculated it and don't want to recalculate it here.
        var barChartLeft = barChartBounds.Left;
        var xAxisHeight = displayBounds.Height * XAxisProperties.SizeRatioOfParent;
        return new SRectF(
            barChartLeft,
            barChartBounds.Bottom,
            displayBounds.Right - barChartLeft,
            xAxisHeight
        );
    }

    private SRectF GetYAxisBounds(SRectF displayBounds, SRectF barChartBounds)
    {
        // The Y-axis is strictly to the left of only the bar chart, so it ends more upward than the display area.
        // Pass in barChartBounds because we just calculated it and don't want to recalculate it here.
        var barChartBottom = barChartBounds.Bottom;
        var yAxisWidth = displayBounds.Width * YAxisProperties.SizeRatioOfParent;
        return new SRectF(
            displayBounds.X,
            displayBounds.Top,
            yAxisWidth,
            displayBounds.Bottom - barChartBottom
        );
    }

    private bool IsDense(SRectF barChartBounds)
    {
        var totalSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
        return totalSlots > (BigInteger)barChartBounds.Width;
    }

    private bool ShouldInsetBarWidths(SRectF barChartBounds)
    {
        var totalSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
        var slotWidth = barChartBounds.Width * MathHelpers.BigIntegerRatioToDouble(BigInteger.One, totalSlots);

        // TODO: lift totally arbitrary 3f threshold into a constant
        return slotWidth >= 3f;
    }

    private BigInteger GetXMinimumFixedPoint()
    {
        var integerPart = Properties.XMinimum << 32;
        var fractionalPart = ((BigInteger)_animMinSlotScrollOffset);
        return integerPart + fractionalPart;
    }

    private BigInteger GetXMaximumFixedPoint()
    {
        var integerPart = Properties.XMaximum << 32;
        var fractionalPart = ((BigInteger)_animMaxSlotScrollOffset);
        return integerPart + fractionalPart;
    }

    private SRectF GetXSlotBounds(BigInteger x, SRectF barChartBounds)
    {
        var totalSlotsIntegral = (Properties.XMaximum - Properties.XMinimum) + 1;
        var totalSlotsFixedPoint = (GetXMaximumFixedPoint() - GetXMinimumFixedPoint()) + (BigInteger.One << 32);
        var barChartWidthFixedPoint = (BigInteger)(barChartBounds.Width * (1L << 32));
        var barChartLeftFixedPoint = (BigInteger)(barChartBounds.Left * (1L << 32));
        var slotIndex = x - Properties.XMinimum;

        if (!IsDense(barChartBounds))
        {
            var slotWidthFixedPoint = (barChartWidthFixedPoint << 32) / totalSlotsFixedPoint;
            var slotLeftFixedPoint = barChartLeftFixedPoint
                + (slotIndex * slotWidthFixedPoint)
                + (_animMinSlotScrollOffset);

            // Overflow is actually okay here - if you're asking for slots way off the left or right, that's on you.
            var slotWidth = ((double)slotWidthFixedPoint) / (1L << 32);
            var slotLeft = ((double)slotLeftFixedPoint) / (1L << 32);

            return new SRectF(slotLeft, barChartBounds.Top, slotWidth, barChartBounds.Height);
        }
        else
        {
            var pixelIndex = barChartBounds.Width * MathHelpers.BigIntegerRatioToDouble(slotIndex, totalSlotsIntegral);
            return new SRectF(barChartBounds.Left + pixelIndex, barChartBounds.Top, 1, barChartBounds.Height);
        }
    }

    private double GetYSlotCenter(double y, SRectF barChartBounds)
    {
        var yRange = Properties.YMaximum - Properties.YMinimum;
        var yRatio = (y - Properties.YMinimum) / yRange;
        var yPixel = barChartBounds.Bottom - (yRatio * barChartBounds.Height);
        return yPixel;
    }

    private double GetYBaseline(SRectF barChartBounds)
    {
        if (Properties.YMaximum < 0)
        {
            return barChartBounds.Top;
        }
        else if (Properties.YMinimum > 0)
        {
            return barChartBounds.Bottom;
        }
        else
        {
            var yRange = Properties.YMaximum - Properties.YMinimum;
            var zeroY = barChartBounds.Top + ((Properties.YMaximum / yRange) * barChartBounds.Height);
            return zeroY;
        }
    }

    private (int MinMultiple, int MaxMultiple) GetYGridlines(SRectF barChartBounds)
    {
        var multiple = 1;
        int? highMultiple = null;
        int? lowMultiple = null;

        while (highMultiple == null || lowMultiple == null)
        {
            var positiveGridline = multiple * YAxisProperties.GridlineGap;
            var negativeGridline = -multiple * YAxisProperties.GridlineGap;

            var positiveGridlineY = GetYSlotCenter(positiveGridline, barChartBounds);
            var negativeGridlineY = GetYSlotCenter(negativeGridline, barChartBounds);

            if (highMultiple == null && positiveGridlineY < barChartBounds.Top)
            {
                highMultiple = multiple - 1;
            }
            if (lowMultiple == null && negativeGridlineY > barChartBounds.Bottom)
            {
                lowMultiple = multiple - 1;
            }

            multiple += 1;
        }

        return (lowMultiple.Value, highMultiple.Value);
    }
}
