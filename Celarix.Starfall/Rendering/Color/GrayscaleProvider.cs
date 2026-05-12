using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Color
{
    public sealed class GrayscaleProvider
    {
        private float _luminance;

        public float Luminance
        {
            get => _luminance;
            set
            {
                if (value < 0f || value > 1f)
                    throw new ArgumentOutOfRangeException(nameof(Luminance), "Luminance must be between 0 and 1.");
                _luminance = value;
            }
        }

        public SColor Sample()
        {
            return SampleLuminance(Luminance);
        }

        private static SColor SampleLuminance(float luminance)
        {
            var grayValue = (byte)(luminance * 255);
            return new SColor(grayValue, grayValue, grayValue, 255);
        }

        public IReadOnlyList<SColor> SampleNColors(int count, float distanceBetweenColors)
        {
            if (count < 1)
            {
                throw new ArgumentException("Must ask for at least one color", nameof(count));
            }
            else if (count == 1)
            {
                return [Sample()];
            }

            var colors = new SColor[count];
            var range = distanceBetweenColors * (count - 1);
            var halfRange = range / 2f;
            var startLuminance = Luminance - halfRange;
            var step = range / (count - 1);

            for (var i = 0; i < count; i++)
            {
                var currentLuminance = startLuminance + i * step;
                colors[i] = SampleLuminance(Math.Clamp(currentLuminance, 0, 1));
            }
            return colors;
        }

        private static float AddLuminance(float baseLuminance, float additionalLuminance)
        {
            return Math.Clamp(baseLuminance + additionalLuminance, 0f, 1f);
        }

        private static GrayscaleProvider FromSColor(SColor color)
        {
            // Using the luminosity method for grayscale conversion
            float luminance = (0.299f * color.R + 0.587f * color.G + 0.114f * color.B) / 255f;
            return new GrayscaleProvider { Luminance = luminance };
        }
    }
}
