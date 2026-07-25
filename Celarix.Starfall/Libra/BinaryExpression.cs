using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Libra
{
    public sealed class BinaryExpression : LibraExpression
    {
        private const double MarginWidthMultipleOfFontSize = 0.75d;

        public string Operator { get; set; }
        public LibraExpression Left { get; set; }
        public LibraExpression Right { get; set; }

        public BinaryExpression(string @operator,
            LibraExpression left,
            LibraExpression right,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null)
        {
            Operator = @operator;
            Left = left;
            Right = right;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Id = id;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var marginWidth =
                context.MeasurementService.MeasureText("m", context.Font).Width
                * MarginWidthMultipleOfFontSize;

            var left = Left.Layout(context);
            var right = Right.Layout(context);

            // It's worth using TextExpression internally so operators follow
            // precisely the same text metric rules as ordinary text.
            var operatorExpression = new TextExpression(
                Operator,
                id: null,
                foregroundColor: ForegroundColor,
                backgroundColor: BackgroundColor);

            var op = operatorExpression.Layout(context);

            var commonAxisY = Math.Max(
                left.MathAxisY,
                Math.Max(op.MathAxisY, right.MathAxisY));

            var leftX = 0d;
            var operatorX = left.Bounds.Width + marginWidth;
            var rightX = operatorX + op.Bounds.Width + marginWidth;

            var leftY = commonAxisY - left.MathAxisY;
            var operatorY = commonAxisY - op.MathAxisY;
            var rightY = commonAxisY - right.MathAxisY;

            var renderables = new List<LibraRenderable>();

            renderables.AddRange(
                MoveRenderablesCopy(left.Renderables, new SPointF(leftX, leftY)));

            renderables.AddRange(
                MoveRenderablesCopy(op.Renderables, new SPointF(operatorX, operatorY)));

            renderables.AddRange(
                MoveRenderablesCopy(right.Renderables, new SPointF(rightX, rightY)));

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));

            // Normalize so this expression's local bounds begin at (0, 0).
            var normalizationOffset = new SPointF(
                -rawBounds.Left,
                -rawBounds.Top);

            MoveRenderables(renderables, normalizationOffset);

            var normalizedBounds = new SRectF(
                0,
                0,
                rawBounds.Width,
                rawBounds.Height);

            var normalizedAxisY = commonAxisY - rawBounds.Top;

            // A binary expression's ordinary baseline should usually follow the
            // baselines of its principal operands, not the center of its total box.
            //
            // For now, use the lowest aligned operand baseline. This remains stable
            // when one operand contains a superscript.
            var commonBaselineY = Math.Max(
                leftY + left.BaselineY,
                Math.Max(
                    operatorY + op.BaselineY,
                    rightY + right.BaselineY));

            var normalizedBaselineY = commonBaselineY - rawBounds.Top;

            Console.WriteLine(
                $"Left: baseline={left.BaselineY}, axis={left.MathAxisY}");
            Console.WriteLine(
                $"Op: baseline={op.BaselineY}, axis={op.MathAxisY}");

            return new LibraLayoutResult(
                renderables,
                normalizedBounds,
                normalizedBaselineY,
                normalizedAxisY);
        }
    }
}
