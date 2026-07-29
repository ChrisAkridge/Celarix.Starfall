using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Libra.Renderables
{
    public abstract class LibraRenderable
    {
        protected internal LibraRenderableKey Key { get; }
        
        public SPointF Position { get; set; }
        public SSizeF Size { get; set; }
        public SRectF Bounds => Position.WithSize(Size);
        
        public SColor ForegroundColor { get; set; }
        public SColor BackgroundColor { get; set; }

        public double Opacity { get; set; } = 1.0;

        public LibraRenderable(LibraRenderableKey key, string? id = null) : this(key, SColor.White, SColor.Transparent)
        {
        }

        public LibraRenderable(LibraRenderableKey key, SColor foregroundColor, SColor backgroundColor)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Key = key;
        }

        public abstract void RenderAt(IRenderTarget target, SPointF position, double scaleFactor);

        public abstract LibraRenderable Clone();
    }

    public abstract class LibraFontRenderable : LibraRenderable
    {
        public SFont Font { get; set; }

        public LibraFontRenderable(LibraRenderableKey key, SFont font, SColor foregroundColor, SColor backgroundColor)
            : base(key, foregroundColor, backgroundColor)
        {
            Font = font;
        }
    }
}
