using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.CustomElements
{
    internal sealed class DistributionElement : HeliumElement
    {
        public override IReadOnlyList<HeliumElement> Children => throw new NotImplementedException();

        public override double DesiredWidthFraction => throw new NotImplementedException();

        public override double DesiredHeightFraction => throw new NotImplementedException();

        public override void ArrangeChildren(SRectF thisBounds)
        {
            throw new NotImplementedException();
        }

        public override HeliumElement Clone()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IRenderable> GetRenderables(MeasurementService measurementService)
        {
            throw new NotImplementedException();
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            throw new NotImplementedException();
        }
    }
}
