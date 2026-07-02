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
        private int _state;
        private BinaryDrawingExampleElement _element;

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
            //binaryDrawingElement.SetDataFromFile(@"E:\Documents\Files\Programming\Cix\Logs\log.txt");
            binaryDrawingElement.SetDataFromFile(@"C:\Windows\System32\kernel32.dll");
            Add([binaryDrawingElement]);
            _element = binaryDrawingElement;
        }

        public override SlideAdvanceResult Advance()
        {
            if (_state == 0)
            {
                _element.ShowBytes();
                _state = 1;
            }
            else if (_state == 1)
            {
                _element.ShowBoxes();
                _state = 2;
            }
            else if (_state == 2)
            {
                _element.ColorBoxes();
                _state = 3;
            }
            else if (_state == 3)
            {
                _element.MergeBoxes();
                _state = 4;
            }
            else if (_state == 4)
            {
                _element.BuildPixelRow();
                _state = 5;
            }
            else if (_state == 5)
            {
                _element.FillImage();
                _state = 6;
            }
            return SlideAdvanceResult.InternalStateChanged;
        }
    }
}
