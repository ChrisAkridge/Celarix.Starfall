using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Extensions;
using Celarix.Starfall.Layout.Atria.Components;
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
    private const double InfoPanelDisplayItemMarginMultiplier = 1.25d;

    // No need to rebuild this every time.
    private readonly LibraMetrics _metrics = LibraMetrics.Default;

    public ChartProperties Properties { get; }

    public ChartElement(ChartProperties properties, string atriaIdString)
    {
        Properties = properties;
        Id = AtriaId.Parse(atriaIdString);
    }

    public override void Render(IRenderTarget target)
    {
        // This is more fun than it looks, promise!

        // Let's start by figuring out how big the title bar is. It spans the full width of the element.
        var titleBarNaturalHeight = Size.Height * Properties.TitleBarHeightRatioOfElement;
        var titleBarNaturalBounds = new SRectF(Position, new SSizeF(Size.Width, titleBarNaturalHeight));
        var titleBarHeight = titleBarNaturalHeight * (Properties.TitleVisibilityToggleProgress ?? 0.0);
        var titleBarBounds = new SRectF(Position, new SSizeF(Size.Width, titleBarHeight));
        var nonTitleSize = new SSizeF(Size.Width, Size.Height - titleBarHeight);
        var nonTitlePosition = new SPointF(Position.X, Position.Y + titleBarHeight);
        var nonTitleBounds = new SRectF(nonTitlePosition, nonTitleSize);
        DrawTitleBar(titleBarBounds, titleBarNaturalBounds, target);

        // The same pattern applies to the info panel, which is at the right of the element and spans
        // the full height minus the title bar.
        var infoPanelNaturalWidth = Size.Width * Properties.InfoPanelWidthRatioOfElement;
        var infoPanelNaturalBounds = new SRectF(Size.Width - infoPanelNaturalWidth, Position.Y + titleBarHeight, infoPanelNaturalWidth, nonTitleSize.Height);
        var infoPanelWidth = infoPanelNaturalWidth * (Properties.InfoPanelVisibilityToggleProgress ?? 0.0);
        var infoPanelBounds = new SRectF(Size.Width - infoPanelWidth, titleBarHeight, infoPanelWidth, nonTitleSize.Height)
            + Position;
        var chartSize = new SSizeF(Size.Width - infoPanelWidth, nonTitleSize.Height);
        var chartPosition = new SPointF(Position.X, Position.Y + titleBarHeight);
        var chartBounds = new SRectF(chartPosition, chartSize);
        DrawInfoPanel(infoPanelBounds, infoPanelNaturalBounds, target);
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
        var context = BuildLibraRenderingContext(Properties.InfoPanelBaseFont);

        var borderColor = Properties.InfoPanelBorderColor.WithOpacity(globalOpacity);
        var backgroundColor = Properties.InfoPanelBackgroundColor.WithOpacity(globalOpacity);
        target.DrawRectangleOfThickness(bounds, Properties.InfoPanelBorderThickness, borderColor);
        target.DrawRectangle(bounds, backgroundColor, SPaintStyle.Fill, SAngle.Zero);

        // But, oh, is there so much to do.
        var root = new LayoutNode();
        var contentBounds = root.Inset(Properties.InfoPanelPaddingRatio, Properties.InfoPanelPaddingRatio, "contentBounds");

        // Okay, maybe not THAT much to do.
        var stacker = new LayoutStacker(Direction.Vertical, Properties.InfoPanelSummaryItemMargin,
            InfoPanelDisplayItemMarginMultiplier);
    }

    private void DrawInfoPanelContent(SRectF contentBounds, IRenderTarget target)
    {
        // Empty for now. We need the actual, you know, summary properties to come
        // from somewhere first.
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
