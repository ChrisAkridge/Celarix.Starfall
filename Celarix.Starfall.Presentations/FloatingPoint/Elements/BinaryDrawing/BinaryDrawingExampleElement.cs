using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal sealed class BinaryDrawingExampleElement : AtriaElement
    {
        private const int BytesPerRow = 16;
        //private const double TripletMargin = 0.15d;
        //private const double InTripletMargin = 0.1d;
        //private const double InSquareMargin = 0.05d;
        private const double ByteMargin = 0.2d;

        private static readonly SColor AddressColor = new(0xBF, 0xBF, 0xBF, 0xFF);

        private SSizeF _byteSize;   // Basis size for all other layout calculations
        private SSizeF _addressSize;
        private float _fontSize;
        private SFont _font;
        private BinaryDrawingStage _stage;
        private byte[] _data;
        private Rgba32[] _pixels;

        private SSizeF ByteSquare
        {
            get
            {
                var maxAxis = Math.Max(_byteSize.Width, _byteSize.Height);
                return new SSizeF(maxAxis, maxAxis);
            }
        }

        public float FontSize
        {
            get => _fontSize;
        }

        //private SSizeF RowSize
        //{
        //    get
        //    {
        //        var tripletMarginActual = _byteSize.Width * TripletMargin;
        //        var inTripletMarginActual = _byteSize.Width * InTripletMargin;
        //        var inSquareMarginActual = _byteSize.Width * InSquareMargin;

        //        var width = tripletMarginActual
        //            + 
        //    }
        //}

        public override void Render(IRenderTarget target)
        {
            var dataIndex = 0;
            var y = 0d;
            var byteMarginActual = _byteSize.Width * ByteMargin;
            while (y < Size.Height)
            {
                var addressText = dataIndex.ToString("X8");
                target.DrawTextDirectly(addressText, _font, new SPointF(0, y).WithSize(_addressSize), AddressColor, SAngle.Zero);
                var xBasis = _addressSize.Width + byteMarginActual;

                for (int i = 0; i < BytesPerRow; i++)
                {
                    var data = _data[dataIndex];
                    dataIndex = (dataIndex + 1) % _data.Length;
                    var x = xBasis + (i * (_byteSize.Width + byteMarginActual));

                    // Draw the background square
                    target.DrawRectangle(new SPointF(x, y).WithSize(ByteSquare), SColor.FromArgb(0xFF, 0x1E, 0x1E, 0x1E), SPaintStyle.Fill, SAngle.Zero);

                    // Draw the byte text
                    target.DrawTextDirectly(data.ToString("X2"), _font, new SPointF(x, y).WithSize(_byteSize), SColor.White, SAngle.Zero);
                }
                y += _byteSize.Height + byteMarginActual;
            }
        }

        public void SetFontSize(float newSize, MeasurementService measurementService)
        {
            _fontSize = newSize;
            _font = new SFontFamily("Consolas", _fontSize);
            _byteSize = measurementService.MeasureText("FF", _font);
            _addressSize = measurementService.MeasureText("00000000", _font);
        }

        public void SetDataFromFile(string filePath)
        {
            _data = File.ReadAllBytes(filePath);
            _pixels = new Rgba32[(int)Math.Ceiling(_data.Length / 3d)];
        }
    }
}
