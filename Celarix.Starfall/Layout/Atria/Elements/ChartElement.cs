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
using System.Diagnostics;
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
    private const float MinimumWarnableRenderedFontSize = 8f;

    // No need to rebuild this every time.
    private readonly LibraMetrics _metrics = LibraMetrics.Default;
    private readonly List<PositionedLibraRenderable> _infoPanelRenderables = [];
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
        _titleVisibilityAnimation.Replace(() => Animations.StartNow(frames,
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
        _infoPanelVisibilityAnimation.Replace(() => Animations.StartNow(frames,
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
        // target.DrawRectangle(contentBounds!.Value, SColor.Red, SPaintStyle.Stroke, SAngle.Zero);

        // Okay, maybe not THAT much to do.
        if (_rebuildInfoPanel)
        {
            var stacker = new LayoutStacker(
                Direction.Vertical,
                Properties.InfoPanelSummaryItemMargin,
                InfoPanelDisplayItemMarginMultiplier);
            _infoPanelRenderables.Clear();
            _infoPanelRenderActions.Clear();

            LayoutCurrentValue(target, stacker, contentBounds.Value);
            LayoutRangeSection(target, stacker, contentBounds.Value);
            LayoutInfoPanelRow(Properties.VisibleDisplays.HasFlag(InfoPanelSummaries.Mean),
                new("Mean", _infoPanelText.MeanText, _infoPanelText.MeanAlternateText),
                stacker,
                contentBounds.Value.Left,
                contentBounds.Value.Right,
                contentBounds.Value.Position,
                font,
                labelColor,
                target);
            LayoutInfoPanelRow(Properties.VisibleDisplays.HasFlag(InfoPanelSummaries.Median),
                new("Median", _infoPanelText.MedianText, _infoPanelText.MedianAlternateText),
                stacker,
                contentBounds.Value.Left,
                contentBounds.Value.Right,
                contentBounds.Value.Position,
                font,
                labelColor,
                target);
            LayoutInfoPanelRow(Properties.VisibleDisplays.HasFlag(InfoPanelSummaries.Mode),
                new("Mode", _infoPanelText.ModeText, _infoPanelText.ModeAlternateText),
                stacker,
                contentBounds.Value.Left,
                contentBounds.Value.Right,
                contentBounds.Value.Position,
                font,
                labelColor,
                target);
            _rebuildInfoPanel = false;
        }

        foreach (var renderable in _infoPanelRenderables)
        {
            renderable.Render(target);
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

        var offset = new SPointF(0, contentBounds.Y);

        var currentText = _infoPanelText.CurrentValueText;
        var currentValueAlternate = _infoPanelText.CurrentValueAlternateText;

        var currentLayout = LayoutInfoPanelValue(currentText, isAlternate: false);
        var alternateLayout = LayoutInfoPanelValue(currentValueAlternate, isAlternate: true);

        if (currentLayout == null && alternateLayout == null)
        {
            return;
        }

        var currentLayoutScaleFactor = (currentLayout != null && currentLayout.Bounds.Width > contentBounds.Width)
            ? contentBounds.Width / currentLayout.Bounds.Width
            : 1d;
        var alternateLayoutScaleFactor = (alternateLayout != null && alternateLayout.Bounds.Width > contentBounds.Width)
            ? contentBounds.Width / alternateLayout.Bounds.Width
            : 1d;

        var currentBounds = (currentLayout != null)
            ? stacker.PlaceWithNoMargin(currentLayout.Bounds.Size * currentLayoutScaleFactor, contentBounds.Left, contentBounds.Right, Alignment.RightCenter)
            : (SRectF?)null;
        var alternateBounds = (alternateLayout != null)
            ? stacker.PlaceWithNoMargin(alternateLayout.Bounds.Size * alternateLayoutScaleFactor, contentBounds.Left, contentBounds.Right, Alignment.RightCenter)
            : (SRectF?)null;

        // If we got down here, at least ONE of them must be non-null, so add a margin.
        stacker.PlaceMargin(1);

        if (currentLayout != null)
        {
            currentBounds = currentBounds!.Value.DownBy(contentBounds.Y);
            _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(currentLayout,
                currentBounds!.Value.Position, currentLayoutScaleFactor));
        }

        if (alternateLayout != null)
        {
            alternateBounds = alternateBounds!.Value.DownBy(contentBounds.Y);
            _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(alternateLayout,
                alternateBounds!.Value.Position, alternateLayoutScaleFactor));
        }
    }

    private void LayoutRangeSection(IRenderTarget target, LayoutStacker stacker, SRectF contentBounds)
    {
        if (!Properties.VisibleDisplays.HasFlag(InfoPanelSummaries.RangeLine))
        {
            return;
        }

        var baseFont = Properties.InfoPanelBaseFont;
        var em = target.MeasureText("M", baseFont).Height;
        var stackMargin = em / 4d;
        var railGap = em / 2d;
        var railThickness = Math.Max(1d, Properties.InfoPanelBorderThickness * 2d);

        var minimumLayout = LayoutInfoPanelValue(_infoPanelText.MinimumText, isAlternate: false);
        var minimumAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MinimumAlternateText, isAlternate: true);
        var minimumStack = CreateValueStack(minimumLayout, minimumAlternateLayout, stackMargin);

        var maximumLayout = LayoutInfoPanelValue(_infoPanelText.MaximumText, isAlternate: false);
        var maximumAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MaximumAlternateText, isAlternate: true);
        var maximumStack = CreateValueStack(maximumLayout, maximumAlternateLayout, stackMargin);

        var rangeLayout = LayoutInfoPanelValue(_infoPanelText.RangeText, isAlternate: false);
        var rangeAlternateLayout = LayoutInfoPanelValue(_infoPanelText.RangeAlternateText, isAlternate: true);
        var rangeStack = CreateValueStack(rangeLayout, rangeAlternateLayout, stackMargin);

        var midpointLayout = LayoutInfoPanelValue(_infoPanelText.MidpointText, isAlternate: false);
        var midpointAlternateLayout = LayoutInfoPanelValue(_infoPanelText.MidpointAlternateText, isAlternate: true);
        var midpointStack = CreateValueStack(midpointLayout, midpointAlternateLayout, stackMargin);

        // The portrait panel gives endpoint values a wide label column and reserves a narrow
        // vertical rail at its right. Derived values then use the entire width below the rail.
        var endpointLabelWidth = Math.Max(0d, contentBounds.Width - railGap - railThickness);
        var top = stacker.MajorAxisPosition + contentBounds.Y;
        var maximumSize = AddValueStack(maximumStack,
            new SRectF(contentBounds.Left, top, endpointLabelWidth, 0d),
            Alignment.RightCenter, "maximum", _infoPanelText.MaximumText != null, _infoPanelText.MaximumAlternateText != null);
        var maximumCenterY = top + (maximumSize.Height / 2d);
        var railHeight = Math.Max(em * 4d, Math.Max(maximumSize.Height, em));
        var minimumCenterY = maximumCenterY + railHeight;
        var minimumTop = minimumCenterY - (FittedStackSize(minimumStack, endpointLabelWidth).Height / 2d);
        var minimumSize = AddValueStack(minimumStack,
            new SRectF(contentBounds.Left, minimumTop, endpointLabelWidth, 0d),
            Alignment.RightCenter, "minimum", _infoPanelText.MinimumText != null, _infoPanelText.MinimumAlternateText != null);
        var endpointBottom = Math.Max(top + maximumSize.Height, minimumTop + minimumSize.Height);

        var railX = contentBounds.Right - railThickness;
        _infoPanelRenderActions.Add(t =>
        {
            var lineColor = Properties.InfoPanelBorderColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d);
            t.DrawRectangle(new SRectF(railX, maximumCenterY, railThickness, minimumCenterY - maximumCenterY), lineColor, SPaintStyle.Fill, SAngle.Zero);
            t.DrawRectangle(new SRectF(railX - railGap, maximumCenterY, railGap + railThickness, railThickness), lineColor, SPaintStyle.Fill, SAngle.Zero);
            t.DrawRectangle(new SRectF(railX - railGap, minimumCenterY - railThickness, railGap + railThickness, railThickness), lineColor, SPaintStyle.Fill, SAngle.Zero);
        });

        var summaryTop = endpointBottom + em;
        var columnGap = em / 2d;
        var columnWidth = Math.Max(0d, (contentBounds.Width - columnGap) / 2d);
        var rangeColumn = new SRectF(contentBounds.Left, summaryTop, columnWidth, 0d);
        var midpointColumn = new SRectF(contentBounds.Left + columnWidth + columnGap, summaryTop, columnWidth, 0d);
        var rangeSummaryHeight = AddSummaryColumn("Range", rangeStack, rangeColumn, stackMargin,
            _infoPanelText.RangeText != null, _infoPanelText.RangeAlternateText != null);
        var midpointSummaryHeight = AddSummaryColumn("Midpoint", midpointStack, midpointColumn, stackMargin,
            _infoPanelText.MidpointText != null, _infoPanelText.MidpointAlternateText != null);

        stacker.Place(new SSizeF(contentBounds.Width,
            (summaryTop - top) + Math.Max(rangeSummaryHeight, midpointSummaryHeight)), 0d, 1);
    }

    private IReadOnlyList<PositionedLibraRenderable> CreateValueStack(LibraLayoutResult? primary, LibraLayoutResult? alternate, double marginPx)
    {
        if (primary == null && alternate == null)
        {
            return [];
        }

        if (primary != null && alternate == null)
        {
            return PositionedLibraRenderable.FromLayout(primary!, SPointF.Zero, 1.0d).ToArray();
        }

        if (primary == null && alternate != null)
        {
            return PositionedLibraRenderable.FromLayout(alternate!, SPointF.Zero, 1.0d).ToArray();
        }

        // Place the alternate centered below the primary with the specified margin.
        if (primary!.Bounds.Width > alternate!.Bounds.Width)
        {
            var alternateXInset = AlignmentHelper.AlignAxis(primary.Bounds.Width, alternate.Bounds.Width, Alignment.Center);
            alternate = alternate.Translate(new SPointF(alternateXInset, primary.Bounds.Height + marginPx));
            return [.. PositionedLibraRenderable.FromLayout(primary, SPointF.Zero, 1.0d), .. PositionedLibraRenderable.FromLayout(alternate, SPointF.Zero, 1.0d)];
        }

        if (alternate.Bounds.Width > primary.Bounds.Width)
        {
            var primaryXInset = AlignmentHelper.AlignAxis(alternate.Bounds.Width, primary.Bounds.Width, Alignment.Center);
            primary = primary.Translate(new SPointF(primaryXInset, 0d));
            alternate = alternate.Translate(new SPointF(0d, primary.Bounds.Height + marginPx));
            return [.. PositionedLibraRenderable.FromLayout(primary, SPointF.Zero, 1.0d), .. PositionedLibraRenderable.FromLayout(alternate, SPointF.Zero, 1.0d)];

        }
        else
        {
            // Same width, so just stack them.
            alternate = alternate.Translate(new SPointF(0d, primary.Bounds.Height + marginPx));
            return [.. PositionedLibraRenderable.FromLayout(primary, SPointF.Zero, 1.0d), .. PositionedLibraRenderable.FromLayout(alternate, SPointF.Zero, 1.0d)];
        }
    }

    private double AddSummaryColumn(string label,
        IReadOnlyList<PositionedLibraRenderable> valueStack,
        SRectF columnBounds,
        double stackMargin,
        bool hasPrimaryValue,
        bool hasAlternateValue)
    {
        var labelLayout = ChartText.String(label).Layout(
            BuildLibraRenderingContext(Properties.InfoPanelBaseFont),
            Properties.InfoPanelLabelColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d));
        var labelX = columnBounds.Left + AlignmentHelper.AlignAxis(columnBounds.Width, labelLayout.Bounds.Width, Alignment.Center);
        _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(labelLayout,
            new SPointF(labelX - labelLayout.Bounds.X, columnBounds.Top - labelLayout.Bounds.Y), 1d));

        var valueTop = columnBounds.Top + labelLayout.Bounds.Height + stackMargin;
        var valueSize = AddValueStack(valueStack,
            new SRectF(columnBounds.Left, valueTop, columnBounds.Width, 0d),
            Alignment.Center, label.ToLowerInvariant(), hasPrimaryValue, hasAlternateValue);
        return labelLayout.Bounds.Height + stackMargin + valueSize.Height;
    }

    private SSizeF AddValueStack(IReadOnlyList<PositionedLibraRenderable> stack,
        SRectF availableBounds,
        Alignment alignment,
        string regionName,
        bool hasPrimaryValue,
        bool hasAlternateValue)
    {
        var stackBounds = StackBounds(stack);
        if (stackBounds.Width <= 0d || stackBounds.Height <= 0d || availableBounds.Width <= 0d)
        {
            return SSizeF.Zero;
        }

        var scaleFactor = StackScaleToFit(stackBounds, availableBounds.Width);
        var scaledSize = stackBounds.Size * scaleFactor;
        var x = availableBounds.Left + AlignmentHelper.AlignAxis(availableBounds.Width, scaledSize.Width, alignment);
        var offset = new SPointF(x - (stackBounds.X * scaleFactor),
            availableBounds.Top - (stackBounds.Y * scaleFactor));
        _infoPanelRenderables.AddRange(stack.Select(renderable => new PositionedLibraRenderable(
            renderable.Renderable,
            (renderable.Position * scaleFactor) + offset,
            renderable.ScaleFactor * scaleFactor)));

        WarnIfTextStackIsTooSmall(regionName, scaleFactor, hasPrimaryValue, hasAlternateValue);
        return scaledSize;
    }

    private void WarnIfTextStackIsTooSmall(string regionName, double scaleFactor,
        bool hasPrimaryValue, bool hasAlternateValue)
    {
        var baseFontSize = Properties.InfoPanelBaseFont.Size ?? 12f;
        var primaryFontSize = baseFontSize * (float)Properties.InfoPanelFontSizeMultiplierStep;
        var smallestFontSize = hasPrimaryValue && hasAlternateValue
            ? Math.Min(primaryFontSize, baseFontSize)
            : hasPrimaryValue ? primaryFontSize : baseFontSize;
        var renderedFontSize = smallestFontSize * (float)scaleFactor;
        if (renderedFontSize <= MinimumWarnableRenderedFontSize)
        {
            Debug.WriteLine($"Chart info panel {regionName} stack rendered at {renderedFontSize:F1}pt.");
        }
    }

    private static SRectF StackBounds(IReadOnlyList<PositionedLibraRenderable> stack) =>
        stack.Count == 0 ? SRectF.Empty : SRectF.BoundsOf(stack.Select(renderable => renderable.Bounds));

    private static SSizeF FittedStackSize(IReadOnlyList<PositionedLibraRenderable> stack, double availableWidth)
    {
        var stackBounds = StackBounds(stack);
        return stackBounds.Size * StackScaleToFit(stackBounds, availableWidth);
    }

    private static double StackScaleToFit(SRectF stackBounds, double availableWidth) =>
        stackBounds.Width <= 0d || availableWidth <= 0d ? 0d : Math.Min(1d, availableWidth / stackBounds.Width);

    private void LayoutInfoPanelRow(
        bool displayEnabled,
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

        if (!displayEnabled)
        {
            return;
        }

        var labelSize = target.MeasureText(row.Label, font);
        var labelLayout = ChartText.String(row.Label)
            .Layout(BuildLibraRenderingContext(font), labelColor.WithOpacity(Properties.InfoPanelVisibilityToggleProgress ?? 1d));

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

        _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(
            labelLayout,
            labelAndValueBounds[0].Position + offset,
            1d));

        if (valueSize.Width > 0d)
        {
            _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(
                valueLayout,
                labelAndValueBounds[1].Position + offset,
                1d));
        }

        if (alternateSize.Width > 0d)
        {
            var alternateX = stacker.AlignOnBoundedMinorAxis(
                alternateSize,
                xMin,
                xMax,
                Alignment.RightCenter);

            var alternateBounds = stacker.Place(alternateSize, alternateX, 1);

            _infoPanelRenderables.AddRange(PositionedLibraRenderable.FromLayout(
                alternateLayout,
                alternateBounds.Position + offset,
                1d));
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
