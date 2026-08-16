using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SRect
    {
        public static readonly SRect Empty = new SRect(0, 0, 0, 0);

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        public SPoint Position => new(X, Y);
        public SSize Size => new(Width, Height);

        public SRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override string ToString() => $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";

        public void ThrowIfSizeNotPositive(string? parameterName = null)
        {
            if (Width <= 0 || Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Width and Height must be positive. Actual: {this}");
            }
        }
    }
}
