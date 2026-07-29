using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Expressions
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
            string? id = null) : base(id)
        {
            Numerator = numerator;
            Denominator = denominator;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public FractionExpression(LibraExpression numerator,
            LibraExpression denominator,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraId libraId) : base(libraId)
        {
            Numerator = numerator;
            Denominator = denominator;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var numerator = Numerator.Layout(context.ScaleFont(ScriptScale));
            var denominator = Denominator.Layout(context.ScaleFont(ScriptScale));

            var sidePadding = context.Em * SidePaddingEm;
            var numeratorGap = context.Em * NumeratorGapEm;
            var denominatorGap = context.Em * DenominatorGapEm;
            var barThickness = context.Em * VinculumThicknessEm;

            var width = MathHelpers.PadSides(Math.Max(numerator.Bounds.Width, denominator.Bounds.Width), sidePadding);

            var numeratorX = AlignmentHelper.CenterAlign(width, numerator.Bounds.Width);
            var denominatorX = AlignmentHelper.CenterAlign(width, denominator.Bounds.Width);
            var barY = numerator.Bounds.Height + numeratorGap;
            var denominatorY = barY + barThickness + denominatorGap;

            var baselineY = denominatorY + denominator.BaselineY;
            var mathAxisY = barY + (barThickness / 2d);

            var bar = new LibraRectangleRenderable(
                Id.RenderableKey("vinculum"),
                new SSizeF(width, barThickness),
                ForegroundColor)
            {
                Position = new SPointF(0, barY)
            };

            var numeratorRenderables = MoveRenderablesCopy(numerator.Renderables, new SPointF(numeratorX, 0));
            var denominatorRenderables = MoveRenderablesCopy(denominator.Renderables, new SPointF(denominatorX, denominatorY));

            return new LibraLayoutResult(
                numeratorRenderables
                    .Concat([bar])
                    .Concat(denominatorRenderables),
                new SRectF(0, 0, width, denominatorY + denominator.Bounds.Height),
                baselineY,
                mathAxisY
            );
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [Numerator, Denominator];

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newNumerator = ReplaceChild(Numerator, querySelector, replacementFactory);
            var newDenominator = ReplaceChild(Denominator, querySelector, replacementFactory);

            if (ReferenceEquals(newNumerator, Numerator)
                && ReferenceEquals(newDenominator, Denominator))
            {
                return this;
            }

            return WithChildren(newNumerator, newDenominator);
        }

        private static LibraExpression ReplaceChild(
            LibraExpression child,
            string querySelector,
            Func<LibraExpression, LibraExpression> replacementFactory)
        {
            if (child.Id.Matches(querySelector))
            {
                return replacementFactory(child);
            }

            return child.Replace(querySelector, replacementFactory);
        }

        private FractionExpression WithChildren(LibraExpression newNumerator, LibraExpression newDenominator)
        {
            return new FractionExpression(
                newNumerator,
                newDenominator,
                ForegroundColor,
                BackgroundColor,
                Id);
        }
    }
}
