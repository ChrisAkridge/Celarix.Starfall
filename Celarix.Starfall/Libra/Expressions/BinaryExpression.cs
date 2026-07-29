using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Libra.Expressions
{
    public sealed class BinaryExpression : LibraExpression
    {
        public string Operator { get; set; }
        public LibraExpression Left { get; set; }
        public LibraExpression Right { get; set; }

        public BinaryExpression(string @operator,
            LibraExpression left,
            LibraExpression right,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null) : base(id)
        {
            Operator = @operator;
            Left = left;
            Right = right;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        private BinaryExpression(string @operator,
            LibraExpression left,
            LibraExpression right,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraId libraId) : base(libraId)
        {
            Operator = @operator;
            Left = left;
            Right = right;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var metrics = context.Metrics.BinaryExpressions;
            var marginWidth = context.Em  * metrics.MarginWidthEm;

            var left = Left.Layout(context);
            var right = Right.Layout(context);
            var op = TextExpression.LayoutText(context, Operator, ForegroundColor, BackgroundColor, Id.RenderableKey("operator"));
            Console.WriteLine($"In BinaryExpression.Layout: left={left.Bounds}, right={right.Bounds}, op={op.Size}, op id={Id.RenderableKey("operator")}");

            // TOIMPROVE
            var commonAxisY = Math.Max(
                left.MathAxisY,
                Math.Max(TextExpression.MathAxisY(context), right.MathAxisY));

            var leftX = 0d;
            var operatorX = left.Bounds.Width + marginWidth;
            var rightX = operatorX + op.Bounds.Width + marginWidth;

            var leftY = commonAxisY - left.MathAxisY;
            var operatorY = commonAxisY - TextExpression.MathAxisY(context);
            var rightY = commonAxisY - right.MathAxisY;

            var renderables = new List<LibraRenderable>();

            renderables.AddRange(
                MoveRenderablesCopy(left.Renderables, new SPointF(leftX, leftY)));

            renderables.AddRange(
                MoveRenderablesCopy([op], new SPointF(operatorX, operatorY)));

            renderables.AddRange(
                MoveRenderablesCopy(right.Renderables, new SPointF(rightX, rightY)));

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));

            // Normalize so this expression's local bounds begin at (0, 0).
            var normalizationOffset = new SPointF(
                -rawBounds.Left,
                -rawBounds.Top);

            MoveRenderables(renderables, normalizationOffset);

            var normalizedBounds = rawBounds.At(SPointF.Zero);

            var normalizedAxisY = commonAxisY - rawBounds.Top;

            // A binary expression's ordinary baseline should usually follow the
            // baselines of its principal operands, not the center of its total box.
            //
            // For now, use the lowest aligned operand baseline. This remains stable
            // when one operand contains a superscript.
            var commonBaselineY = Math.Max(
                leftY + left.BaselineY,
                Math.Max(
                    operatorY + TextExpression.BaselineY(context),
                    rightY + right.BaselineY));

            var normalizedBaselineY = commonBaselineY - rawBounds.Top;

            return new LibraLayoutResult(
                renderables,
                normalizedBounds,
                normalizedBaselineY,
                normalizedAxisY);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [Left, Right];

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newLeft = ReplaceChild(Left, querySelector, replacementFactory);
            var newRight = ReplaceChild(Right, querySelector, replacementFactory);

            if (ReferenceEquals(newLeft, Left)
                && ReferenceEquals(newRight, Right))
            {
                return this;
            }

            return WithChildren(newLeft, newRight);
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

        private BinaryExpression WithChildren(LibraExpression newLeft, LibraExpression newRight)
        {
            return new BinaryExpression(
                Operator,
                newLeft,
                newRight,
                ForegroundColor,
                BackgroundColor,
                Id);
        }
    }
}
