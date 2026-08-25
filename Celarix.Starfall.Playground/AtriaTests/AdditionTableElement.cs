using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Components;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    internal sealed class AdditionTableElement : AtriaElement
    {
        private sealed class AdditionGridCellProvider : IGridCellProvider
        {

        }

        private const string CarryingOneLabel = "carrying a 1";
        private const double CarryingOneLabelMarginYFactor = 0.2d;

        private static readonly SColor _headerColor = SColor.Blue;
        private static readonly SColor _cellColor = SColor.FromArgb(0xFF, 0x80, 0x80, 0xFF);
        private static readonly SColor _highlightedHeaderColor = SColor.FromArgb(0xFF, 0xCC, 0xCC, 0x33);
        private static readonly SColor _highlightedRowAndColumnColor = SColor.FromArgb(0xFF, 0xE6, 0xE6, 0x99);
        private static readonly SColor _highlightedCellColor = SColor.Yellow;

        private static readonly SFont _headerFont = new SFontFamily("Cambria Math", 20f, FontWeight.Bold, FontWidth.Normal, FontSlant.Upright);
        private static readonly SFont _cellFont = new SFontFamily("Cambria Math", 20f);

        private readonly Grid _table;
        private SPoint _selectedCell;

        public bool CarryingOne { get; set; }
        public double LabelInsetMultiplier { get; set; } = 0.8d;

        public SPoint SelectedCell
        {
            get => _selectedCell;
            set
            {
                value.ThrowIfOutOfBounds(new(0, 0, 11, 11));
                var oldValue = _selectedCell;
                SetRowColor(oldValue.Y, false);
                SetColumnColor(oldValue.X, false);
                SetRowColor(value.Y, true);
                SetColumnColor(value.X, true);
                _table.ColorCell(value, _highlightedCellColor);
                _selectedCell = value;
            }
        }

        public AdditionTableElement(string atriaIdString)
        {
            Id = AtriaId.Parse(atriaIdString);

            _table = new Grid(new(11, 11), new(1, 1), 2d, SColor.White);
            _table.ColorRange(new(0, 0, 11, 1), _headerColor);
            _table.ColorRange(new(0, 0, 1, 11), _headerColor);
            _table.ColorRange(new(1, 1, 10, 10), _cellColor);
        }

        public override void Render(IRenderTarget target)
        {
            if (Slide == null || Slide.MeasurementService == null)
            {
                return;
            }

            _table.FitGridToBounds(Bounds.Size);
            _table.Render(target, Slide.MeasurementService, Position);

            const string WidestText = "18";
            SSizeF innerCellSize = _table.InnerCellSize.ShrinkTowardCenterByFactor(1 - LabelInsetMultiplier, 1 - LabelInsetMultiplier);
            var headerFontSize = target.FitTextToWidth(WidestText, _headerFont, (float)innerCellSize.Width);
            var cellFontSize = target.FitTextToWidth("18", _cellFont, (float)innerCellSize.Width);
            var requiredFontSize = Math.Min(headerFontSize, cellFontSize);

            for (var y = 0; y < 11; y++)
            {
                for (var x = 0; x < 11; x++)
                {
                    string cellValue;

                    var numericX = CarryingOne ? x : x - 1;
                    var numericY = y - 1;

                    if (x == 0 && y == 0)
                    {
                        cellValue = "+";
                    }
                    else if (x == 0)
                    {
                        cellValue = numericY.ToString();
                    }
                    else if (y == 0)
                    {
                        cellValue = numericX.ToString();
                    }
                    else
                    {
                        cellValue = (numericX + numericY).ToString();
                    }

                    var cellBounds = _table.GetInnerCellBounds(Position, new(x, y)).ShrinkByFactor(1 - LabelInsetMultiplier,
                        1 - LabelInsetMultiplier);
                    var font = (x == 0 || y == 0)
                        ? _headerFont.WithSize(requiredFontSize)
                        : _cellFont.WithSize(requiredFontSize);
                    var textColor = (x == 0 || y == 0) ? SColor.White : SColor.Black;
                    target.DrawText(cellValue, font, cellBounds, textColor, SAngle.Zero);
                }
            }

            if (CarryingOne)
            {
                var carryLabelSize = target.MeasureText(CarryingOneLabel, _cellFont);
                var marginY = carryLabelSize.Height * CarryingOneLabelMarginYFactor;
                var labelY = Bounds.Top - carryLabelSize.Height - marginY;
                var labelX = Bounds.Right - carryLabelSize.Width;
                var labelPosition = new SPointF(Position.X, labelY);
                var labelBounds = new SRectF(labelPosition, new(Bounds.Width, carryLabelSize.Height));
                target.DrawText(CarryingOneLabel, _cellFont, labelBounds, SColor.White, SAngle.Zero,
                    Alignment.RightCenter);
            }
        }

        private void SetRowColor(int y, bool highlighted)
        {
            var cellColor = highlighted ? _highlightedRowAndColumnColor : _cellColor;
            var headerColor = highlighted ? _highlightedHeaderColor : _headerColor;

            _table.ColorRange(new(1, y, 10, 1), cellColor);
            _table.ColorCell(new(0, y), headerColor);
        }

        private void SetColumnColor(int x, bool highlighted)
        {
            var cellColor = highlighted ? _highlightedRowAndColumnColor : _cellColor;
            var headerColor = highlighted ? _highlightedHeaderColor : _headerColor;

            _table.ColorRange(new(x, 1, 1, 10), cellColor);
            _table.ColorCell(new(x, 0), headerColor);
        }
    }
}
