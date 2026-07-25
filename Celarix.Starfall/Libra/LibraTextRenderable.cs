using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Libra
{
    public sealed class LibraTextRenderable : LibraFontRenderable
    {
        public string Text { get; set; }

        public LibraTextRenderable(string text, SFont font, SColor foregroundColor, SColor backgroundColor, string? id = null)
            : base(font, foregroundColor, backgroundColor, id)
        {
            Text = text;
        }

        public override LibraRenderable Clone()
        {
            return new LibraTextRenderable(Text, Font, ForegroundColor, BackgroundColor, Id)
            {
                Position = Position,
                Size = Size
            };
        }

        public override void RenderAt(IRenderTarget target, SPointF position, double scaleFactor)
        {
            var newBounds = new SRectF(position, Size * scaleFactor);
            target.DrawRectangle(newBounds, BackgroundColor, SPaintStyle.Fill, SAngle.Zero);
            target.DrawTextDirectly(Text, Font.WithSize((Font.Size ?? 12f) * (float)scaleFactor), newBounds, ForegroundColor, SAngle.Zero);
        }
    }
}
