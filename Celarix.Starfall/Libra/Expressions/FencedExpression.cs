using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Expressions
{
    public sealed class FencedExpression : LibraExpression
    {
        public LibraExpression Expression { get; set; }
        public FenceType FenceType { get; set; }

        public FencedExpression(LibraExpression expression,
            FenceType type,
            SColor foregroundColor,
            SColor backgroundColor,
            string? id = null) : base(id)
        {
            Expression = expression;
            FenceType = type;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        private FencedExpression(LibraExpression expression,
            FenceType fenceType,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraId libraId) : base(libraId)
        {
            Expression = expression;
            FenceType = fenceType;
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

            return new FencedExpression(newExpression, FenceType, ForegroundColor, BackgroundColor, Id);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [Expression];

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var metrics = context.Metrics.Fences;
            var innerMargin = context.Em * metrics.InnerMarginEm;

            var expressionLayout = Expression.Layout(context);
            LibraRenderable leftFence;
            LibraRenderable rightFence;

            var requiredHeight = expressionLayout.Bounds.Height + (2 * metrics.VerticalPaddingEm * context.Em);
            var useProceduralFence = (requiredHeight >= (metrics.ProceduralThresholdEm * context.Em))
                || context.FenceRenderingMode == FenceRenderingMode.Procedural;

            if (!useProceduralFence)
            {
                leftFence = TextExpression
                    .LayoutTextResult(context,
                        GetStandardHeightFenceCharacterLeft(FenceType).ToString(),
                        ForegroundColor,
                        BackgroundColor,
                        Id.RenderableKey("left-fence"))
                    .Renderables
                    .Single();
                rightFence = TextExpression
                    .LayoutTextResult(context,
                        GetStandardHeightFenceCharacterRight(FenceType).ToString(),
                        ForegroundColor,
                        BackgroundColor,
                        Id.RenderableKey("right-fence"))
                    .Renderables
                    .Single();
            }
            else
            {
                var pathStyle = new PathStyle(ForegroundColor, null, 1d, SStrokeCap.Butt, SStrokeJoin.Miter);
                leftFence = LibraProceduralSymbols.LeftFence(FenceType, context, pathStyle, Id.InternalId, "left-fence");
                rightFence = LibraProceduralSymbols.RightFence(FenceType, context, pathStyle, Id.InternalId, "right-fence");
            }

            var commonAxisY = expressionLayout.MathAxisY;

            var leftParenX = 0d;
            var expressionX = leftParenX + leftFence.Bounds.Width + innerMargin;
            var rightParenX = expressionX + expressionLayout.Bounds.Width + innerMargin;

            var leftParenY = commonAxisY - TextExpression.MathAxisY(context);
            var expressionY = commonAxisY - expressionLayout.MathAxisY;
            var rightParenY = commonAxisY - TextExpression.MathAxisY(context);

            var renderables = new List<LibraRenderable>();
            renderables.AddRange(
                MoveRenderablesCopy([leftFence], new SPointF(leftParenX, leftParenY)));
            renderables.AddRange(
                MoveRenderablesCopy(expressionLayout.Renderables, new SPointF(expressionX, expressionY)));
            renderables.AddRange(
                MoveRenderablesCopy([rightFence], new SPointF(rightParenX, rightParenY)));
            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));
            var normalizationOffset = new SPointF(-rawBounds.Left, -rawBounds.Top);

            MoveRenderables(renderables, normalizationOffset);
            var normalizedBounds = rawBounds.At(SPointF.Zero);
            var normalizedAxisY = commonAxisY + normalizationOffset.Y;
            var normalizedBaselineY = expressionLayout.BaselineY + normalizationOffset.Y;

            return new LibraLayoutResult(renderables, normalizedBounds, normalizedBaselineY, normalizedAxisY);
        }

        private static char GetStandardHeightFenceCharacterLeft(FenceType fenceType)
        {
            return fenceType switch
            {
                FenceType.Parentheses => '(',
                FenceType.SquareBrackets => '[',
                FenceType.CurlyBraces => '{',
                FenceType.AngleBrackets => '<',
                FenceType.SingleVerticalBars => '|',
                FenceType.DoubleVerticalBars => '‖',
                FenceType.Floor => '⌊',
                FenceType.Ceiling => '⌈',
                FenceType.FancyAngleBrackets => '⟨',
                FenceType.FancyDoubleAngleBrackets => '⟪',
                _ => throw new NotImplementedException($"No left fence character defined for fence type {fenceType}.")
            };
        }

        private static char GetStandardHeightFenceCharacterRight(FenceType fenceType)
        {
            return fenceType switch
            {
                FenceType.Parentheses => ')',
                FenceType.SquareBrackets => ']',
                FenceType.CurlyBraces => '}',
                FenceType.AngleBrackets => '>',
                FenceType.SingleVerticalBars => '|',
                FenceType.DoubleVerticalBars => '‖',
                FenceType.Floor => '⌋',
                FenceType.Ceiling => '⌉',
                FenceType.FancyAngleBrackets => '⟩',
                FenceType.FancyDoubleAngleBrackets => '⟫',
                _ => throw new NotImplementedException($"No right fence character defined for fence type {fenceType}.")
            };
        }
    }
}
