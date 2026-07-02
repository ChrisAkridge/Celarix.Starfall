using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal struct ColoredBoxPoint
    {
        public double XCoordinate { get; set; }
        public PointKind Kind { get; set; }
        public SColor Color { get; set; }

        public ColoredBoxPoint(double xCoordinate, PointKind kind, SColor color)
        {
            XCoordinate = xCoordinate;
            Kind = kind;
            Color = color;
        }

        public override string ToString() => $"{Kind} @ {XCoordinate}";
    }
}
