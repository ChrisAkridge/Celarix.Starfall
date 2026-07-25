using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Libra
{
    public sealed class TextExpression : LibraExpression
    {
        public string Text { get; set; }

        public TextExpression(string text, SColor foregroundColor, SColor backgroundColor, string? id = null)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Id = id;
        }

        protected internal override LibraLayoutResult Layout(LibraRenderingContext context)
        {
            var initialTextSize = context.MeasurementService.MeasureText(Text, context.Font);
            var fontMetrics = context.FontMetrics;
            var renderable = new LibraTextRenderable(Text, context.Font, ForegroundColor, BackgroundColor, Id)
            {
                Position = SPointF.Zero,    // Position will be set by the parent expression
                Size = initialTextSize
            };
            return new LibraLayoutResult([renderable],
                new SRectF(0, 0, initialTextSize.Width, initialTextSize.Height),
                context.BaselineY,
                DefaultMathAxisY(context));
        }
    }
}
