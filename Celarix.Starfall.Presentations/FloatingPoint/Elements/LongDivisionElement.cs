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
        private class SolutionRow
        {
            private const int DividerLineStrokeWidth = 4;

            internal static double? _digitWidth;

            public enum RowType
            {
                Subtrahend,
                Difference,
                DividerLine
            }

            // okay this I don't like - it means that rendering now needs to be stateful
            // but I only want divider lines to be as wide as whatever the text above them is,
            private double _lastTextWidth = 0d;

            public RowType Type { get; }
            public string? Value { get; }
            public int IndentLevel { get; }

            public SolutionRow(RowType type, string? value, int indentLevel)
            {
                Type = type;
                Value = value;
                IndentLevel = indentLevel;
            }

            public void Render(IRenderTarget target, double fontSize, double leftX, ref double y)
            {
                var offsetX = leftX + (IndentLevel * _digitWidth!.Value);

                if (Type == RowType.DividerLine)
                {

                }
            }
        }

        private const double FontSize = 48d;

        private string _dividend;
        private string _divisor;
        private string _quotient;

        public LongDivisionElement(string dividend, string divisor, string quotient)
        {
            _dividend = dividend;
            _divisor = divisor;
            _quotient = quotient;
        }


        public override void Render(IRenderTarget target)
        {
            if (SolutionRow._digitWidth == null)
            {
                var measurementService = Slide!.MeasurementService;
                var digitSize = measurementService.MeasureText("0", new SFontFamily("Consolas", FontSize));
                SolutionRow._digitWidth = digitSize.Width;
            }
        }
    }
}
