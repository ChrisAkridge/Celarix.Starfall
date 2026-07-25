using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public sealed class FractionExpression : LibraExpression
    {
        private const double ScriptScale = 0.8d;
        private const double SidePaddingEm = 0.12d;
        private const double NumeratorGapEm = 0.15d;
        private const double DenominatorGapEm = 0.15d;
        private const double VinculumThicknessEm = 0.06d;

        public LibraExpression Numerator { get; set; }
        public LibraExpression Denominator { get; set; }

        public FractionExpression(LibraExpression numerator,
            LibraExpression denominator,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null)
        {
            Numerator = numerator;
            Denominator = denominator;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Id = id;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var numerator = Numerator.Layout(context.ScaleFont(ScriptScale));
            var denominator = Denominator.Layout(context.ScaleFont(ScriptScale));

            var sidePadding = context.Em * SidePaddingEm;
            var numeratorGap = context.Em * NumeratorGapEm;
            var denominatorGap = context.Em * DenominatorGapEm;
            var barThickness = context.Em * VinculumThicknessEm;

            var width =
                Math.Max(numerator.Bounds.Width, denominator.Bounds.Width)
                + (sidePadding * 2);

            var numeratorX = (width - numerator.Bounds.Width) / 2d;
            var denominatorX = (width - denominator.Bounds.Width) / 2d;
            var barY = numerator.Bounds.Height + numeratorGap;
            var denominatorY = barY + barThickness + denominatorGap;

            var baselineY = denominatorY + denominator.BaselineY;
            var mathAxisY = barY + (barThickness / 2d);

            var bar = new LibraRectangleRenderable(
                new SSizeF(width, barThickness),
                ForegroundColor)
            {
                Position = new SPointF(0, barY)
            };

            var numeratorRenderables = MoveRenderablesCopy(numerator.Renderables, new SPointF(numeratorX, 0));
            var denominatorRenderables = MoveRenderablesCopy(denominator.Renderables, new SPointF(denominatorX, denominatorY));

            Console.WriteLine(
                $"Fraction: baseline={baselineY}, axis={mathAxisY}, " +
                $"bar center={barY + barThickness / 2d}");

            return new LibraLayoutResult(
                numeratorRenderables
                    .Concat([bar])
                    .Concat(denominatorRenderables),
                new SRectF(0, 0, width, denominatorY + denominator.Bounds.Height),
                baselineY,
                mathAxisY
            );
        }
    }
}
