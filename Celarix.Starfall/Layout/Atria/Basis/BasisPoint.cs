using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Basis
{
    public sealed class BasisPoint : BasisElement, IAtriaIdentified, ISlideAddable
    {
        public AtriaId Id { get; private set; }
        public SPointF Point { get; set; }
        public AtriaSlide Slide { get; set; }

        public BasisPoint(SPointF point, string atriaIdString)
        {
            Point = point;
            Id = AtriaId.Parse(atriaIdString);
        }

        public override void RenderDebug(IRenderTarget target)
        {
            target.DrawEllipse(Point, new SSizeF(10, 10), SColor.Magenta, SPaintStyle.Fill);
        }
    }
}
