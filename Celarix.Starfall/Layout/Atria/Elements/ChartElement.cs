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
    private readonly List<LibraRenderable> _infoPanelRenderables = [];
    private readonly List<Action<IRenderTarget>> _infoPanelRenderActions = [];
    private bool _rebuildInfoPanel = true;
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
        _rebuildInfoPanel = true;
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

        // Debug
        target.DrawRectangle(contentBounds!.Value, SColor.Red, SPaintStyle.Stroke, SAngle.Zero);

        // Okay, maybe not THAT much to do.
        if (_rebuildInfoPanel)
        {
            var stacker = new LayoutStacker(
                Direction.Vertical,
                Properties.InfoPanelSummaryItemMargin,
                InfoPanelDisplayItemMarginMultiplier);
            _infoPanelRenderables.Clear();
            _infoPanelRenderActions.Clear();

            //foreach (var row in GetInfoPanelRows())
            //{
            //    DrawInfoPanelRow(
            //        row,
            //        stacker,
            //        contentBounds.Value.Left,
            //        contentBounds.Value.Right,
            //        contentBounds.Value.Position,
            //        font,
            //        labelColor,
            //        target);
            //}

            LayoutCurrentValue(target, stacker, contentBounds.Value);
            LayoutRangeSection(target, stacker, contentBounds.Value);
            _rebuildInfoPanel = false;
        }

        foreach (var renderable in _infoPanelRenderables)
        {
            // When we laid out the renderables, they already were positioned relative to the content
            // bounds, so we don't need to add the content bounds position here.
            renderable.RenderAt(target, renderable.Position, 1d);
        }

        foreach (var action in _infoPanelRenderActions)
        {
            action(target);
        }
    }

    private void LayoutCurrentValue(IRenderTarget target, LayoutStacker stacker, SRectF contentBounds)
    {
        if (!Properties.VisibleDisplays.HasFlag(InfoPanelSummaries.CurrentValue))
        {
            return;
        }

        var downwardOffset = new SPointF(0, contentBounds.Y);

        var currentText = _infoPanelText.CurrentValueText;
        var currentValueAlternate = _infoPanelText.CurrentValueAlternateText;

        var currentLayout = LayoutInfoPanelValue(currentText, isAlternate: false)?.ScaleToFitWidth(contentBounds.Width);
        var alternateLayout = LayoutInfoPanelValue(currentValueAlternate, isAlternate: true)?.ScaleToFitWidth(contentBounds.Width);

        if (currentLayout == null && alternateLayout == null)
        {
            return;
        }

        var currentBounds = (currentLayout != null)
            ? stacker.PlaceWithNoMargin(currentLayout.Bounds.Size, contentBounds.Left, contentBounds.Right, Alignment.RightCenter)
            : (SRectF?)null;
        var alternateBounds = (alternateLayout != null)
            ? stacker.PlaceWithNoMargin(alternateLayout.Bounds.Size, contentBounds.Left, contentBounds.Right, Alignment.RightCenter)
            : (SRectF?)null;

        // If we got down here, at least ONE of them must be non-null, so add a margin.
        stacker.PlaceMargin(1);

        if (currentLayout != null)
        {
            currentLayout = currentLayout.Translate(currentBounds!.Value.Position + downwardOffset);
        }

        if (alternateLayout != null)
        {
            alternateLayout = alternateLayout.Translate(alternateBounds!.Value.Position + downwardOffset);
        }

        _infoPanelRenderables.AddRange(currentLayout?.Renderables ?? []);
        _infoPanelRenderables.AddRange(alternateLayout?.Renderables ?? []);
    }

    private void LayoutRangeSection(IRenderTarget target, LayoutStacker stacker, SRectF contentBounds)
    {
        // This one's fun - dynamically resizing stuff based on how big we render it. We'll be drawing:
        //  RANGE
        // ┌─────┐
        // MIN–MAX
        //  (mid)

        var majorFont = Properties.InfoPanelBaseFont.WithSize((float)((Properties.InfoPanelBaseFont.Size ?? 12f) * Properties.InfoPanelFontSizeMultiplierStep));

        // Start with the en-dash.
        var enDashSize = target.MeasureText("–", majorFont);

        // Pad it out by 1/2 en on the left and right.
        var halfEn = enDashSize.Width / 2d;
        var quarterEn = enDashSize.Width / 4d;
        var paddedEnDashSize = new SSizeF(enDashSize.Width + (halfEn * 2d), enDashSize.Height);

        // Layout the minimum and minimum alternate.
        var minimumLayout = LayoutInfoPanelValue(_infoPanelText.MinimumText, isAlternate: false);
        var minimumAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MinimumAlternateText, isAlternate: true);
        var minimumLocalStack = CreateLocalStackForRange(minimumLayout, minimumAlternateLayout, quarterEn);
        var maximumLocalStackBounds = SRectF.BoundsOf(minimumLocalStack.Select(r => r.Bounds));

        // Then the maximum and maximum alternate.
        var maximumLayout = LayoutInfoPanelValue(_infoPanelText.MaximumText, isAlternate: false);
        var maximumAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MaximumAlternateText, isAlternate: true);
        var maximumLocalStack = CreateLocalStackForRange(maximumLayout, maximumAlternateLayout, quarterEn);
        var minimumLocalStackBounds = SRectF.BoundsOf(maximumLocalStack.Select(r => r.Bounds));

        // Compute the full width of the three of them.
        var rangeWidth = minimumLocalStackBounds.Width + paddedEnDashSize.Width + maximumLocalStackBounds.Width;
        var enDashX = minimumLocalStackBounds.Width + halfEn;

        // Get the size of the horizontal bracket.
        var bracketSize = new SSizeF(rangeWidth, halfEn);

        // Layout the range and range alternate.
        var rangeLayout = LayoutInfoPanelValue(_infoPanelText.RangeText, isAlternate: false);
        var rangeAlternateLayout = LayoutInfoPanelValue(_infoPanelText.RangeAlternateText, isAlternate: true);
        var rangeLocalStack = CreateLocalStackForRange(rangeLayout, rangeAlternateLayout, quarterEn);
        var rangeLocalStackBounds = SRectF.BoundsOf(rangeLocalStack.Select(r => r.Bounds));

        // Layout the midpoint and midpoint alternate. Make them smaller than the rest of the text, since they're less important.
        var midpointLayout = LayoutInfoPanelValue(_infoPanelText.MidpointText, isAlternate: false)?.Scale(1d / Properties.InfoPanelFontSizeMultiplierStep);
        var midpointAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MidpointAlternateText, isAlternate: true)?.Scale(1d / Properties.InfoPanelFontSizeMultiplierStep);
        var midpointLocalStack = CreateLocalStackForRange(midpointLayout, midpointAlternateLayout, quarterEn);
        var midpointLocalStackBounds = SRectF.BoundsOf(midpointLocalStack.Select(r => r.Bounds));

        // Start figuring out Y coordinates.
        var top = stacker.MajorAxisPosition + contentBounds.Y;
        var rangeLocalStackY = top;
        var bracketY = rangeLocalStackY + quarterEn + rangeLocalStackBounds.Height;
        var minMaxOuterHeight = Math.Max(minimumLocalStackBounds.Height, Math.Max(maximumLocalStackBounds.Height, enDashSize.Height));
        var minimumLocalStackY = bracketY + bracketSize.Height
            + AlignmentHelper.AlignAxis(minMaxOuterHeight, minimumLocalStackBounds.Height, Alignment.Center);
        var maximumLocalStackY = bracketY + bracketSize.Height
            + AlignmentHelper.AlignAxis(minMaxOuterHeight, maximumLocalStackBounds.Height, Alignment.Center);
        var enDashY = bracketY + bracketSize.Height
            + AlignmentHelper.AlignAxis(minMaxOuterHeight, enDashSize.Height, Alignment.Center);
        // Hey, it's still easier than center-aligning a <div> now, isn't it?
        var minMaxBottom = Math.Max(minimumLocalStackY + minimumLocalStackBounds.Height,
            Math.Max(maximumLocalStackY + maximumLocalStackBounds.Height, enDashY + enDashSize.Height));
        var midpointY = minMaxBottom + quarterEn;

        // Bound the whole thing and center it in content bounds.
        var scaleFactor = 1d;
        var widestWidth = Math.Max(rangeLocalStackBounds.Width, Math.Max(rangeWidth, midpointLocalStackBounds.Width));
        if (widestWidth > contentBounds.Width)
        {
            scaleFactor = contentBounds.Width / widestWidth;
        }
        var bounds = new SRectF(0d, 0d, widestWidth * scaleFactor, (midpointY + midpointLocalStackBounds.Height) * scaleFactor);
        var centerOffset = contentBounds.Left + AlignmentHelper.CenterAlign(contentBounds.Width, bounds.Width);

        // Figure out the remaining X coordinates.
        var rangeLocalStackX = centerOffset + AlignmentHelper.AlignAxis(bounds.Width, rangeLocalStackBounds.Width * scaleFactor, Alignment.Center);
        var minimumLocalStackX = centerOffset + AlignmentHelper.AlignAxis(bounds.Width, rangeWidth * scaleFactor, Alignment.Center);
        var enDashLocalStackX = minimumLocalStackX + (minimumLocalStackBounds.Width * scaleFactor) + (halfEn * scaleFactor);
        var maximumLocalStackX = enDashLocalStackX + (halfEn * scaleFactor);
        var midPointX = centerOffset + AlignmentHelper.AlignAxis(bounds.Width, midpointLocalStackBounds.Width * scaleFactor, Alignment.Center);

        // Add the renderables and actions to the lists.
        _infoPanelRenderables.AddRange(rangeLocalStack.Select(r => ScaleAndTranslate(r, new SPointF(rangeLocalStackX, rangeLocalStackY))));
        _infoPanelRenderActions.Add(t =>
        {
            var lineColor = Properties.InfoPanelBorderColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d);
            var lineWidth = Properties.InfoPanelBorderThickness * 4d;   // totally arbitrary, but it looks good
            var left = contentBounds.Left + centerOffset;   // yes, capture over everything, why not
            var right = (left + (rangeWidth * scaleFactor)) - lineWidth;
            // Draw the vertical bracket lines.
            t.DrawRectangle(new(left, bracketY, lineWidth, bracketSize.Height), lineColor, SPaintStyle.Fill, SAngle.Zero);
            t.DrawRectangle(new(right, bracketY, lineWidth, bracketSize.Height), lineColor, SPaintStyle.Fill, SAngle.Zero);
            // Draw the horizontal bracket line.
            t.DrawRectangle(new(left, bracketY, rangeWidth, lineWidth), lineColor, SPaintStyle.Fill, SAngle.Zero);
        });
        _infoPanelRenderables.AddRange(minimumLocalStack.Select(r => ScaleAndTranslate(r, new SPointF(minimumLocalStackX, minimumLocalStackY))));
        _infoPanelRenderables.AddRange(maximumLocalStack.Select(r => ScaleAndTranslate(r, new SPointF(maximumLocalStackX, maximumLocalStackY))));
        _infoPanelRenderActions.Add(t =>
        {
            var enDashColor = Properties.InfoPanelLabelColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d);
            t.DrawText("–", majorFont, new(new(enDashLocalStackX * scaleFactor, enDashY * scaleFactor), enDashSize * scaleFactor), enDashColor, SAngle.Zero);
        });
        _infoPanelRenderables.AddRange(midpointLocalStack.Select(r => ScaleAndTranslate(r, new SPointF(midPointX, midpointY))));

        // Now adjust the stacker to account for the space we just used.
        stacker.Place(new SSizeF(bounds.Width, bounds.Height), 0d, 1);

        LibraRenderable ScaleAndTranslate(LibraRenderable original, SPointF offset)
        {
            return original.ScaleAndTranslate(scaleFactor, offset);
        }
    }

    private IReadOnlyList<LibraRenderable> CreateLocalStackForRange(LibraLayoutResult? primary, LibraLayoutResult? alternate, double marginPx)
    {
        if (primary == null && alternate == null)
        {
            return [];
        }

        if (primary != null && alternate == null)
        {
            return primary.Renderables;
        }

        if (primary == null && alternate != null)
        {
            return alternate.Renderables;
        }

        // Place the alternate centered below the primary with the specified margin.
        if (primary!.Bounds.Width > alternate!.Bounds.Width)
        {
            var alternateXInset = AlignmentHelper.AlignAxis(primary.Bounds.Width, alternate.Bounds.Width, Alignment.Center);
            alternate = alternate.Translate(new SPointF(alternateXInset, primary.Bounds.Height + marginPx));
            return [.. primary.Renderables, .. alternate.Renderables];
        }

        if (alternate.Bounds.Width > primary.Bounds.Width)
        {
            var primaryXInset = AlignmentHelper.AlignAxis(alternate.Bounds.Width, primary.Bounds.Width, Alignment.Center);
            primary = primary.Translate(new SPointF(primaryXInset, 0d));
            alternate = alternate.Translate(new SPointF(0d, primary.Bounds.Height + marginPx));
            return [.. primary.Renderables, .. alternate.Renderables];

        }
        else
        {
            // Same width, so just stack them.
            alternate = alternate.Translate(new SPointF(0d, primary.Bounds.Height + marginPx));
            return [.. primary.Renderables, .. alternate.Renderables];
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
        var valueLayout = LayoutInfoPanelValue(row.Value, isAlternate: false);
        var alternateLayout = LayoutInfoPanelValue(row.AlternateValue, isAlternate: true);

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
        bool isAlternate)
    {
        if (valueText == null)
        {
            return LibraLayoutResult.Empty;
        }

        var color = isAlternate ? Properties.InfoPanelSecondaryColor : Properties.InfoPanelValueColor;
        var fontSize = isAlternate
            ? (Properties.InfoPanelBaseFont.Size ?? 12f)
            : (Properties.InfoPanelBaseFont.Size ?? 12f) * Properties.InfoPanelFontSizeMultiplierStep;
        var context = BuildLibraRenderingContext(Properties.InfoPanelBaseFont.WithSize((float)fontSize));
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
