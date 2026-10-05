using Celarix.Starfall.Atria;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Decks.Playground.AtriaTests
{
    public sealed class TimeProgressSlide : AtriaSlide
    {
        public TimeProgressSlide(AtriaRuntime runtime, SSizeF size) : base(runtime, size)
        {
            BackgroundColor = new SColor(8, 0, 130, 255);
            var countElement = new CountPanelElement
            {
                Count = BigInteger.Parse("11"),
                Size = new SSizeF(1000, 600)
            };
            var centerBasis = new BasisPoint(Center, "#center");
            countElement.AnchorCenterTo(centerBasis);
            Add([countElement, centerBasis]);
        }

        public override void Initialize()
        {
            
        }
    }
}
