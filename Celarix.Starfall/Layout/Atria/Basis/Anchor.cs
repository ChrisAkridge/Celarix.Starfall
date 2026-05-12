using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Basis
{
    internal sealed class Anchor
    {
        private readonly BasisPoint _point;
        private readonly Alignment _anchoredPoint;

        public Anchor(BasisPoint point, Alignment anchoredPoint)
        {
            _point = point;
            _anchoredPoint = anchoredPoint;
        }

        public SPointF GetPosition(SSizeF elementSize)
        {
            return _anchoredPoint switch
            {
                Alignment.TopLeft => _point.Point,
                Alignment.TopCenter => _point.Point + new SSizeF(-elementSize.Width / 2, 0),
                Alignment.TopRight => _point.Point + new SSizeF(-elementSize.Width, 0),
                Alignment.LeftCenter => _point.Point + new SSizeF(0, -elementSize.Height / 2),
                Alignment.Center => _point.Point + new SSizeF(-elementSize.Width / 2, -elementSize.Height / 2),
                Alignment.RightCenter => _point.Point + new SSizeF(-elementSize.Width, -elementSize.Height / 2),
                Alignment.BottomLeft => _point.Point + new SSizeF(0, -elementSize.Height),
                Alignment.BottomCenter => _point.Point + new SSizeF(-elementSize.Width / 2, -elementSize.Height),
                Alignment.BottomRight => _point.Point + new SSizeF(-elementSize.Width, -elementSize.Height),
                _ => throw new InvalidOperationException($"Unsupported alignment: {_anchoredPoint}")
            };
        }
    }
}
