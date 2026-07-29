using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Celarix.Starfall.Libra.Expressions
{
    public sealed class RowExpression : LibraExpression
    {
        public IReadOnlyList<LibraExpression> Children { get; set; }
        public double GapEm { get; set; }
        public int AlignToChildIndex { get; set; } = 0;

        public RowExpression(IEnumerable<LibraExpression> children,
            double gapEm,
            string? id = null) : base(id)
        {
            Children = [.. children];
            GapEm = gapEm;
            ForegroundColor = SColor.Transparent;
            BackgroundColor = SColor.Transparent;
        }

        private RowExpression(IEnumerable<LibraExpression> children,
            double gapEm,
            LibraId libraId) : base(libraId)
        {
            Children = [.. children];
            GapEm = gapEm;
            ForegroundColor = SColor.Transparent;
            BackgroundColor = SColor.Transparent;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var childLayouts = Children.Select(c => c.Layout(context)).ToArray();
            var aligningToChildIndex = AlignToChildIndex >= 0 && AlignToChildIndex < Children.Count
                ? AlignToChildIndex
                : 0;
            var aligningToMathAxisY = childLayouts[aligningToChildIndex].MathAxisY;
            var referenceBaselineY = childLayouts[aligningToChildIndex].BaselineY;

            var childrenXOffsets = new double[Children.Count];
            var childrenYOffsets = new double[Children.Count];
            var currentXOffset = 0d;
            for (var i = 0; i < childLayouts.Length; i++)
            {
                childrenXOffsets[i] = currentXOffset;
                currentXOffset += childLayouts[i].Bounds.Width;
                if (i < childLayouts.Length - 1)
                {
                    currentXOffset += GapEm * context.Em;
                }

                var childMathAxisY = childLayouts[i].MathAxisY;
                var requiredYOffset = aligningToMathAxisY - childMathAxisY;
                Console.WriteLine($"Child {i} {Children[i].GetType().Name} MathAxisY: {childMathAxisY}, aligningToMathAxisY: {aligningToMathAxisY}, requiredYOffset: {requiredYOffset}");
                childrenYOffsets[i] = requiredYOffset;
            }

            var renderables = new List<LibraRenderable>();
            for (var i = 0; i < childLayouts.Length; i++)
            {
                var childLayout = childLayouts[i];
                var childXOffset = childrenXOffsets[i];
                var childYOffset = childrenYOffsets[i];
                renderables.AddRange(childLayout.RenderablesMovedBy(new SPointF(childXOffset, childYOffset)));
            }

            var rawBounds = SRectF.BoundsOf(renderables.Select(r => r.Bounds));
            var normalizationOffset = new SPointF(-rawBounds.Left, -rawBounds.Top);
            MoveRenderables(renderables, normalizationOffset);

            return new LibraLayoutResult(renderables,
                rawBounds.At(SPointF.Zero),
                referenceBaselineY + normalizationOffset.Y,
                aligningToMathAxisY + normalizationOffset.Y);
        }

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            var newChildren = Children.Select(c => ReplaceChild(c, querySelector, replacementFactory)).ToArray();

            for (var i = 0; i < newChildren.Length; i++)
            {
                var oldChild = Children[i];
                var newChild = newChildren[i];
                if (!ReferenceEquals(oldChild, newChild))
                {
                    return WithChildren(newChildren);
                }
            }

            return this;
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

        private RowExpression WithChildren(IEnumerable<LibraExpression> newChildren)
        {
            return new RowExpression(newChildren, GapEm, Id);
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => Children;
    }
}
