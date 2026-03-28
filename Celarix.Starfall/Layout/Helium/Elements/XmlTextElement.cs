using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public sealed class XmlTextElement : HeliumElement
    {
        public string Xml { get; set; } = string.Empty;
        public SFont BaseFont { get; set; } = new SFontFamily("Arial");
        public SColor BaseColor { get; set; } = SColor.Black;
        public SAngle Rotation { get; set; } = SAngle.Zero;

        public override IReadOnlyList<HeliumElement> Children => Array.Empty<HeliumElement>();

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;


        public override void ArrangeChildren(SRectF thisBounds)
        {
            // No children, so nothing to arrange.
        }

        public override HeliumElement Clone()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            throw new NotImplementedException();
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            throw new NotImplementedException();
        }
    }
}
