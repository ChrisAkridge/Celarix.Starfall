using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests.Operations
{
    internal sealed class ByteGrid : AtriaElement
    {
        private SFont _font = new SFontFamily("Consolas", 12f);
        private SColor[,] _gridCellColors = new SColor[16, 16];
        private SColor[,] _gridCellTextColors = new SColor[16, 16];
        private string[,] _gridCellText = new string[16, 16];

        public Func<byte, byte> Transform { get; set; }

        public ByteGrid(Func<byte, byte> transform)
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

            // Draw the grid
            var gridWidthCells = 16;
            var gridHeightCells = 16;
            var gridVLines = gridWidthCells + 1;
            var gridHLines = gridHeightCells + 1;
            var gridLineWidth = 2f;
            var outerCellWidth = gridSize.Width / gridWidthCells;
            var outerCellHeight = gridSize.Height / gridHeightCells;
            var innerCellWidth = (gridSize.Width - gridLineWidth * gridVLines) / gridWidthCells;
            var innerCellHeight = (gridSize.Height - gridLineWidth * gridHLines) / gridHeightCells;

            for (var y = 0; y < gridHLines; y++)
            {
                var lineY = gridSize.Top + (y * outerCellHeight);
                target.DrawLine(new SPointF(gridSize.Left, lineY), new SPointF(gridSize.Right, lineY), SColor.White, gridLineWidth);
            }

            for (var x = 0; x < gridVLines; x++)
            {
                var lineX = gridSize.Left + (x * outerCellWidth);
                target.DrawLine(new SPointF(lineX, gridSize.Top), new SPointF(lineX, gridSize.Bottom), SColor.White, gridLineWidth);
            }

            // Build the cell data
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var value = (byte)((y * 16) + x);
                    var transformedValue = Transform(value);
                    _gridCellColors[x, y] = SColor.FromArgb(255, transformedValue, transformedValue, transformedValue);
                    _gridCellTextColors[x, y] = SColor.TextColorForBackgroundColor(_gridCellColors[x, y]);
                    _gridCellText[x, y] = transformedValue.ToString("X2");
                }
            }

            // Draw the cells
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var cellX = gridSize.Left + (x * outerCellWidth) + gridLineWidth;
                    var cellY = gridSize.Top + (y * outerCellHeight) + gridLineWidth;
                    var cellWidth = innerCellWidth;
                    var cellHeight = innerCellHeight;

                    var cellBounds = new SRectF(cellX, cellY, cellWidth, cellHeight);
                    target.DrawRectangle(cellBounds, _gridCellColors[x, y], SPaintStyle.Fill, SAngle.Zero);
                    target.DrawText(_gridCellText[x, y], _font, cellBounds, _gridCellTextColors[x, y], SAngle.Zero);
                }
            }

            // Draw the X labels
            for (var x = 0; x < 16; x++)
            {
                var cellX = gridSize.Left + (x * outerCellWidth) + gridLineWidth;
                var labelBounds = new SRectF(cellX, Bounds.Top, innerCellWidth, innerCellHeight);
                target.DrawText($"-{x:X}", _font, labelBounds, SColor.White, SAngle.Zero);
            }

            // Draw the Y labels
            for (var y = 0; y < 16; y++)
            {
                var cellY = gridSize.Top + (y * outerCellHeight) + gridLineWidth;
                var labelBounds = new SRectF(Bounds.Left, cellY, innerCellWidth, innerCellHeight);
                target.DrawText($"{y:X}-", _font, labelBounds, SColor.White, SAngle.Zero);
            }
        }
    }
}
