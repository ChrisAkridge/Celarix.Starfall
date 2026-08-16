using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SSize
    {
        public int Width { get; }
        public int Height { get; }
        public SSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override readonly string ToString() => $"(Width: {Width}, Height: {Height})";

        public void ThrowIfNotPositive(string? parameterName = null)
        {
            if (Width <= 0 || Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Width and Height must be positive. Actual: {this}");
            }
        }
    }
}
