using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Basis
{
    public sealed class BasisLine : BasisElement
    {
        public SPointF From { get; set; }
        public SPointF To { get; set; }

        public SPointF Center => new((From.X + To.X) / 2, (From.Y + To.Y) / 2);

        public BasisLine(SPointF from, SPointF to)
        {
            From = from;
            To = to;
        }

        public override void RenderDebug(IRenderTarget target)
        {
            target.DrawLine(From, To, SColor.Yellow, 3f);
        }
    }
}
