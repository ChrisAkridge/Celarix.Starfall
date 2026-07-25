using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Libra
{
    public abstract class LibraRenderable
    {
        public string? Id { get; set; }
        
        public SPointF Position { get; set; }
        public SSizeF Size { get; set; }
        public SRectF Bounds => Position.WithSize(Size);
        
        public SColor ForegroundColor { get; set; }
        public SColor BackgroundColor { get; set; }

        public LibraRenderable(string? id = null) : this(SColor.White, SColor.Transparent, id)
        {

        }

        public LibraRenderable(SColor foregroundColor, SColor backgroundColor, string? id = null)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Id = id;
        }

        public abstract void RenderAt(IRenderTarget target, SPointF position, double scaleFactor);

        public abstract LibraRenderable Clone();
    }

    public abstract class LibraFontRenderable : LibraRenderable
    {
        public SFont Font { get; set; }

        public LibraFontRenderable(SFont font, SColor foregroundColor, SColor backgroundColor, string? id = null)
            : base(foregroundColor, backgroundColor, id)
        {
            Font = font;
        }
    }
}
