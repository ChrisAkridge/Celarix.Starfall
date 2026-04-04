using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.Distributions
{
    internal sealed class DistributionElement : HeliumElement
    {
        private double _rangeMinimum;
        private double _rangeMaximum;
        private readonly Dictionary<double, int> _values = new();

        // Visual properties
        private readonly SColor _barColor;
        private readonly SColor _axisColor;
        private readonly SFont _labelFont;
        private SRectF _distributionBounds;

        // Animation properties
        private readonly double _fadeDuration = 0.25d;    // duration in seconds for the fade-out of the last added value
        private Stack<(double AddedValue, double Opacity)> _lastVisuallyAddedValues = new();

        public DistributionElement(SColor barColor, SColor axisColor, SFont labelFont)
        {
            _barColor = barColor;
            _axisColor = axisColor;
            _labelFont = labelFont;
        }

        public void AddValue(double value)
        {
            if (value < _rangeMinimum) { _rangeMinimum = value; }
            if (value > _rangeMaximum) { _rangeMaximum = value; }
            if (_values.TryGetValue(value, out int value1))
            {
                _values[value] = ++value1;
            }
            else
            {
                _values[value] = 1;
            }
        }

        public override IReadOnlyList<HeliumElement> Children => [];

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

        public override void ArrangeChildren(SRectF thisBounds) { }

        public override HeliumElement Clone()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            const double axisLineWidth = 1d;
            SRectF actualBounds = ActualBounds!.Value;
            var renderables = new List<IRenderable>();

            // First: vertical axis line
            var vAxisStart = new SPointF(_distributionBounds.Left - axisLineWidth, actualBounds.Y);
            var vAxisEnd = new SPointF(_distributionBounds.Left - axisLineWidth, actualBounds.Y + _distributionBounds.Height);
            var vAxis = new LineRenderable(vAxisStart, vAxisEnd, _axisColor, (float)axisLineWidth);
            renderables.Add(vAxis);

            // Second: horizontal axis line
            var hAxisStart = new SPointF(_distributionBounds.Left - axisLineWidth, _distributionBounds.Bottom + axisLineWidth);
            var hAxisEnd = new SPointF(_distributionBounds.Right - axisLineWidth, _distributionBounds.Bottom + axisLineWidth);
            var hAxis = new LineRenderable(hAxisStart, hAxisEnd, _axisColor, (float)axisLineWidth);
            renderables.Add(hAxis);

            // Third: the bars
            var range = _rangeMaximum - _rangeMinimum;
            var widthPerValue = _distributionBounds.Width / range;
            var highestFrequency = _values.Count > 0 ? _values.Values.Max() : 1d;
            var heightPerFrequency = _distributionBounds.Height / highestFrequency;

            var keys = _values.Keys.OrderBy(k => k).ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                var value = keys[i];
                var frequency = _values[value];
                var barHeight = frequency * heightPerFrequency;
                var barX = _distributionBounds.Left + (value - _rangeMinimum) * widthPerValue;
                var barY = _distributionBounds.Bottom - barHeight;
                var barWidth = widthPerValue;
                var bar = new RectangleRenderable()
                {
                    Bounds = new SRectF(barX, barY, barWidth, barHeight),
                    PaintStyle = SPaintStyle.Fill,
                    Color = _barColor
                };
                renderables.Add(bar);
            }

            return renderables;
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            ActualSize = availableSize;
            _distributionBounds = GetDistributionBounds(measurementService);
        }

        // Rendering helpers
        private double GetAxisMargin(MeasurementService measurementService)
        {
            var fontSize = _labelFont.Size ?? 12f;
            const string textWithAscendersAndDescenders = "Hgq";
            var textHeight = measurementService.MeasureText(textWithAscendersAndDescenders, _labelFont.WithSize(fontSize)).Height;
            var axisMargin = textHeight * 2d;
            return axisMargin;
        }

        private SRectF GetDistributionBounds(MeasurementService measurementService)
        {
            // The left and bottom hold the axes. We'll make them twice the height of whatever the
            // axis text height is, then draw the axes right outside the bounding box this method
            // returns.
            double axisMargin = GetAxisMargin(measurementService);

            // Now we can calculate the bounds for the distribution bars, which will be the area inside the axes.
            var distributionX = ActualBounds!.Value.X + axisMargin;
            var distributionY = ActualBounds.Value.Y;
            var distributionWidth = ActualBounds!.Value.Width - axisMargin;
            var distributionHeight = ActualBounds!.Value.Height - axisMargin;
            return new SRectF(distributionX, distributionY, distributionWidth, distributionHeight);
        }

       
    }
}
