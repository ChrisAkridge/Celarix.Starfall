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
        private const double SuperscriptSeparationShare = 0.20d;

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

            ScriptPlacement? superscriptPlacement = null;
            ScriptPlacement? subscriptPlacement = null;
            
            if (Superscript != null)
            {
                superscriptPlacement = LayoutSuperscript(context, baseLayout.BaselineY, baseLayout.Bounds.Right);
            }

            if (Subscript != null)
            {
                subscriptPlacement = LayoutSubscript(context, baseLayout.BaselineY, baseLayout.Bounds.Right);
            }

            if (superscriptPlacement != null && subscriptPlacement != null)
            {
                SeparateScripts(context, ref superscriptPlacement, ref subscriptPlacement);
            }

            if (superscriptPlacement != null)
            {
                renderables.AddRange(MoveRenderablesCopy(superscriptPlacement.Layout.Renderables, superscriptPlacement.Offset));
            }

            if (subscriptPlacement != null)
            {
                renderables.AddRange(MoveRenderablesCopy(subscriptPlacement.Layout.Renderables, subscriptPlacement.Offset));
            }

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));
            var normalizationOffset = new SPointF(-rawBounds.Left, -rawBounds.Top);
            MoveRenderables(renderables, normalizationOffset);

            var mathAxisY = baseLayout.MathAxisY + normalizationOffset.Y;
            var baselineY = baseLayout.BaselineY + normalizationOffset.Y;

            return new LibraLayoutResult(renderables, rawBounds + normalizationOffset, baselineY, mathAxisY);
        }

        private ScriptPlacement LayoutSuperscript(LibraRenderingContext context,
            double baseBaselineY,
            double baseRight)
        {
            var baselineRaise = SuperscriptBaselineRaiseEm * context.Em;
            var clearance = ClearanceEm * context.Em;
            var horizontalMargin = HorizontalMarginEm * context.Em;

            var layout = Superscript!.Layout(context.ScaleFont(FontSizeMultiplier));
            var yFromBaselineRaise = baseBaselineY - baselineRaise - layout.BaselineY;
            var yFromBottomClearence = baseBaselineY - clearance - layout.Bounds.Bottom;

            var y = Math.Min(yFromBaselineRaise, yFromBottomClearence);
            var x = baseRight + horizontalMargin;

            return new ScriptPlacement(layout, new(x, y));
        }

        private ScriptPlacement LayoutSubscript(LibraRenderingContext context,
            double baseBaselineY,
            double baseRight)
        {
            var baselineDrop = SubscriptBaselineDropEm * context.Em;
            var clearance = ClearanceEm * context.Em;
            var horizontalMargin = HorizontalMarginEm * context.Em;

            var layout = Subscript!.Layout(context.ScaleFont(FontSizeMultiplier));
            var yFromBaselineDrop = baseBaselineY + baselineDrop - layout.BaselineY;
            var yFromTopClearence = baseBaselineY + clearance - layout.Bounds.Top;
            
            var y = Math.Max(yFromBaselineDrop, yFromTopClearence);
            var x = baseRight + horizontalMargin;

            return new ScriptPlacement(layout, new(x, y));
        }

        private static void SeparateScripts(LibraRenderingContext context, ref ScriptPlacement superscript, ref ScriptPlacement subscript)
        {
            var minimumGap = ScriptGapEm * context.Em;
            var superscriptBounds = superscript.Layout.Bounds + superscript.Offset;
            var subscriptBounds = subscript.Layout.Bounds + subscript.Offset;

            var overlapsHorizontally =
                superscriptBounds.Right > subscriptBounds.Left
                && subscriptBounds.Right > superscriptBounds.Left;

            if (!overlapsHorizontally) { return; }

            var currentGap = subscriptBounds.Top - superscriptBounds.Bottom;

            if (currentGap >= minimumGap) { return; }

            var requiredSeparation = minimumGap - currentGap;
            var superscriptAdjustment = requiredSeparation * SuperscriptSeparationShare;
            var subscriptAdjustment = requiredSeparation - superscriptAdjustment;

            superscript = superscript with { Offset = new(superscript.Offset.X, superscript.Offset.Y - superscriptAdjustment) };
            subscript = subscript with { Offset = new(subscript.Offset.X, subscript.Offset.Y + subscriptAdjustment) };
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
