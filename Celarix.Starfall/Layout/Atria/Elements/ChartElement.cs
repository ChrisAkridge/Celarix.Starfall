using Celarix.Starfall.Charts.Displays;
using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Extensions;
using Celarix.Starfall.Layout.Atria.Components;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Metrics;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements;

public sealed class ChartElement : AtriaElement
{
    private readonly record struct InfoPanelRow(
        string Label,
        ChartText? Value,
        ChartText? AlternateValue
    );

    private const double VisibilityAnimationDurationSeconds = 0.5d;
    private const double InfoPanelDisplayItemMarginMultiplier = 1.25d;

    // No need to rebuild this every time.
    private readonly LibraMetrics _metrics = LibraMetrics.Default;
    private readonly IChartDisplay _chartDisplay;
    private InfoPanelText _infoPanelText;
    private AnimationSlot? _titleVisibilityAnimation;
    private AnimationSlot? _infoPanelVisibilityAnimation;
    private bool _connected;

    public ChartProperties Properties { get; }

    public ChartElement(ChartProperties properties,
        IChartDisplay chartDisplay,
        string atriaIdString)
    {
        Properties = properties;
        _chartDisplay = chartDisplay;
        _infoPanelText = _chartDisplay.GetInfoPanelText(Properties.DisplayedPercentiles);

        Id = AtriaId.Parse(atriaIdString);

        Connect();
    }

    public void Connect()
    {
        if (_connected) return;
        Properties.PropertiesChanged += Properties_PropertiesChanged;
        _chartDisplay.DataChanged += ChartDisplay_DataChanged;
        _connected = true;
    }

    public void Disconnect()
    {
        if (!_connected) return;
        Properties.PropertiesChanged -= Properties_PropertiesChanged;
        _connected = false;
    }

    private void Properties_PropertiesChanged(object? sender, EventArgs e)
    {
        _chartDisplay.OnContainerChanged();
    }

    private void ChartDisplay_DataChanged(object? sender, EventArgs e)
    {
        _infoPanelText = _chartDisplay.GetInfoPanelText(Properties.DisplayedPercentiles);
    }

    public void SetTitleBarVisibility(bool visible)
    {
        if ((visible && Properties.TitleVisibility is AnimatedVisiblity.Visible or AnimatedVisiblity.Appearing)
            || (!visible && Properties.TitleVisibility is AnimatedVisiblity.Invisible or AnimatedVisiblity.Disappearing)) return;

        _titleVisibilityAnimation ??= Animations.CreateSlot("ChartElement.TitleVisibility");
        var start = Properties.TitleVisibilityToggleProgress
            ?? (Properties.TitleVisibility == AnimatedVisiblity.Visible ? 1d : 0d);
        var end = visible ? 1d : 0d;
        Properties.BeginTitleVisibilityChange(visible, start);
        var frames = AnimationContext.SecondsToFrames(VisibilityAnimationDurationSeconds);
        _titleVisibilityAnimation.Replace(() => FixedDurationAnimation.StartNow(frames,
            progress => Properties.UpdateTitleVisibilityProgress(start + ((end - start) * progress)),
            () => Properties.CompleteTitleVisibilityChange(visible)), AnimationSlotReplacementBehavior.CancelExisting);
    }

    public void SetInfoPanelVisibility(bool visible)
    {
        if ((visible && Properties.InfoPanelVisibility is AnimatedVisiblity.Visible or AnimatedVisiblity.Appearing)
            || (!visible && Properties.InfoPanelVisibility is AnimatedVisiblity.Invisible or AnimatedVisiblity.Disappearing)) return;

        _infoPanelVisibilityAnimation ??= Animations.CreateSlot("ChartElement.InfoPanelVisibility");
        var start = Properties.InfoPanelVisibilityToggleProgress
            ?? (Properties.InfoPanelVisibility == AnimatedVisiblity.Visible ? 1d : 0d);
        var end = visible ? 1d : 0d;
        Properties.BeginInfoPanelVisibilityChange(visible, start);
        var frames = AnimationContext.SecondsToFrames(VisibilityAnimationDurationSeconds);
        _infoPanelVisibilityAnimation.Replace(() => FixedDurationAnimation.StartNow(frames,
            progress => Properties.UpdateInfoPanelVisibilityProgress(start + ((end - start) * progress)),
            () => Properties.CompleteInfoPanelVisibilityChange(visible)), AnimationSlotReplacementBehavior.CancelExisting);
    }

    public override void Render(IRenderTarget target)
    {
        // This is more fun than it looks, promise!
        _chartDisplay.AnimationContext ??= Animations;

        // Let's start by figuring out how big the title bar is. It spans the full width of the element.
        var titleBarNaturalHeight = Size.Height * Properties.TitleBarHeightRatioOfElement;
        var titleBarNaturalBounds = new SRectF(Position, new SSizeF(Size.Width, titleBarNaturalHeight));
        var titleBarHeight = Size.Height * Properties.CurrentTitleBarHeightRatioOfElement;
        var titleBarBounds = new SRectF(Position, new SSizeF(Size.Width, titleBarHeight));
        var nonTitleSize = new SSizeF(Size.Width, Size.Height - titleBarHeight);
        var nonTitlePosition = new SPointF(Position.X, Position.Y + titleBarHeight);
        var nonTitleBounds = new SRectF(nonTitlePosition, nonTitleSize);
        DrawTitleBar(titleBarBounds, titleBarNaturalBounds, target);

        // The same pattern applies to the info panel, which is at the right of the element and spans
        // the full height minus the title bar.
        var infoPanelNaturalWidth = Size.Width * Properties.InfoPanelWidthRatioOfElement;
        var infoPanelNaturalBounds = new SRectF(Bounds.Right - infoPanelNaturalWidth, Position.Y + titleBarHeight, infoPanelNaturalWidth, nonTitleSize.Height);
        var infoPanelWidth = Size.Width * Properties.CurrentInfoPanelWidthRatioOfElement;
        var infoPanelBounds = new SRectF(Bounds.Right - infoPanelWidth, titleBarHeight, infoPanelWidth, nonTitleSize.Height);
        var chartSize = new SSizeF(Size.Width - infoPanelWidth, nonTitleSize.Height);
        var chartPosition = new SPointF(Position.X, Position.Y + titleBarHeight);
        var chartBounds = new SRectF(chartPosition, chartSize);
        DrawInfoPanel(infoPanelBounds, infoPanelNaturalBounds, target);

        // Then, the chart itself!
        _chartDisplay.Render(target, chartBounds);
    }

    private void DrawTitleBar(SRectF bounds, SRectF naturalBounds, IRenderTarget target)
    {
        if (bounds.Height == 0d) { return; }

        // We animate the non-title area as visibly shrinking or growing, but in order to avoid the text looking
        // weird, we just draw it at its natural size and fade it in/out.

        // If we got down here, we're either animating to being visible/invisible, or we're fully visible.
        // So pick the opacity as either the progress of the animation, or 100%.
        var textOpacity = Properties.TitleVisibilityToggleProgress ?? 1d;
        var titleContext = BuildLibraRenderingContext(Properties.TitleFont);
        var titleLayout = Properties.TitleText.Layout(titleContext, Properties.TitleColor.WithOpacity(textOpacity));

        // We need to make sure the title text fits our bar at all.
        var horizontalScaleFactor = (titleLayout.Bounds.Width > naturalBounds.Width)
            ? naturalBounds.Width / titleLayout.Bounds.Width
            : 1d;
        var verticalScaleFactor = (titleLayout.Bounds.Height > naturalBounds.Height)
            ? naturalBounds.Height / titleLayout.Bounds.Height
            : 1d;
        var scaleFactor = Math.Min(horizontalScaleFactor, verticalScaleFactor);
        var scaledBounds = titleLayout.Bounds.Size * scaleFactor;
        var alignedPosition = AlignmentHelper.Align(Alignment.Center, naturalBounds, scaledBounds);
        RenderLibraRenderables(titleLayout, alignedPosition, scaleFactor, target);
    }

    private void DrawInfoPanel(SRectF bounds, SRectF naturalBounds, IRenderTarget target)
    {
        if (bounds.Width == 0) { return; }

        // Same deal as with the title bar: we animate the non-info area as visibly shrinking or growing
        // and fade in the info panel at natural size.
        var globalOpacity = Properties.InfoPanelVisibilityToggleProgress ?? 1d;
        var font = Properties.InfoPanelBaseFont;
        var context = BuildLibraRenderingContext(font);
        var labelColor = Properties.InfoPanelLabelColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d);

        var borderColor = Properties.InfoPanelBorderColor.WithOpacity(globalOpacity);
        var backgroundColor = Properties.InfoPanelBackgroundColor.WithOpacity(globalOpacity);

        target.DrawRectangle(bounds, backgroundColor, SPaintStyle.Fill, SAngle.Zero);
        target.DrawRectangleOfThickness(
            bounds,
            Properties.InfoPanelBorderThickness,
            borderColor);

        // But, oh, is there so much to do.
        var root = new LayoutNode();
        var contentBoundsNode = root.Inset(Properties.InfoPanelPaddingRatio, Properties.InfoPanelPaddingRatio, "contentBounds");
        if (!contentBoundsNode.TryGetBoundsFor("contentBounds", naturalBounds, out var contentBounds))
        {
            throw new InvalidOperationException("Failed to get content bounds for info panel.");
        }

        // Okay, maybe not THAT much to do.
        var stacker = new LayoutStacker(
        Direction.Vertical,
        Properties.InfoPanelSummaryItemMargin,
        InfoPanelDisplayItemMarginMultiplier);

        foreach (var row in GetInfoPanelRows())
        {
            DrawInfoPanelRow(
                row,
                stacker,
                contentBounds.Value.Left,
                contentBounds.Value.Right,
                contentBounds.Value.Position,
                font,
                labelColor,
                target);
        }
    }

    private IEnumerable<InfoPanelRow> GetInfoPanelRows()
    {
        var text = _infoPanelText;

        yield return new("Current", text.CurrentValueText, text.CurrentValueAlternateText);
        yield return new("Minimum", text.MinimumText, text.MinimumAlternateText);
        yield return new("Maximum", text.MaximumText, text.MaximumAlternateText);
        yield return new("Range", text.RangeText, text.RangeAlternateText);
        yield return new("Midpoint", text.MidpointText, text.MidpointAlternateText);
        yield return new("Mean", text.MeanText, text.MeanAlternateText);
        yield return new("Median", text.MedianText, text.MedianAlternateText);
        yield return new("Mode", text.ModeText, text.ModeAlternateText);
        yield return new(
            "Population standard deviation",
            text.PopulationStandardDeviationText,
            text.PopulationStandardDeviationAlternateText);
        yield return new(
            "Sample standard deviation",
            text.SampleStandardDeviationText,
            text.SampleStandardDeviationAlternateText);

        if (text.Percentiles is not null)
        {
            foreach (var percentile in text.Percentiles)
            {
                yield return new(
                    $"{percentile.Percentile:0.##}th percentile",
                    percentile.PercentileText,
                    null);
            }
        }

        yield return new("Count and sum", text.CountAndSumText, null);
    }

    private void DrawInfoPanelRow(
    InfoPanelRow row,
    LayoutStacker stacker,
    double xMin,
    double xMax,
    SPointF offset,
    SFont font,
    SColor labelColor,
    IRenderTarget target)
    {
        if (row.Value is null && row.AlternateValue is null)
        {
            return;
        }

        var labelSize = target.MeasureText(row.Label, font);
        var valueLayout = LayoutInfoPanelValue(row.Value, useAlternateColor: false);
        var alternateLayout = LayoutInfoPanelValue(row.AlternateValue, useAlternateColor: true);

        var valueSize = valueLayout.Bounds.Size;
        var alternateSize = alternateLayout.Bounds.Size;

        // LayoutStacker requires positive sizes. Only place layouts that exist.
        var valueX = valueSize.Width > 0d
            ? stacker.AlignOnBoundedMinorAxis(valueSize, xMin, xMax, Alignment.RightCenter)
            : xMax;

        var labelAndValueBounds = valueSize.Width > 0d
            ? stacker.PlaceAtSameMajorPosition(
                [(labelSize, 0d), (valueSize, valueX)],
                -1)
            : [stacker.Place(labelSize, 0d, -1)];

        target.DrawText(
            row.Label,
            font,
            labelAndValueBounds[0] + offset,
            labelColor,
            SAngle.Zero);

        if (valueSize.Width > 0d)
        {
            RenderLibraRenderables(
                valueLayout,
                labelAndValueBounds[1].Position + offset,
                1d,
                target);
        }

        if (alternateSize.Width > 0d)
        {
            var alternateX = stacker.AlignOnBoundedMinorAxis(
                alternateSize,
                xMin,
                xMax,
                Alignment.RightCenter);

            var alternateBounds = stacker.Place(alternateSize, alternateX, 1);

            RenderLibraRenderables(
                alternateLayout,
                alternateBounds.Position + offset,
                1d,
                target);
        }
    }

    private LibraLayoutResult LayoutInfoPanelValue(ChartText? valueText,
        bool useAlternateColor)
    {
        if (valueText == null)
        {
            return LibraLayoutResult.Empty;
        }

        var color = useAlternateColor ? Properties.InfoPanelSecondaryColor : Properties.InfoPanelValueColor;
        var context = BuildLibraRenderingContext(Properties.InfoPanelBaseFont);
        return valueText.Layout(context, color.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d));
    }

    private LibraRenderingContext BuildLibraRenderingContext(SFont font) => new(Slide!.MeasurementService!,
            font,
            _metrics,
            FenceRenderingMode.Procedural);

    private void RenderLibraRenderables(LibraLayoutResult layout, SPointF position, double scaleFactor, IRenderTarget target)
    {
        foreach (var renderable in layout.Renderables)
        {
            var scaledPosition = (renderable.Position * scaleFactor) + position;
            renderable.RenderAt(target, scaledPosition, scaleFactor);
        }
    }
}
