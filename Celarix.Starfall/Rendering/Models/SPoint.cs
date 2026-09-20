using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SPoint(int x, int y)
    {
        public readonly int X { get; } = x;
        public readonly int Y { get; } = y;

        public override readonly string ToString() => $"({X}, {Y})";

        public void ThrowIfOutOfBounds(SRect bounds)
        {
            if (X < bounds.Left || X > bounds.Right || Y < bounds.Top || Y > bounds.Bottom)
            {
                throw new ArgumentOutOfRangeException($"Point {this} is out of bounds {bounds}");
            }
        }
    }
}
