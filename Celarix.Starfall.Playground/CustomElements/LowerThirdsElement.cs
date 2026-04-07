using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.CustomElements
{
    internal sealed class LowerThirdsElement : HeliumElement
    {
        // At some point, having some kind of animation timeline would be nice, so that we don't have
        // to hardcode so much animation logic.
        private const string CelarixLogoPath = @"E:\Documents\Files\Pictures\Miscellaneous\Avatar\Large Square.png";
        private const int LogoEdgeLength = 581;
        private const double LogoAppearDuration = 0.4d;
        private const double LogoHoldDuration = 1.0d;
        private const double LogoFadeOutDuration = 0.25d;
        private const double PanelAppearDelay = LogoHoldDuration;
        private const double PanelAppearDuration = 0.5d;
        private const double PanelHoldDuration = 2.0d;
        private const double PanelSlideUpDuration = 0.25d;

        private SSizeF targetLogoSize = SSizeF.Zero;
        private double logoInitialScale = 2.5d;
        private double logoFinalScale = 1.0d;
        private double panelHeightFraction = 0.9d;
        private double panelWidthFraction = 0.9d;
        private SPointF ActualCenter => ActualPosition.HasValue && ActualSize.HasValue
            ? new SPointF(ActualPosition.Value.X + ActualSize.Value.Width / 2, ActualPosition.Value.Y + ActualSize.Value.Height / 2)
            : SPointF.Zero;

        // Live animation state.
        private double elapsedTime = 0d;
        private SRectF logoBounds = new(SPointF.Zero, SSizeF.Zero);
        private double logoOpacity = 0d;
        private SRectF panelBounds = new(SPointF.Zero, SSizeF.Zero);

        public override IReadOnlyList<HeliumElement> Children => [];

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => 1.0 / 3.0;

        public override void Update(double deltaTime)
        {
            elapsedTime += deltaTime;
            var logoCenter = ActualCenter;
            if (elapsedTime < LogoAppearDuration)
            {
                var progress = elapsedTime / LogoAppearDuration;
                var smoothstep = Easings.Smoothstep(progress);
                logoOpacity = smoothstep;
                var scale = logoInitialScale + (logoFinalScale - logoInitialScale) * smoothstep;
                var logoSize = new SSizeF(targetLogoSize.Width * scale, targetLogoSize.Height * scale);
                logoBounds = new SRectF(
                    new SPointF(logoCenter.X - logoSize.Width / 2, logoCenter.Y - logoSize.Height / 2),
                    logoSize
                );
            }
            else if (elapsedTime > LogoAppearDuration && elapsedTime < LogoAppearDuration + LogoHoldDuration)
            {
                logoOpacity = 1d;
                var logoSize = targetLogoSize;
                logoBounds = new SRectF(
                    new SPointF(logoCenter.X - logoSize.Width / 2, logoCenter.Y - logoSize.Height / 2),
                    logoSize
                );
            }
            else if (elapsedTime > LogoAppearDuration + LogoHoldDuration && elapsedTime < LogoAppearDuration + LogoHoldDuration + LogoFadeOutDuration)
            {
                var progress = (elapsedTime - LogoAppearDuration - LogoHoldDuration) / LogoFadeOutDuration;
                var smoothstep = Easings.Smoothstep(1 - progress);
                logoOpacity = smoothstep;
                var logoSize = targetLogoSize;
                logoBounds = new SRectF(
                    new SPointF(logoCenter.X - logoSize.Width / 2, logoCenter.Y - logoSize.Height / 2),
                    logoSize
                );
            }

            // ugly hack
            // we really need to figure out this lifecycle thing
            // and animation keyframing
            if (ActualSize == null || ActualPosition == null) { return; }

            SSizeF actualSize = ActualSize!.Value;
            SPointF actualPosition = ActualPosition!.Value;
            if (elapsedTime > PanelAppearDelay && elapsedTime < PanelAppearDelay + PanelAppearDuration)
            {
                var progress = (elapsedTime - PanelAppearDelay) / PanelAppearDuration;
                var smoothstep = Easings.Smoothstep(progress);
                var panelWidth = actualSize.Width * panelWidthFraction * smoothstep;
                var panelHeight = actualSize.Height * panelHeightFraction;
                panelBounds = new SRectF(
                    new SPointF(ActualCenter.X - panelWidth / 2, actualPosition.Y + actualSize.Height - panelHeight),
                    new SSizeF(panelWidth, panelHeight)
                );
            }
            else if (elapsedTime > PanelAppearDelay + PanelAppearDuration && elapsedTime < PanelAppearDelay + PanelAppearDuration + PanelHoldDuration)
            {
                var panelWidth = ActualSize.Value.Width * panelWidthFraction;
                var panelHeight = ActualSize.Value.Height * panelHeightFraction;
                panelBounds = new SRectF(
                    new SPointF(ActualCenter.X - panelWidth / 2, ActualPosition.Value.Y + ActualSize.Value.Height - panelHeight),
                    new SSizeF(panelWidth, panelHeight)
                );
            }
            else if (elapsedTime > PanelAppearDelay + PanelAppearDuration + PanelHoldDuration)
            {
                var panelWidth = ActualSize.Value.Width * panelWidthFraction;
                var panelHeight = ActualSize.Value.Height * panelHeightFraction;
                panelBounds = new SRectF(
                    new SPointF(ActualCenter.X - panelWidth / 2, ActualPosition.Value.Y + ActualSize.Value.Height - panelHeight),
                    new SSizeF(panelWidth, panelHeight)
                );
            }
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            
        }

        public override HeliumElement Clone()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IRenderable> GetRenderables(MeasurementService measurementService)
        {
            var renderables = new List<IRenderable>();
            if (logoOpacity != 0)
            {
                renderables.Add(new ImageFileRenderable(CelarixLogoPath, logoBounds, logoOpacity));
            }

            if (panelBounds.Size != SSizeF.Zero)
            {
                SColor frameColor = new(200, 200, 200, 255);
                renderables.Add(RectangleRenderable.CreateFrame(panelBounds.RoundStandard(), frameColor, 2d));  // Outer frame
                renderables.Add(RectangleRenderable.CreateFrame(panelBounds.Shrink(2d, 2d).RoundStandard(), SColor.White, 10d));    // Inner frame
                renderables.Add(RectangleRenderable.CreateFrame(panelBounds.Shrink(12d, 12d).RoundStandard(), frameColor, 2d));  // Inner frame 3D effect
            }
            return renderables;
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            var width = availableSize.Width * (double)DesiredWidthFraction;
            var height = availableSize.Height * (double)DesiredHeightFraction;
            ActualSize = new SSizeF(width, height);
            targetLogoSize = new SSizeF(height, height);
        }
    }
}
