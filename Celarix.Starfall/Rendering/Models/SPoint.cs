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
    }
}
