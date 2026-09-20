using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Libra.Expressions
{
    public sealed class TextExpression : LibraExpression
    {
        public string Text { get; set; }

        public TextExpression(string text, SColor foregroundColor, SColor backgroundColor, string? id = null)
            : base(id)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        internal TextExpression(string text, SColor foregroundColor, SColor backgroundColor, LibraId libraId)
            : base(libraId)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var renderable = LayoutText(context, Text, ForegroundColor, BackgroundColor, Id.RenderableKey("text"));
            return new LibraLayoutResult([renderable],
                new SRectF(0, 0, renderable.Size.Width, renderable.Size.Height),
                BaselineY(context),
                MathAxisY(context));
        }

        protected internal override IReadOnlyList<LibraExpression> GetChildren() => [];

        public override LibraExpression Replace(string querySelector, Func<LibraExpression, LibraExpression> replacementFactory)
        {
            return this;
        }

        public static LibraTextRenderable LayoutText(LibraRenderingContext context,
            string text,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraRenderableKey key)
        {
            var initialTextSize = context.MeasurementService.MeasureText(text, context.Font);
            var renderable = new LibraTextRenderable(key, text, context.Font, foregroundColor, backgroundColor)
            {
                Position = SPointF.Zero,    // Position will be set by the parent expression
                Size = initialTextSize
            };
            return renderable;
        }

        public static LibraLayoutResult LayoutTextResult(LibraRenderingContext context,
            string text,
            SColor foregroundColor,
            SColor backgroundColor,
            LibraRenderableKey key)
        {
            var renderable = LayoutText(context, text, foregroundColor, backgroundColor, key);
            return new LibraLayoutResult([renderable],
                new SRectF(0, 0, renderable.Size.Width, renderable.Size.Height),
                BaselineY(context),
                MathAxisY(context));
        }

        // Have them here so if we ever want to change the default behavior for all text expressions, we can do it in one place.
        public static double BaselineY(LibraRenderingContext context) => context.BaselineY;
        public static double MathAxisY(LibraRenderingContext context) => DefaultMathAxisY(context);
    }
}
