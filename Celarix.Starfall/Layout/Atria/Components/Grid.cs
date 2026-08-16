using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Components
{
    public sealed class Grid
    {
        private SColor[,] _cellColors;

        public double GridLineWidth { get; }
        public SColor GridLineColor { get; set; }
        public SSize GridSizeInCells { get; private set; }
        public SSizeF CellSize { get; private set; }

        public SSizeF Bounds => new(GridSizeInCells.Width * CellSize.Width, GridSizeInCells.Height * CellSize.Height);
        public SSizeF InnerCellSize => new((Bounds.Width - GridLineWidth * (GridSizeInCells.Width + 1)) / GridSizeInCells.Width,
            (Bounds.Height - GridLineWidth * (GridSizeInCells.Height + 1)) / GridSizeInCells.Height);

        public Grid(SSize initialGridSizeInCells,
            SSizeF initialCellSize,
            double gridLineWidth,
            SColor gridLineColor)
        {
            GridSizeInCells = initialGridSizeInCells;
            CellSize = initialCellSize;
            GridLineWidth = gridLineWidth;
            GridLineColor = gridLineColor;
            _cellColors = new SColor[GridSizeInCells.Width, GridSizeInCells.Height];
        }

        public void Render(IRenderTarget target, MeasurementService measurementService,
            SPointF position)
        {
            var positionedBounds = Bounds.At(position);
            var gridVLines = GridSizeInCells.Width + 1;
            var gridHLines = GridSizeInCells.Height + 1;
            var innerCellWidth = (Bounds.Width - GridLineWidth * gridVLines) / GridSizeInCells.Width;
            var innerCellHeight = (Bounds.Height - GridLineWidth * gridHLines) / GridSizeInCells.Height;

            // Draw the cell colors
            for (var y = 0; y < GridSizeInCells.Height; y++)
            {
                for (var x = 0; x < GridSizeInCells.Width; x++)
                {
                    var cellColor = _cellColors[x, y];
                    if (cellColor.A > 0) // Only draw if the cell color is not fully transparent
                    {
                        var cellX = positionedBounds.Left + (x * CellSize.Width) + GridLineWidth;
                        var cellY = positionedBounds.Top + (y * CellSize.Height) + GridLineWidth;
                        var cellRect = new SRectF(cellX, cellY, innerCellWidth, innerCellHeight);
                        target.DrawRectangle(cellRect, cellColor, SPaintStyle.Fill, SAngle.Zero);
                    }
                }
            }

            // Draw the grid lines
            for (var y = 0; y < gridHLines; y++)
            {
                var lineY = positionedBounds.Top + (y * CellSize.Height);
                target.DrawLine(new SPointF(positionedBounds.Left, lineY), new SPointF(positionedBounds.Right, lineY), GridLineColor,
                    (float)GridLineWidth);
            }

            for (var x = 0; x < gridVLines; x++)
            {
                var lineX = positionedBounds.Left + (x * CellSize.Width);
                target.DrawLine(new SPointF(lineX, positionedBounds.Top), new SPointF(lineX, positionedBounds.Bottom), GridLineColor,
                    (float)GridLineWidth);
            }
        }

        public void ResizeGrid(SSize newSize)
        {
            newSize.ThrowIfNotPositive(nameof(newSize));

            // Only expand the underlying color array, never shrink it, so we don't lose data.
            if (newSize.Width > _cellColors.GetLength(0) || newSize.Height > _cellColors.GetLength(1))
            {
                var newCellColors = new SColor[newSize.Width, newSize.Height];
                for (var y = 0; y < Math.Min(newSize.Height, _cellColors.GetLength(1)); y++)
                {
                    for (var x = 0; x < Math.Min(newSize.Width, _cellColors.GetLength(0)); x++)
                    {
                        newCellColors[x, y] = _cellColors[x, y];
                    }
                }
                _cellColors = newCellColors;
            }

            GridSizeInCells = newSize;
        }

        public void SetOuterCellSize(SSizeF newSize)
        {
            newSize.ThrowIfNotPositive(nameof(newSize));
            CellSize = newSize;
        }

        public void FitGridToBounds(SSizeF bounds)
        {
            bounds.ThrowIfNotPositive(nameof(bounds));
            var newCellWidth = bounds.Width / GridSizeInCells.Width;
            var newCellHeight = bounds.Height / GridSizeInCells.Height;
            SetOuterCellSize(new SSizeF(newCellWidth, newCellHeight));
        }

        public SRectF GetInnerCellBounds(SPointF position, SPoint cell)
        {
            if (cell.X < 0 || cell.X >= GridSizeInCells.Width || cell.Y < 0 || cell.Y >= GridSizeInCells.Height)
            {
                throw new ArgumentOutOfRangeException($"Cell coordinates ({cell.X}, {cell.Y}) are out of bounds for grid size {GridSizeInCells}.");
            }

            var positionedBounds = Bounds.At(position);
            var cellXPos = positionedBounds.Left + (cell.X * CellSize.Width) + GridLineWidth;
            var cellYPos = positionedBounds.Top + (cell.Y * CellSize.Height) + GridLineWidth;
            var innerCellWidth = (positionedBounds.Width - GridLineWidth * (GridSizeInCells.Width + 1)) / GridSizeInCells.Width;
            var innerCellHeight = (positionedBounds.Height - GridLineWidth * (GridSizeInCells.Height + 1)) / GridSizeInCells.Height;
            return new SRectF(cellXPos, cellYPos, innerCellWidth, innerCellHeight);
        }

        public void ColorRange(SRect cellRange, SColor color)
        {
            cellRange.ThrowIfSizeNotPositive(nameof(cellRange));

            for (var y = cellRange.Top; y < cellRange.Bottom; y++)
            {
                for (var x = cellRange.Left; x < cellRange.Right; x++)
                {
                    if (x >= 0 && x < GridSizeInCells.Width && y >= 0 && y < GridSizeInCells.Height)
                    {
                        _cellColors[x, y] = color;
                    }
                }
            }
        }

        public void ColorCell(SPoint cell, SColor color)
        {
            cell.ThrowIfOutOfBounds(new SRect(0, 0, GridSizeInCells.Width, GridSizeInCells.Height));
            _cellColors[cell.X, cell.Y] = color;
        }
    }
}
