using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal sealed class MixingColorList
    {
        private List<SColor> _colors = new();

        public SColor MixedColor
        {
            get
            {
                // Perform a clamped additive color mixing.
                var red = 0;
                var green = 0;
                var blue = 0;
                var alpha = 0;

                foreach (var color in _colors)
                {
                    red += color.R;
                    green += color.G;
                    blue += color.B;
                    alpha += color.A;
                }

                // Clamp the color components to the valid range [0, 255].
                red = Math.Min(red, 255);
                green = Math.Min(green, 255);
                blue = Math.Min(blue, 255);
                alpha = Math.Min(alpha, 255);

                return new SColor((byte)red, (byte)green, (byte)blue, (byte)alpha);
            }
        }

        public void AddColor(SColor color)
        {
            _colors.Add(color);
        }

        public void RemoveColor(SColor color)
        {
            if (!_colors.Remove(color))
            {
                throw new InvalidOperationException($"Color {color} not found in the list.");
            }
        }
    }
}
