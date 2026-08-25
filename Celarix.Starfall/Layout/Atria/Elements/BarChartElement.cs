using Celarix.Starfall.Extensions;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using Celarix.Starfall.Stats;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements;

// Just. Code. Still.
// Getting something on screen is more important than perfect design.
public sealed class BarChartElement : AtriaElement
{
    private const double InitialInnerSizeRatio = 0.9d;
    private const double InitialBarWidthRatioOfSlotWidth = 0.8d;
    private const double XAxisLabelTopMarginEm = 0.5d;
    private const double YAxisLabelRightMarginEm = 1d;
    private const int DefaultYGridlineCount = 5;
    private const double InfoPanelWidthRatio = 0.4d;
    private const double InfoPanelLineSpacingEm = 0.5d;
    private const double InfoPanelBorderWidth = 2d;

    private static readonly SColor XAxisColor = SColor.White;
    private static readonly SColor XLabelColor = SColor.White;
    private static readonly SColor YGridlineColor = new SColor(0x6F, 0x6F, 0x6F, 0xFF);
    private static readonly SColor YLabelColor = new SColor(0x6F, 0x6F, 0x6F, 0xFF);

    private SSizeF _innerSizeRatios = new(InitialInnerSizeRatio, InitialInnerSizeRatio);
    private double _barWidthRatioOfSlotWidth = InitialBarWidthRatioOfSlotWidth;

    // Layout fields
    private string? _invalidationKeyAtLastRender;
    private SRectF? _plotBounds;
    private SRectF? _infoPanelBounds;
    private SFont _labelFont = new SFontFamily("Calibri", 12f);
    private double? _em;
    private (SRectF LabelBounds, string LabelText)[] _xAxisLabels = [];
    private (SRectF LabelBounds, string LabelText)[] _yAxisLabels = [];

    private (DateOnly, int)[] _data;
    private DoubleStatsInfo _info;

    private SSizeF InnerSize => Size * _innerSizeRatios;

    public SSizeF InnerSizeRatios
    {
        get => _innerSizeRatios;
        set
        {
            _innerSizeRatios = value.ZeroIfNotPositive();
        }
    }

    public double BarWidthRatioOfSlotWidth
    {
        get => _barWidthRatioOfSlotWidth;
        set
        {
            _barWidthRatioOfSlotWidth = Math.Clamp(value, 0d, 1d);
        }
    }

    public SColor BarColor { get; set; } = SColor.White;
    public double YMinimum { get; set; }
    public double YMaximum { get; set; }
    public double YGridlineSpacing { get; set; }

    public bool InfoPanelVisible { get; set; } = true;
    public SColor InfoPanelBorderColor { get; set; } = SColor.Blue;
    public SColor InfoPanelBackgroundColor { get; set; } = SColor.DarkBlue;
    public SFont InfoPanelFont { get; set; } = new SFontFamily("Calibri", 20f);
    public SColor InfoPanelFontColor { get; set; } = SColor.White;
    

    public BarChartElement(IEnumerable<(DateOnly, int)> data, string atriaIdString)
    {
        _data = [.. data.OrderBy(d => d.Item1)];
        _info = new DoubleStatsInfo(_data.Select(d => (double)d.Item2));
        Id = AtriaId.Parse(atriaIdString);

        // Set some initial defaults.
        YMinimum = 0d;
        YMaximum = GetVisuallyRoundedValue(_data.MaxBy(d => d.Item2).Item2);

        var yRange = YMaximum - YMinimum;
        YGridlineSpacing = yRange / DefaultYGridlineCount;
    }

    private void RefreshLayoutIfNeeded(IRenderTarget target)
    {
        var invalidationKey = GetInvalidationKey();
        if (invalidationKey == _invalidationKeyAtLastRender)
        {
            return;
        }
        _invalidationKeyAtLastRender = invalidationKey;

        // Compute the chart and info panel bounds.
        var innerBounds = Bounds.ShrinkByFactor(1 - _innerSizeRatios.Width, 1 - _innerSizeRatios.Height);
        if (InfoPanelVisible)
        {
            var (chartOuter, infoPanelOuter) = innerBounds.SplitHorizontal(innerBounds, 1 - InfoPanelWidthRatio);
            _plotBounds = chartOuter.ShrinkByFactor(1 - _innerSizeRatios.Width, 1 - _innerSizeRatios.Height);
            _infoPanelBounds = infoPanelOuter.ShrinkByFactor(1 - _innerSizeRatios.Width, 1 - _innerSizeRatios.Height);
        }
        else
        {
            _plotBounds = innerBounds;
            _infoPanelBounds = null;
        }

        // Calculate the X-axis labels.
        var previousCandidateLabels = new List<(SRectF LabelBounds, string LabelText)>();
        var candidateLabels = new List<(SRectF LabelBounds, string LabelText)>();

        for (var xTickCount = 2; ; xTickCount++)
        {
            for (var i = 0; i < xTickCount; i++)
            {
                var fraction = i / (double)(xTickCount - 1);
                var index = (int)Math.Round(fraction * (_data.Length - 1));
                var label = _data[index].Item1.ToString("M/d");
                var slotBounds = GetSlotBounds(index);

                var labelSize = target.MeasureText(label, _labelFont);
                var labelX = slotBounds.Center.X - (labelSize.Width / 2);
                var labelY = slotBounds.Bottom + (XAxisLabelTopMarginEm * _em!.Value);
                candidateLabels.Add((new SRectF(labelX, labelY, labelSize.Width, labelSize.Height), label));
            }

            var hasOverlap = SRectF.AnyIntersection([.. candidateLabels.Select(l => l.LabelBounds)]);
            if (hasOverlap) { break; }
            else
            {
                previousCandidateLabels = candidateLabels;
                candidateLabels = new();
            }
        }

        _xAxisLabels = previousCandidateLabels.ToArray();

        // Calculate the Y-axis labels.
        var innerRect = _plotBounds!.Value;
        var yRange = YMaximum - YMinimum;
        var yGridlineCount = (int)Math.Floor((YMaximum - YMinimum) / YGridlineSpacing);

        _yAxisLabels = new (SRectF LabelBounds, string LabelText)[yGridlineCount + 1];
        for (var i = 0; i <= yGridlineCount; i++)
        {
            var yValue = YMinimum + (i * YGridlineSpacing);
            var yPosition = ValueToYPosition(yValue, innerRect);
            var labelSize = target.MeasureText(yValue.ToString("F0"), _labelFont);
            var labelX = innerRect.Left - (labelSize.Width + (YAxisLabelRightMarginEm * _em!.Value));
            var labelY = yPosition - (labelSize.Height / 2);
            _yAxisLabels[i] = (new SRectF(labelX, labelY, labelSize.Width, labelSize.Height), yValue.ToString("F0"));
        }
    }

    public override void Render(IRenderTarget target)
    {
        _em ??= target.MeasureText("M", _labelFont).Height;

        RefreshLayoutIfNeeded(target);
        
        var innerRect = GetChartBounds()!.Value;
        var yRange = YMaximum - YMinimum;

        // Draw the Y-axis gridlines.
        var yGridlineCount = (int)Math.Floor((YMaximum - YMinimum) / YGridlineSpacing);
        for (var i = 0; i <= yGridlineCount; i++)
        {
            var yValue = YMinimum + (i * YGridlineSpacing);
            var yPosition = ValueToYPosition(yValue, innerRect);
            target.DrawLine(new SPointF(innerRect.Left, yPosition), new SPointF(innerRect.Right, yPosition), YGridlineColor.WithOpacity(Opacity), 1f);
        }

        // Draw the Y-axis labels.
        foreach (var (labelBounds, labelText) in _yAxisLabels)
        {
            target.DrawText(labelText, _labelFont, labelBounds, YLabelColor.WithOpacity(Opacity), SAngle.Zero);
        }

        // Draw the bars.
        var slotCount = _data.Length;
        var slotWidth = innerRect.Width / slotCount;
        var barWidth = slotWidth * _barWidthRatioOfSlotWidth;
        var maxValue = _data.Max(d => d.Item2);

        for (var i = 0; i < _data.Length; i++)
        {
            var item = _data[i];
            var slotRect = GetSlotBounds(i);
            var slotCenterX = slotRect.Center.X;
            var barX = slotCenterX - (barWidth / 2);
            var barHeight = (item.Item2 - YMinimum) / yRange * slotRect.Height;
            var barY = slotRect.Bottom - barHeight;
            var barRect = new SRectF(barX, barY, barWidth, barHeight);

            target.DrawRectangle(barRect, BarColor.WithOpacity(Opacity), SPaintStyle.Fill, SAngle.Zero);
        }

        // Draw the line at Y = 0.
        var zeroLineY = ValueToYPosition(0, innerRect);
        target.DrawLine(new SPointF(innerRect.Left, zeroLineY), new SPointF(innerRect.Right, zeroLineY), XAxisColor.WithOpacity(Opacity), 1f);

        // Draw the X-axis labels.
        foreach (var (labelBounds, labelText) in _xAxisLabels)
        {
            target.DrawText(labelText, _labelFont, labelBounds, XLabelColor.WithOpacity(Opacity), SAngle.Zero);
        }

        // Draw the info panel if needed.
        if (InfoPanelVisible)
        {
            var infoPanelBounds = GetInfoPanelBounds()!.Value;
            var infoPanelInset = infoPanelBounds.Shrink(InfoPanelBorderWidth * 2d, InfoPanelBorderWidth * 2d);
            target.DrawRectangleOfThickness(infoPanelBounds, InfoPanelBorderWidth, InfoPanelBorderColor.WithOpacity(Opacity));
            target.DrawRectangle(infoPanelInset, InfoPanelBackgroundColor.WithOpacity(Opacity), SPaintStyle.Fill, SAngle.Zero);

            var infoText = _info.GetDisplayText();
            var em = target.MeasureText("M", InfoPanelFont).Height;
            var textX = infoPanelInset.X + em;
            var textY = infoPanelInset.Y + em;
            foreach (var line in infoText)
            {
                var lineSize = target.MeasureText(line, InfoPanelFont);
                var lineBounds = new SRectF(textX, textY, lineSize.Width, lineSize.Height);
                target.DrawText(line, InfoPanelFont, lineBounds, InfoPanelFontColor.WithOpacity(Opacity), SAngle.Zero);
                textY += lineSize.Height + (InfoPanelLineSpacingEm * em);
            }
        }
    }

    private SRectF? GetChartBounds() => _plotBounds;

    private SRectF? GetInfoPanelBounds() => _infoPanelBounds;

    private SRectF GetSlotBounds(int slotIndex)
    {
        var innerRect = GetChartBounds();
        if (innerRect == null) { throw new InvalidOperationException("Chart bounds are not set."); }
        var slotCount = _data.Length;
        var slotWidth = innerRect.Value.Width / slotCount;
        return new SRectF(innerRect.Value.X + (slotIndex * slotWidth), innerRect.Value.Y, slotWidth, innerRect.Value.Height);
    }

    private string GetInvalidationKey()
    {
        var sb = new StringBuilder();
        sb.Append(Bounds.ToString());
        sb.Append(_innerSizeRatios.ToString());
        sb.Append(_barWidthRatioOfSlotWidth);
        sb.Append(YMinimum);
        sb.Append(YMaximum);
        sb.Append(YGridlineSpacing);
        sb.Append(InfoPanelVisible);
        return sb.ToString();
    }

    private double ValueToYPosition(double value, SRectF innerRect)
    {
        var yRange = YMaximum - YMinimum;
        return innerRect.Bottom - ((value - YMinimum) / yRange * innerRect.Height);
    }

    private static double GetVisuallyRoundedValue(double value)
    {
        if (value.EqualsWithTolerance(0d, 0.001d)) { return 0d; }

        // For example, 4450 gets:
        // - magnitude = 1000
        // - valueInMagnitude = 4.45
        // - integerBasis = 5
        // and returns 5000.
        var absValue = Math.Abs(value);
        
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(absValue)));
        var valueInMagnitude = absValue / magnitude;
        var integerBasis = Math.Floor(valueInMagnitude) + 1;
        var result = integerBasis * magnitude;

        if (value < 0)
        {
            result *= -1;
        }
        return result;
    }
}
