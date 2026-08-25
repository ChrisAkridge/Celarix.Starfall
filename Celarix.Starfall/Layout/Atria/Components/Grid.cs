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
        public IGridCellProvider? CellProvider { get; set; }
        public SFont DefaultFont { get; set; } = new SFontFamily("Calibri", 12f);
        public SColor DefaultTextColor { get; set; } = SColor.Black;
        public GridTextScalingMode TextScalingMode { get; set; } = GridTextScalingMode.SizeEqually;
        public double ContentInsetFactor { get; set; } = 0.15d;

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
            Render(target, measurementService, position, CellProvider);
        }

        public void Render(IRenderTarget target,
            MeasurementService measurementService,
            SPointF position,
            IGridCellProvider? cellProvider)
        {
            var positionedBounds = Bounds.At(position);
            var gridVLines = GridSizeInCells.Width + 1;
            var gridHLines = GridSizeInCells.Height + 1;

            // Draw the cell colors
            for (var y = 0; y < GridSizeInCells.Height; y++)
            {
                for (var x = 0; x < GridSizeInCells.Width; x++)
                {
                    var cell = new SPoint(x, y);
                    var cellColor = cellProvider?.GetCellColor(cell) ?? _cellColors[x, y];
                    if (cellColor.A > 0) // Only draw if the cell color is not fully transparent
                    {
                        target.DrawRectangle(GetInnerCellBounds(position, cell), cellColor, SPaintStyle.Fill, SAngle.Zero);
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

            if (cellProvider is not null)
            {
                RenderCellText(target, measurementService, position, cellProvider);
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

        public SRectF GetOuterCellBounds(SPointF position, SPoint cell)
        {
            ThrowIfCellOutOfBounds(cell);

            var positionedBounds = Bounds.At(position);
            var cellXPos = positionedBounds.Left + (cell.X * CellSize.Width);
            var cellYPos = positionedBounds.Top + (cell.Y * CellSize.Height);
            return new SRectF(cellXPos, cellYPos, CellSize.Width, CellSize.Height);
        }

        public SRectF GetInnerCellBounds(SPointF position, SPoint cell)
        {
            ThrowIfCellOutOfBounds(cell);

            var positionedBounds = Bounds.At(position);
            var cellXPos = positionedBounds.Left + (cell.X * CellSize.Width) + GridLineWidth;
            var cellYPos = positionedBounds.Top + (cell.Y * CellSize.Height) + GridLineWidth;
            var innerCellWidth = (positionedBounds.Width - GridLineWidth * (GridSizeInCells.Width + 1)) / GridSizeInCells.Width;
            var innerCellHeight = (positionedBounds.Height - GridLineWidth * (GridSizeInCells.Height + 1)) / GridSizeInCells.Height;
            return new SRectF(cellXPos, cellYPos, innerCellWidth, innerCellHeight);
        }

        public SRectF GetContentCellBounds(SPointF position, SPoint cell)
        {
            return GetContentCellBounds(position, cell, ContentInsetFactor);
        }

        public SRectF GetContentCellBounds(SPointF position, SPoint cell,
            double insetFactor)
        {
            if (insetFactor < 0d || insetFactor >= 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(insetFactor), "Inset factor must be at least 0 and less than 1.");
            }

            return GetInnerCellBounds(position, cell).ShrinkByFactor(insetFactor, insetFactor);
        }

        public bool TryGetCellAt(SPointF gridPosition,
            SPointF point,
            out SPoint cell)
        {
            var gridBounds = Bounds.At(gridPosition);
            if (point.X < gridBounds.Left
                || point.X >= gridBounds.Right
                || point.Y < gridBounds.Top
                || point.Y >= gridBounds.Bottom)
            {
                cell = default;
                return false;
            }

            var x = (int)Math.Floor((point.X - gridPosition.X) / CellSize.Width);
            var y = (int)Math.Floor((point.Y - gridPosition.Y) / CellSize.Height);
            cell = new SPoint(x, y);
            return x >= 0 && x < GridSizeInCells.Width && y >= 0 && y < GridSizeInCells.Height;
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
            ThrowIfCellOutOfBounds(cell);
            _cellColors[cell.X, cell.Y] = color;
        }

        private void RenderCellText(IRenderTarget target,
            MeasurementService measurementService,
            SPointF position,
            IGridCellProvider cellProvider)
        {
            var equalFontSize = TextScalingMode == GridTextScalingMode.SizeEqually
                ? CalculateEqualFontSize(target, position, cellProvider)
                : null;

            for (var y = 0; y < GridSizeInCells.Height; y++)
            {
                for (var x = 0; x < GridSizeInCells.Width; x++)
                {
                    var cell = new SPoint(x, y);
                    var text = cellProvider.GetText(cell);
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var contentBounds = GetContentCellBounds(position, cell);
                    var font = cellProvider.GetFont(cell) ?? DefaultFont;
                    font = TextScalingMode switch
                    {
                        GridTextScalingMode.SizeEqually when equalFontSize is not null => font.WithSize(equalFontSize.Value),
                        GridTextScalingMode.ShrinkCellsToFit => font.WithSize(target.FitTextToWidth(text, font, (float)contentBounds.Width)),
                        _ => font
                    };
                    var textColor = cellProvider.GetTextColor(cell) ?? DefaultTextColor;
                    target.DrawText(text, font, contentBounds, textColor, SAngle.Zero);
                }
            }
        }

        private float? CalculateEqualFontSize(IRenderTarget target,
            SPointF position,
            IGridCellProvider cellProvider)
        {
            float? requiredFontSize = null;

            for (var y = 0; y < GridSizeInCells.Height; y++)
            {
                for (var x = 0; x < GridSizeInCells.Width; x++)
                {
                    var cell = new SPoint(x, y);
                    var text = cellProvider.GetText(cell);
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var font = cellProvider.GetFont(cell) ?? DefaultFont;
                    var contentBounds = GetContentCellBounds(position, cell);
                    var fontSize = target.FitTextToWidth(text, font, (float)contentBounds.Width);
                    requiredFontSize = requiredFontSize is null
                        ? fontSize
                        : Math.Min(requiredFontSize.Value, fontSize);
                }
            }

            return requiredFontSize;
        }

        private void ThrowIfCellOutOfBounds(SPoint cell)
        {
            if (cell.X < 0 || cell.X >= GridSizeInCells.Width || cell.Y < 0 || cell.Y >= GridSizeInCells.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(cell),
                    $"Cell coordinates ({cell.X}, {cell.Y}) are out of bounds for grid size {GridSizeInCells}.");
            }
        }
    }
}
