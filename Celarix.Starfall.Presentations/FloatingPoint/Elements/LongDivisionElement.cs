using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements
{
    internal sealed class LongDivisionElement : AtriaElement
    {
        private struct TextGridCell
        {
            [Flags]
            public enum BorderSide
            {
                None = 0,
                Top = 0b0001,
                Bottom = 0b0010,
                Left = 0b0100,
                Right = 0b1000
            }

            private SSizeF _cellSize;
            private SFont _font;

            public char Character { get; set; }
            public BorderSide Borders { get; set; }

            public TextGridCell(SSizeF cellSize, SFont font)
            {
                _cellSize = cellSize;
                _font = font;
                Character = ' ';
                Borders = BorderSide.None;
            }

            public void Render(IRenderTarget target, SPoint gridCell)
            {
                // Find the proper coordinates
                var cellBounds = new SRectF(
                    gridCell.X * _cellSize.Width,
                    gridCell.Y * _cellSize.Height,
                    _cellSize.Width,
                    _cellSize.Height);

                // Draw the character
                if (Character != ' ')
                {
                    target.DrawTextDirectly(Character.ToString(), _font, cellBounds, SColor.White, SAngle.Zero);
                }

                // Draw the borders, if any
                if (Borders.HasFlag(BorderSide.Top))
                {
                    target.DrawLine(cellBounds.TopLeft, cellBounds.TopRight, SColor.White, 2f);
                }
                if (Borders.HasFlag(BorderSide.Bottom))
                {
                    target.DrawLine(cellBounds.BottomLeft, cellBounds.BottomRight, SColor.White, 2f);
                }
                if (Borders.HasFlag(BorderSide.Left))
                {
                    target.DrawLine(cellBounds.TopLeft, cellBounds.BottomLeft, SColor.White, 2f);
                }
                if (Borders.HasFlag(BorderSide.Right))
                {
                    target.DrawLine(cellBounds.TopRight, cellBounds.BottomRight, SColor.White, 2f);
                }
            }
        }

        private const float FontSize = 48f;

        private string _dividend;
        private string _divisor;
        private string _quotient;

        private TextGridCell[,] _cells;
        private readonly SFont _font;
        private readonly SSizeF? _cellSize;

        public LongDivisionElement(string dividend, string divisor, string quotient)
        {
            _dividend = dividend;
            _divisor = divisor;
            _quotient = quotient;

            _font = new SFontFamily("Consolas", FontSize);
            _cells = new TextGridCell[50, 50];  // CANIMPROVE: sigh. Hardcoding it all. Realistically, we will struggle with the fade-in more.
        }


        public override void Render(IRenderTarget target)
        {
            
        }
    }
}
