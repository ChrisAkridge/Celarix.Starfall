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
        private const double LogoAppearDuration = 0.75d;
        private const double LogoHoldDuration = 1.0d;
        private const double LogoFadeOutDuration = 0.25d;
        private const double PanelAppearDelay = LogoHoldDuration;
        private const double PanelAppearDuration = 0.5d;
        private const double PanelHoldDuration = 2.0d;
        private const double PanelSlideUpDuration = 0.25d;

        private SSizeF targetLogoSize = SSizeF.Zero;
        private double logoInitialScale = 1.5d;
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
