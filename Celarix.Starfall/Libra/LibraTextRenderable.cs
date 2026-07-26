using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Libra
{
    public sealed class LibraTextRenderable : LibraFontRenderable
    {
        public string Text { get; set; }

        public LibraTextRenderable(LibraRenderableKey key, string text, SFont font, SColor foregroundColor, SColor backgroundColor)
            : base(key, font, foregroundColor, backgroundColor)
        {
            Text = text;
        }

        public override LibraRenderable Clone()
        {
            return new LibraTextRenderable(Key, Text, Font, ForegroundColor, BackgroundColor)
            {
                Position = Position,
                Size = Size
            };
        }

        public override void RenderAt(IRenderTarget target, SPointF position, double scaleFactor)
        {
            var newBounds = new SRectF(position, Size * scaleFactor);
            target.DrawRectangle(newBounds, BackgroundColor.WithOpacity(Opacity), SPaintStyle.Fill, SAngle.Zero);
            target.DrawTextDirectly(Text, Font.WithSize((Font.Size ?? 12f) * (float)scaleFactor), newBounds, ForegroundColor.WithOpacity(Opacity), SAngle.Zero);
        }
    }
}
