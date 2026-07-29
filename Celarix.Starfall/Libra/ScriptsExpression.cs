using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public sealed class ScriptsExpression : LibraExpression
    {
        private sealed record ScriptPlacement(
            LibraLayoutResult Layout,
            SPointF Offset)
        {
            public SRectF PlacedBounds => Layout.Bounds + Offset;
        }

        private const double SuperscriptBaselineRaiseEm = 0.35d;
        private const double SubscriptBaselineDropEm = 0.20d;
        private const double HorizontalMarginEm = 0.05d;
        private const double FontSizeMultiplier = 0.65d;
        private const double ClearanceEm = 0.05d;
        private const double ScriptGapEm = 0.10d;

        public LibraExpression BaseExpression { get; set; }
        public LibraExpression? Superscript { get; set; }
        public LibraExpression? Subscript { get; set; }

        public ScriptsExpression(LibraExpression baseExpression,
            LibraExpression? superscript,
            LibraExpression? subscript,
            string? id = null) : base(id)
        {
            BaseExpression = baseExpression;
            Superscript = superscript;
            Subscript = subscript;

            // Since the superscript and subscript have their own colors, we don't need to set them here.
            // So set two placeholders.
            ForegroundColor = SColor.Transparent;
            BackgroundColor = SColor.Transparent;
        }

        private ScriptsExpression(LibraExpression baseExpression,
            LibraExpression? superscript,
            LibraExpression? subscript,
            LibraId libraId) : base(libraId)
        {
            BaseExpression = baseExpression;
            Superscript = superscript;
            Subscript = subscript;

            // Since the superscript and subscript have their own colors, we don't need to set them here.
            // So set two placeholders.
            ForegroundColor = SColor.Transparent;
            BackgroundColor = SColor.Transparent;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var renderables = new List<LibraRenderable>();

            var baseLayout = BaseExpression.Layout(context);
            renderables.AddRange(baseLayout.Renderables);

            if (Superscript != null)
            {
                renderables.AddRange(LayoutSuperscript(context, baseLayout.BaselineY, baseLayout.Bounds.Right));
            }

            if (Subscript != null)
            {
                renderables.AddRange(LayoutSubscript(context, baseLayout.BaselineY, baseLayout.Bounds.Right));
            }

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));
            var normalizationOffset = new SPointF(-rawBounds.Left, -rawBounds.Top);
            MoveRenderables(renderables, normalizationOffset);

            var mathAxisY = baseLayout.MathAxisY + normalizationOffset.Y;
            var baselineY = baseLayout.BaselineY + normalizationOffset.Y;

            return new LibraLayoutResult(renderables, rawBounds + normalizationOffset, baselineY, mathAxisY);
        }

        private IEnumerable<LibraRenderable> LayoutSuperscript(LibraRenderingContext context,
            double baseBaselineY,
            double baseRight)
        {
            var verticalOffset = SuperscriptBaselineRaiseEm * context.Em;
            var clearance = ClearanceEm * context.Em;
            var horizontalMargin = HorizontalMarginEm * context.Em;

            var superscriptLayout = Superscript!.Layout(context.ScaleFont(FontSizeMultiplier));
            var targetBaselineY = baseBaselineY - verticalOffset;
            var superscriptY = targetBaselineY - superscriptLayout.BaselineY;
            var maximumBottomY = baseBaselineY - clearance;
            var actualBottomY = superscriptLayout.Bounds.Bottom + superscriptY;

            if (actualBottomY > maximumBottomY)
            {
                superscriptY -= actualBottomY - maximumBottomY;
            }

            var superscriptX = baseRight + horizontalMargin;

            return MoveRenderablesCopy(superscriptLayout.Renderables, new(superscriptX, superscriptY));
        }

        private IEnumerable<LibraRenderable> LayoutSubscript(LibraRenderingContext context,
            double baseBaselineY,
            double baseRight)
        {
            var verticalOffset = SubscriptBaselineDropEm * context.Em;
            var clearance = ClearanceEm * context.Em;
            var horizontalMargin = HorizontalMarginEm * context.Em;

            var subscriptLayout = Subscript!.Layout(context.ScaleFont(FontSizeMultiplier));
            var targetBaselineY = baseBaselineY + verticalOffset;
            var subscriptY = targetBaselineY - subscriptLayout.BaselineY;
            var minimumTopY = baseBaselineY + clearance;
            var actualTopY = subscriptLayout.Bounds.Top + subscriptY;

            if (actualTopY < minimumTopY)
            {
                subscriptY += minimumTopY - actualTopY;
            }

            var subscriptX = baseRight + horizontalMargin;
            return MoveRenderablesCopy(subscriptLayout.Renderables, new(subscriptX, subscriptY));
        }

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newBase = ReplaceChild(BaseExpression, querySelector, replacementFactory);
            LibraExpression? newSuperscript = null;
            LibraExpression? newSubscript = null;

            if (Superscript != null)
            {
                newSuperscript = ReplaceChild(Superscript, querySelector, replacementFactory);
            }

            if (Subscript != null)
            {
                newSubscript = ReplaceChild(Subscript, querySelector, replacementFactory);
            }

            var hasNewBase = !ReferenceEquals(BaseExpression, newBase);
            var hasNewSuperscript = (Superscript != null) && !ReferenceEquals(Superscript, newSuperscript);
            var hasNewSubscript = (Subscript != null) && !ReferenceEquals(Subscript, newSubscript);

            if (hasNewBase || hasNewSuperscript || hasNewSubscript)
            {
                return WithChildren(newBase, newSuperscript, newSubscript);

            }
            else
            {
                return this;
            }
        }

        private static LibraExpression ReplaceChild(LibraExpression child,
            string querySelector,
            Func<LibraExpression, LibraExpression> replacementFactory)
        {
            if (child.Id.Matches(querySelector))
            {
                return replacementFactory(child);
            }

            return child.Replace(querySelector, replacementFactory);
        }

        private ScriptsExpression WithChildren(LibraExpression newBase, LibraExpression? newSuperscript, LibraExpression? newSubscript)
        {
            return new ScriptsExpression(newBase, newSuperscript, newSubscript, Id);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => new[] { BaseExpression,  Superscript, Subscript }.Where(e => e != null).ToArray()!;
    }
}
