using Celarix.Starfall.Identity;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Libra.Parsing;
using Celarix.Starfall.Libra.Parsing.Syntax;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Expressions
{
    public abstract class LibraExpression
    {
        public LibraId Id { get; }
        public SColor ForegroundColor { get; set; } = SColor.White;
        public SColor BackgroundColor { get; set; } = SColor.Transparent;

        protected internal LibraExpression(string? id = null)
        {
            Id = LibraId.Parse(id);
        }

        protected internal LibraExpression(LibraId libraId)
        {
            Id = libraId;
        }

        protected internal abstract IReadOnlyList<LibraExpression> GetChildren();

        protected internal abstract LibraLayoutResult Layout(LibraRenderingContext context);

        public LibraExpression ReplaceFromRoot(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            if (Id.Matches(querySelector))
            {
                return replacementFactory(this);
            }

            return Replace(querySelector, replacementFactory);
        }

        public abstract LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory);

        public static LibraBuildContext Parse(string expression)
        {
            return new LibraBuildContext(expression,
                SColor.White, SColor.Transparent, 1.0, null, new Dictionary<string, Func<LibraExpression>>());
        }

        internal static IReadOnlyList<LibraExpression> GetDescendants(LibraExpression root)
        {
            var descendants = new List<LibraExpression>();
            var stack = new Stack<LibraExpression>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                descendants.Add(current);
                foreach (var child in current.GetChildren())
                {
                    stack.Push(child);
                }
            }
            return descendants;
        }

        internal static void IdsAreUniqueOrThrow(LibraExpression root)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var expression in GetDescendants(root))
            {
                if (expression.Id.Id == null)
                {
                    continue;
                }
                if (!seen.Add(expression.Id.Id))
                {
                    throw new InvalidOperationException($"Duplicate ID '{expression.Id.Id}' found in expression tree.");
                }
            }
        }

        protected internal static void MoveRenderables(IReadOnlyList<LibraRenderable> renderables, SPointF offset)
        {
            foreach (var renderable in renderables)
            {
                renderable.Position += offset;
            }
        }

        protected internal static IReadOnlyList<LibraRenderable> MoveRenderablesCopy(IReadOnlyList<LibraRenderable> renderables, SPointF offset)
        {
            var copy = new List<LibraRenderable>(renderables.Count);
            foreach (var renderable in renderables)
            {
                var renderableCopy = renderable.Clone();
                renderableCopy.Position += offset;
                copy.Add(renderableCopy);
            }
            return copy;
        }

        protected internal static double DefaultMathAxisY(LibraRenderingContext context)
        {
            return context.BaselineY - context.Em * 0.25;
        }
    }
}
