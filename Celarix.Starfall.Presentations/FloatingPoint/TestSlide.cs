using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class TestSlide : AtriaSlide
    {
        public TestSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            var singleView = new SingleBinaryViewElement("#singleView", "Consolas")
            {
                Size = new SSizeF(Size.Width, Size.Height),
                Opacity = 1.0f
            };
            Add([singleView]);
        }
    }
}
