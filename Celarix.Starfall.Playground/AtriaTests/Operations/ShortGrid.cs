using Celarix.Starfall;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests.Operations
{
    internal sealed class ShortGrid : AtriaElement
    {
        private readonly SFont _font = new SFontFamily("Consolas", 12f);
        private SKImage _gridImage;
        private Func<short, short> transform;

        public Func<short, short> Transform
        {
            get
            {
                return transform;
            }

            set
            {
                transform = value;
                InvalidateGrid();
            }
        }

        public ShortGrid(Func<short, short> transform)
        {
            Transform = transform;
        }

        public override void Render(IRenderTarget target)
        {
            var measurementService = Slide?.MeasurementService;
            if (measurementService == null) { return; }

            var maxSquare = Bounds.InsetSquare();
            var gridSizeShrinkFactor = 0.1f;
            var gridSize = maxSquare.ShrinkByFactor(gridSizeShrinkFactor, gridSizeShrinkFactor);

            // Draw the grid bitmap
            if (_gridImage == null) { InvalidateGrid(); }
            target.DrawImage(SImage.FromSKImage(_gridImage!), gridSize);

            //double[] debugTopXs = MathHelpers.SubdivideRange(gridSize.Left, gridSize.Right, 16);
            //var debugEllipseSize = new SSizeF(10, 10);

            //for (var i = 0; i < debugTopXs.Length; i++)
            //{
            //    target.DrawEllipse(new SPointF(debugTopXs[i], gridSize.Top - 10d), debugEllipseSize, SColor.Red, SPaintStyle.Fill);
            //}

            // Draw the X labels
            var textMeasure = measurementService.MeasureText("FF--", _font);
            var textHeight = textMeasure.Height;
            var textYOffset = -(textHeight);
            var textWidth = textMeasure.Width;
            var xLabelCenters = MathHelpers.SubdivideRange(gridSize.Left, gridSize.Right, 15);

            for (int i = 0; i < 16; i++)
            {
                var label = $"--{i:X1}0";
                var labelX = xLabelCenters[i] - (textWidth / 2f);
                var labelY = gridSize.Top + textYOffset;
                var bounds = new SRectF(labelX, labelY, textWidth, textHeight);
                target.DrawText(label, _font, bounds, SColor.White, SAngle.Zero);
            }

            // Draw the Y labels
            var textXOffset = -textWidth;
            var yLabelCenters = MathHelpers.SubdivideRange(gridSize.Top, gridSize.Bottom, 15);
            for (int i = 0; i < 16; i++)
            {
                var label = $"--{i:X1}0";
                var labelX = gridSize.Left + textXOffset;
                var labelY = yLabelCenters[i] - (textHeight / 2f);
                var bounds = new SRectF(labelX, labelY, textWidth, textHeight);
                target.DrawText(label, _font, bounds, SColor.White, SAngle.Zero);
            }
        }

        private void InvalidateGrid()
        {
            var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(256, 256);

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    var value = Transform((short)((y * 256) + x));
                    // Blue-green gradient
                    image[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(0,
                        (byte)((value >> 8) & 0xFF),
                        (byte)(value & 0xFF),
                        255);
                }
            }

            _gridImage?.Dispose();
            _gridImage = image.CreateSKImageFromImageSharp();
        }
    }
}
