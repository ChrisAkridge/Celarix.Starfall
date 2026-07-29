using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public sealed class ParenthesizedExpression : LibraExpression
    {
        private const double InnerMarginEm = 0.225d;

        public LibraExpression Expression { get; set; }

        public ParenthesizedExpression(LibraExpression expression,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null) : base(id)
        {
            Expression = expression;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        private ParenthesizedExpression(LibraExpression expression,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraId libraId) : base(libraId)
        {
            Expression = expression;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newExpression = Expression;
            if (Expression.Id.Matches(querySelector))
            {
                newExpression = replacementFactory(Expression);
            }
            else
            {
                newExpression = Expression.Replace(querySelector, replacementFactory);
            }

            if (ReferenceEquals(Expression, newExpression))
            {
                return this;
            }

            return new ParenthesizedExpression(newExpression, ForegroundColor, BackgroundColor, Id);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [Expression];

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var innerMargin = context.Em * InnerMarginEm;

            var leftParen = TextExpression.LayoutText(context, "(", ForegroundColor, BackgroundColor, Id.RenderableKey("left-paren"));
            var expressionLayout = Expression.Layout(context);
            var rightParen = TextExpression.LayoutText(context, ")", ForegroundColor, BackgroundColor, Id.RenderableKey("right-paren"));

            var commonAxisY = expressionLayout.MathAxisY;

            var leftParenX = 0d;
            var expressionX = leftParenX + leftParen.Bounds.Width + innerMargin;
            var rightParenX = expressionX + expressionLayout.Bounds.Width + innerMargin;

            var leftParenY = commonAxisY - TextExpression.MathAxisY(context);
            var expressionY = commonAxisY - expressionLayout.MathAxisY;
            var rightParenY = commonAxisY - TextExpression.MathAxisY(context);

            var renderables = new List<LibraRenderable>();
            renderables.AddRange(
                MoveRenderablesCopy([leftParen], new SPointF(leftParenX, leftParenY)));
            renderables.AddRange(
                MoveRenderablesCopy(expressionLayout.Renderables, new SPointF(expressionX, expressionY)));
            renderables.AddRange(
                MoveRenderablesCopy([rightParen], new SPointF(rightParenX, rightParenY)));
            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));
            var normalizationOffset = new SPointF(-rawBounds.Left, -rawBounds.Top);

            MoveRenderables(renderables, normalizationOffset);
            var normalizedBounds = rawBounds.At(SPointF.Zero);
            var normalizedAxisY = commonAxisY + normalizationOffset.Y;
            var normalizedBaselineY = expressionLayout.BaselineY + normalizationOffset.Y;

            return new LibraLayoutResult(renderables, normalizedBounds, normalizedAxisY, normalizedBaselineY);
        }
    }
}
