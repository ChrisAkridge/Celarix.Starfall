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
using ExtendedNumerics;
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
    private readonly IDataSource _dataSource;
    private IReadOnlyList<ResolvedDataPoint> _barData = [];
    private bool _needStaticInvalidation;
    private bool _connected;

    private double _xAxisEm;
    private double _yAxisEm;

    // Libra fields.
    private LibraRenderingContext _xAxisLabelContext;
    private readonly LibraRenderingContext _yAxisLabelContext;
    private readonly IntegralAxisLabelLayout _xAxisLabelLayout = new();
    private double? _lastXAxisLabelPlotWidth;
    private BigDecimal? _lastXAxisLabelViewportSpan;
    private bool _forceXAxisLabelDensityRecalculation = true;

    // Animation slots and their referents.
    private IReadOnlyList<FittedAxisLabel<BigInteger>> _xAxisLabels = [];
    private IReadOnlyList<FittedLabel> _yAxisLabels = [];
    private IReadOnlyList<FittedLabel> _nextYAxisLabels;
    private IReadOnlyList<(SRectF Bar, SColor color)> _barRenderables = [];
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
        : this(measurementService, new DataSeriesDataSource(dataSeries, new StandardResolutionStrategy()), properties, xAxisProperties, yAxisProperties)
    { }

    public BarChartDisplay(
        MeasurementService measurementService,
        IDataSource dataSource,
        BarChartProperties properties,
        AxisProperties<BigInteger> xAxisProperties,
        AxisProperties<double> yAxisProperties)
    {
        _measurementService = measurementService;
        _dataSource = dataSource;
        Properties = properties;
        XAxisProperties = xAxisProperties;
        YAxisProperties = yAxisProperties;

        Connect();

        _xAxisLabelContext = new LibraRenderingContext(measurementService,
            XAxisProperties.LabelFont, LibraMetrics.Default, FenceRenderingMode.Automatic);
        _yAxisLabelContext = new LibraRenderingContext(measurementService,
            YAxisProperties.LabelFont, LibraMetrics.Default, FenceRenderingMode.Automatic);

        _needStaticInvalidation = true;
    }

    public void Connect()
    {
        if (_connected) return;
        _dataSource.DataChanged += DependencyChanged;
        Properties.PropertiesChanged += DependencyChanged;
        XAxisProperties.PropertiesChanged += DependencyChanged;
        YAxisProperties.PropertiesChanged += DependencyChanged;
        _connected = true;
        _needStaticInvalidation = true;
    }

    public void Disconnect()
    {
        if (!_connected) return;
        _dataSource.DataChanged -= DependencyChanged;
        Properties.PropertiesChanged -= DependencyChanged;
        XAxisProperties.PropertiesChanged -= DependencyChanged;
        YAxisProperties.PropertiesChanged -= DependencyChanged;
        _connected = false;
    }

    private void DependencyChanged(object? sender, EventArgs e)
    {
        _needStaticInvalidation = true;
        if (ReferenceEquals(sender, XAxisProperties))
        {
            _xAxisLabelLayout.InvalidateMeasurements();
            _xAxisLabelContext = new LibraRenderingContext(_measurementService,
                XAxisProperties.LabelFont, LibraMetrics.Default, FenceRenderingMode.Automatic);
            _forceXAxisLabelDensityRecalculation = true;
        }
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
        if (!CanRenderPlot(barChartBounds))
        {
            return;
        }
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
            var totalSlots = Properties.XRange.Range + 1;
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
            for (var x = Properties.XRange.Minimum; x <= Properties.XRange.Maximum; x++)
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
            var renderedBounds = label.LibraLayoutResult.Bounds.At(position);
            if (!SRectF.Intersects(renderedBounds, xAxisBounds)) continue;
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
            var (minMultiple, maxMultiple) = GetYGridlines();

            for (int multiple = minMultiple; multiple <= maxMultiple; multiple++)
            {
                if (multiple == 0) continue;
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
    public void AnimateScrollToXRange(BigDecimal newXMinimum, BigDecimal newXMaximum, double duration, Easing? easing = null)
    {
        if (newXMinimum > newXMaximum)
        {
            throw new ArgumentException("newXMinimum must be less than or equal to newXMaximum.");
        }

        easing ??= Easings.Linear;

        var oldXMinimum = Properties.XMinimum;
        var oldXMaximum = Properties.XMaximum;
        var xMinRange = newXMinimum - oldXMinimum;
        var xMaxRange = newXMaximum - oldXMaximum;

        Action<double> updateAction = d =>
        {
            var fraction = (BigDecimal)d;

            Properties.UpdatePropertiesAtomic(() =>
            {
                Properties.XMinimum = oldXMinimum + (xMinRange * fraction);
                Properties.XMaximum = oldXMaximum + (xMaxRange * fraction);

                Console.WriteLine($"[FRAME] xMin: {Properties.XMinimum}, xMax: {Properties.XMaximum}");
            });
        };

        Action onCompleted = () =>
        {
            Properties.XMinimum = newXMinimum;
            Properties.XMaximum = newXMaximum;

            Console.WriteLine($"[ END ] xMin: {Properties.XMinimum}, xMax: {Properties.XMaximum}");
        };

        _moveRangeAnimation.Replace(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(duration),
            updateAction, onCompleted), AnimationSlotReplacementBehavior.CancelExisting);
    }

    // Invalidation
    public void OnContainerChanged()
    {
        _needStaticInvalidation = true;
        _forceXAxisLabelDensityRecalculation = true;
    }

    private void Invalidate(SRectF displayBounds)
    {
        var barChartBounds = GetBarChartBounds(displayBounds);
        if (!CanRenderPlot(barChartBounds))
        {
            _barData = [];
            _xAxisLabels = [];
            _yAxisLabels = [];
            _barRenderables = [];
            _needStaticInvalidation = false;
            return;
        }
        var totalSlots = Properties.XRange.Range + 1;
        int trueSlots = totalSlots < (BigInteger)barChartBounds.Width ? (int)totalSlots : (int)Math.Floor(barChartBounds.Width);
        _barData = trueSlots > 0
            ? DataResolver.Resolve(_dataSource, Properties.XRange, trueSlots)
            : [];
        var yGridLines = GetYGridlines();

        _xAxisEm = _measurementService.MeasureText("M", XAxisProperties.LabelFont).Width;
        var viewportSpan = Properties.XMaximum - Properties.XMinimum;
        var recalculateXAxisLabelDensity = _forceXAxisLabelDensityRecalculation
            || _lastXAxisLabelPlotWidth != barChartBounds.Width
            || _lastXAxisLabelViewportSpan != viewportSpan;
        _xAxisLabels = _xAxisLabelLayout.Update(Properties.XRange,
            x => XAxisProperties.TickFormatter(x).Layout(_xAxisLabelContext, XAxisProperties.LabelColor),
            x => GetXSlotBounds(x, barChartBounds),
            Side.Bottom, XAxisProperties.LabelMarginEm * _xAxisEm,
            XAxisProperties.LabelFitExtentMultiplier,
            recalculateXAxisLabelDensity);
        _lastXAxisLabelPlotWidth = barChartBounds.Width;
        _lastXAxisLabelViewportSpan = viewportSpan;
        _forceXAxisLabelDensityRecalculation = false;

        _yAxisEm = _measurementService.MeasureText("M", YAxisProperties.LabelFont).Width;
        var minimumYGridline = yGridLines.MinMultiple * YAxisProperties.GridlineGap;
        var maximumYGridline = yGridLines.MaxMultiple * YAxisProperties.GridlineGap;
        _yAxisLabels = ChartHelpers.FitLabelsForDoubleAxis(minimumYGridline,
            maximumYGridline,
            y => YAxisProperties.TickFormatter(y).Layout(_yAxisLabelContext, YAxisProperties.LabelColor),
            y => GetYSlotCenter(y, barChartBounds),
            Side.Left, barChartBounds.Left,
            YAxisProperties.LabelMarginEm * _yAxisEm,
            yGridLines.MaxMultiple - yGridLines.MinMultiple + 1,
            YAxisProperties.LabelFitExtentMultiplier);

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

                Console.Write($"{barWidth:F2}, ");
                var barRect = SRectF.GetIntersection(new SRectF(barLeft, barTop, barWidth, barHeight), barChartBounds);

                if (barRect != SRectF.Empty)
                {
                    barRenderables.Add((barRect, Properties.BarColorFormatter(individualDataPoint.Y)));
                }
            }
            else if (dataPoint is AggregatedDataPoint aggregatedDataPoint)
            {
                var slotBounds = GetXRangeBounds(aggregatedDataPoint.Range, barChartBounds);
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
                var minBarRect = SRectF.GetIntersection(new SRectF(slotBounds.Left, minBarTop, slotBounds.Width, minBarHeight), barChartBounds);

                var meanBarTop = Math.Min(meanYBarCenter, yBaseline);
                var meanBarBottom = Math.Max(meanYBarCenter, yBaseline);
                var meanBarHeight = meanBarBottom - meanBarTop;
                var meanBarRect = SRectF.GetIntersection(new SRectF(slotBounds.Left, meanBarTop, slotBounds.Width, meanBarHeight), barChartBounds);

                var maxBarTop = Math.Min(maxBarYCenter, yBaseline);
                var maxBarBottom = Math.Max(maxBarYCenter, yBaseline);
                var maxBarHeight = maxBarBottom - maxBarTop;
                var maxBarRect = SRectF.GetIntersection(new SRectF(slotBounds.Left, maxBarTop, slotBounds.Width, maxBarHeight), barChartBounds);
                if (minBarRect != SRectF.Empty)
                {
                    barRenderables.Add((minBarRect, barMinColor));
                }
                if (meanBarRect != SRectF.Empty)
                {
                    barRenderables.Add((meanBarRect, barMeanColor));
                }
                if (maxBarRect != SRectF.Empty)
                {
                    barRenderables.Add((maxBarRect, barMaxColor));
                }
            }
        }

        Console.WriteLine();
        _barRenderables = barRenderables;
    }

    private SRectF GetBarChartBounds(SRectF displayBounds)
    {
        var xAxisHeight = displayBounds.Height * XAxisProperties.SizeRatioOfParent;
        var yAxisWidth = displayBounds.Width * YAxisProperties.SizeRatioOfParent;
        var allocatedBounds = new SRectF(
            displayBounds.X + yAxisWidth,
            displayBounds.Y,
            displayBounds.Width - yAxisWidth,
            displayBounds.Height - xAxisHeight
        );
        return Properties.PlotInsets.ApplyTo(allocatedBounds);
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
            barChartBounds.Width,
            xAxisHeight
        );
    }

    private SRectF GetYAxisBounds(SRectF displayBounds, SRectF barChartBounds)
    {
        // The Y-axis is strictly to the left of only the bar chart, so it ends more upward than the display area.
        // Pass in barChartBounds because we just calculated it and don't want to recalculate it here.
        return new SRectF(
            displayBounds.X,
            barChartBounds.Top,
            Math.Max(0d, barChartBounds.Left - displayBounds.Left),
            barChartBounds.Height
        );
    }

    private static bool CanRenderPlot(SRectF bounds) => bounds.Width >= 1d && bounds.Height >= 1d;

    private bool IsDense(SRectF barChartBounds)
    {
        var totalSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
        return totalSlots > (BigInteger)barChartBounds.Width;
    }

    private bool ShouldInsetBarWidths(SRectF barChartBounds)
    {
        var totalSlots = Properties.XRange.Range + 1;
        var slotWidth = barChartBounds.Width * MathHelpers.BigIntegerRatioToDouble(BigInteger.One, totalSlots);

        // TODO: lift totally arbitrary 3f threshold into a constant
        return slotWidth >= 3f;
    }

    private SRectF GetXSlotBounds(BigInteger x, SRectF barChartBounds)
    {
        var totalSlotsIntegral = Properties.XRange.Range + 1;
        var slotIndex = x - Properties.XMinimum;

        if (!IsDense(barChartBounds))
        {
            var slotWidth = (BigDecimal)barChartBounds.Width / totalSlotsIntegral;
            var slotLeft = barChartBounds.Left + (slotIndex * slotWidth);

            return new SRectF((double)slotLeft, barChartBounds.Top, (double)slotWidth, barChartBounds.Height);
        }
        else
        {
            var pixelIndex = (int)BigDecimal.Floor((BigDecimal)barChartBounds.Width * (slotIndex / (BigDecimal)totalSlotsIntegral));
            return new SRectF(barChartBounds.Left + pixelIndex, barChartBounds.Top, 1, barChartBounds.Height);
        }
    }

    private SRectF GetXRangeBounds(XRange range, SRectF barChartBounds)
    {
        var first = GetXSlotBounds(range.Minimum, barChartBounds);
        var last = GetXSlotBounds(range.Maximum, barChartBounds);
        return new SRectF(first.Left, barChartBounds.Top, last.Right - first.Left, barChartBounds.Height);
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

    private (int MinMultiple, int MaxMultiple) GetYGridlines()
    {
        var gap = YAxisProperties.GridlineGap;
        var minimum = checked((int)Math.Ceiling(Properties.YMinimum / gap));
        var maximum = checked((int)Math.Floor(Properties.YMaximum / gap));
        return (minimum, maximum);
    }
}
