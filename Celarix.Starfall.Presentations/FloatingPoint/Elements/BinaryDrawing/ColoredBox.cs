using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal struct ColoredBox
    {
        public SRectF Rectangle { get; set; }
        public SColor Color { get; set; }

        public ColoredBox(SRectF rectangle, SColor color)
        {
            Rectangle = rectangle;
            Color = color;
        }

        public override string ToString() => $"ColoredBox(Rectangle: {Rectangle}, Color: {Color})";
    }
}
