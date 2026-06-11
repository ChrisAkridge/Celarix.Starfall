using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering
{
    public sealed class MeasurementService
    {
        private readonly IRenderTarget renderTarget;

        public MeasurementService(IRenderTarget renderTarget)
        {
            this.renderTarget = renderTarget;
        }

        public SSizeF MeasureText(string text, SFont font) => renderTarget.MeasureText(text, font);

        public float FontSizeForDesiredWidth(string text, SFont font, float width)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = " "; // Avoid zero-size text measurement, which can cause divide-by-zero errors in aspect ratio calculations
            }

            return renderTarget.FitTextToWidth(text, font, width);
        }

        public float FontSizeForDesiredHeight(string text, SFont font, float height)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = " "; // Avoid zero-size text measurement, which can cause divide-by-zero errors in aspect ratio calculations
            }

            return renderTarget.FitTextToHeight(text, font, height);
        }

        public float FontSizeForDesiredSize(string text, SFont font, SSizeF availableSize)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = " "; // Avoid zero-size text measurement, which can cause divide-by-zero errors in aspect ratio calculations
            }

            return availableSize.Height >= availableSize.Width
                ? FontSizeForDesiredWidth(text, font, (float)availableSize.Width)
                : FontSizeForDesiredHeight(text, font, (float)availableSize.Height);
        }

        public double AspectRatioForText(string text, SFont font)
        {
            var size = MeasureText(text, font);
            return size.Width / size.Height;
        }
    }
}
