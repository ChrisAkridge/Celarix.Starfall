using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Layout;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public sealed class HeliumScene
    {
        public HeliumElement? Root { get; set; }
        public SColor BackgroundColor { get; set; }

        public void Render(IRenderTarget target, SSizeF maxSize)
        {
            target.Clear(BackgroundColor);

            if (Root == null) { return; }

            var positionedRoot = Root.Measure(maxSize);
            positionedRoot.Arrange(SPointF.Zero);
        }
    }
}
