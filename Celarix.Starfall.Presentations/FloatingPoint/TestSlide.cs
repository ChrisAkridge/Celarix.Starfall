using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing;
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
            var binaryDrawingElement = new BinaryDrawingExampleElement
            {
                Position = SPointF.Zero,
                Size = new SSizeF(Size.Width, Size.Height),
            };
            binaryDrawingElement.SetFontSize(40f, MeasurementService);
            binaryDrawingElement.SetDataFromFile(@"E:\Documents\Files\Programming\Cix\Logs\log.txt");
            Add([binaryDrawingElement]);
        }
    }
}
