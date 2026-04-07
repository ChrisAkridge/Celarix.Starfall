using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.CustomElements
{
    internal sealed class ProgressBarElement : HeliumElement
    {
        private double minValue;
        private double maxValue;
        private double value;
        private SColor barColor;
        private SColor axisColor;
        private SColor textColor;
        private SFont font;

        private int totalTicks;
        private Func<double, string> tickTextFunc;
        private CachedRenderable? frameRenderable;

        public ProgressBarElement(
            double minValue,
            double maxValue,
            double value,
            SColor barColor,
            SColor axisColor,
            SColor textColor,
            SFont font,
            int totalTicks = 0,
            Func<double, string>? tickTextFunc = null)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.value = value;
            this.barColor = barColor;
            this.axisColor = axisColor;
            this.textColor = textColor;
            this.font = font;
            this.totalTicks = totalTicks;
            this.tickTextFunc = tickTextFunc ?? (v => v.ToString("0.##"));
        }

        public override IReadOnlyList<HeliumElement> Children => [];

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

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

            frameRenderable ??= new CachedRenderable(size => GetFrameRenderables(size, SPointF.Zero, measurementService), ActualBounds!.Value);
            renderables.Add(frameRenderable);

            var size = ActualSize!.Value;
            var position = ActualPosition!.Value;
            var heightThird = size.Height / 3d;
            var axisWidth = Math.Clamp(size.Width * 0.01f, 1f, 5f);
            var barX = axisWidth;
            var maxBarRight = size.Width - axisWidth;
            var barHeight = (heightThird * 2) - axisWidth;

            var barRight = (float)(barX + ((maxBarRight - barX) * ((value - minValue) / (maxValue - minValue))));
            renderables.Add(new RectangleRenderable(new SRectF(
                new SPointF(position.X + barX, position.Y),
                new SSizeF(barRight - barX, barHeight)),
                barColor, SPaintStyle.Fill));

            return renderables;
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            ActualSize = availableSize;
        }

        private IReadOnlyList<IRenderable> GetFrameRenderables(SSizeF size, SPointF position, MeasurementService measurementService)
        {
            var renderables = new List<IRenderable>();
            var topRight = new SPointF(position.X + size.Width, position.Y);
            var heightThird = size.Height / 3d;
            var textPaneY = (heightThird * 2);
            var tickHeight = (heightThird * 2) / 4d;
            var axisWidth = Math.Clamp(size.Width * 0.01f, 1f, 5f);
            var axisHalfWidth = axisWidth / 2;
            var distanceBetweenTicks = size.Width / totalTicks;
            var barHeight = (heightThird * 2) - axisWidth;
            var barX = axisWidth;
            var maxBarRight = size.Width - axisWidth;

            var leftAxisStart = new SPointF(position.X + axisHalfWidth, position.Y);
            var leftAxisEnd = new SPointF(position.X + axisHalfWidth, position.Y + textPaneY);
            var rightAxisStart = new SPointF(topRight.X - axisHalfWidth, position.Y);
            var rightAxisEnd = new SPointF(topRight.X - axisHalfWidth, position.Y + textPaneY);
            var lowerAxisStart = new SPointF(position.X, position.Y + textPaneY - axisHalfWidth);
            var lowerAxisEnd = new SPointF(topRight.X, position.Y + textPaneY - axisHalfWidth);
            renderables.Add(new LineRenderable(leftAxisStart, leftAxisEnd, axisColor, (float)axisWidth));
            renderables.Add(new LineRenderable(rightAxisStart, rightAxisEnd, axisColor, (float)axisWidth));
            renderables.Add(new LineRenderable(lowerAxisStart, lowerAxisEnd, axisColor, (float)axisWidth));

            for (var i = 1; i < totalTicks; i++)
            {
                var tickX = position.X + (distanceBetweenTicks * i);
                var tickStart = new SPointF(tickX, position.Y + textPaneY - tickHeight);
                var tickEnd = new SPointF(tickX, position.Y + textPaneY);
                renderables.Add(new LineRenderable(tickStart, tickEnd, axisColor, (float)axisWidth));
                var tickValue = minValue + ((maxValue - minValue) * (i / (double)(totalTicks)));
                var tickText = tickTextFunc(tickValue);
                var textSize = measurementService.MeasureText(tickText, font);
                var textPosition = new SPointF(
                    tickX - (textSize.Width / 2),
                    textPaneY + textSize.Height);
                renderables.Add(new TextRenderable
                {
                    Text = tickText,
                    Font = font,
                    Bounds = new SRectF(textPosition, textSize),
                    Color = textColor,
                    Rotation = SAngle.Zero,
                });
            }

            // The first tick is left-aligned and the last tick is right-aligned, so they need to be handled separately.
            var minTickText = tickTextFunc(minValue);
            var minTickSize = measurementService.MeasureText(minTickText, font);
            var minTextPosition = new SPointF(position.X, textPaneY + minTickSize.Height);
            var maxTickText = tickTextFunc(maxValue);
            var maxTickSize = measurementService.MeasureText(maxTickText, font);
            var maxTextPosition = new SPointF(topRight.X - maxTickSize.Width, textPaneY + maxTickSize.Height);
            renderables.Add(new TextRenderable
            {
                Text = minTickText,
                Font = font,
                Bounds = new SRectF(minTextPosition, minTickSize),
                Color = textColor,
                Rotation = SAngle.Zero,
            });
            renderables.Add(new TextRenderable
            {
                Text = maxTickText,
                Font = font,
                Bounds = new SRectF(maxTextPosition, maxTickSize),
                Color = textColor,
                Rotation = SAngle.Zero,
            });
            return renderables;
        }
    }
}
