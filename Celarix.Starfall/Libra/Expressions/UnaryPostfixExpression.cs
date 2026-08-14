using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Expressions
{
    public sealed class UnaryPostfixExpression : LibraExpression
    {
        public LibraExpression Operand { get; set; }
        public string Operator { get; set; }

        public UnaryPostfixExpression(LibraExpression operand,
            string @operator,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null) : base(id)
        {
            Operand = operand;
            Operator = @operator;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        private UnaryPostfixExpression(LibraExpression operand,
            string @operator,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraId libraId) : base(libraId)
        {
            Operand = operand;
            Operator = @operator;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var metrics = context.Metrics.BinaryExpressions;
            var marginWidth = context.Em * metrics.MarginWidthEm;

            var operand = Operand.Layout(context);
            var op = TextExpression.LayoutText(context, Operator, ForegroundColor, BackgroundColor, Id.RenderableKey("operator"));
            Console.WriteLine($"In UnaryPostfixExpression.Layout: operand={operand.Bounds}, op={op.Size}, op id={Id.RenderableKey("operator")}");

            // TOIMPROVE
            var commonAxisY = Math.Max(
                operand.MathAxisY,
                Math.Max(TextExpression.MathAxisY(context), operand.MathAxisY));

            var operatorX = operand.Bounds.Width + marginWidth;
            var operandX = 0d;

            var operatorY = commonAxisY - TextExpression.MathAxisY(context);
            var operandY = commonAxisY - operand.MathAxisY;

            var renderables = new List<LibraRenderable>();

            renderables.AddRange(
                MoveRenderablesCopy([op], new SPointF(operatorX, operatorY)));

            renderables.AddRange(
                MoveRenderablesCopy(operand.Renderables, new SPointF(operandX, operandY)));

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));

            // Normalize so this expression's local bounds begin at (0, 0).
            var normalizationOffset = new SPointF(
                -rawBounds.Left,
                -rawBounds.Top);

            MoveRenderables(renderables, normalizationOffset);

            var normalizedBounds = rawBounds.At(SPointF.Zero);

            var normalizedAxisY = commonAxisY - rawBounds.Top;

            // A unary expression's ordinary baseline should usually follow the
            // baselines of its principal operands, not the center of its total box.
            //
            // For now, use the lowest aligned operand baseline. This remains stable
            // when one operand contains a superscript.
            var commonBaselineY = Math.Max(
                operandY + operand.BaselineY,
                operatorY + TextExpression.BaselineY(context));

            var normalizedBaselineY = commonBaselineY - rawBounds.Top;

            return new LibraLayoutResult(
                renderables,
                normalizedBounds,
                normalizedBaselineY,
                normalizedAxisY);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [Operand];

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newOperand = ReplaceChild(Operand, querySelector, replacementFactory);

            if (ReferenceEquals(newOperand, Operand))
            {
                return this;
            }

            return WithChildren(newOperand);
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

        private UnaryPostfixExpression WithChildren(LibraExpression newOperand)
        {
            return new UnaryPostfixExpression(
                newOperand,
                Operator,
                ForegroundColor,
                BackgroundColor,
                Id);
        }
    }
}
