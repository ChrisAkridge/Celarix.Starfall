using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public interface IRenderTarget
    {
        bool CanAnimate { get; }
        bool IsAnimating { get; set; }

        void Start();
        void Complete();

        void Clear(SColor color);
        void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation);
        void DrawEllipse(SPointF center, SSizeF size, SColor color, SPaintStyle paintStyle);
        void DrawRadialGradientCircle(SPointF center, double radius, SColor[] colors, double[] colorPositions, SShaderTileMode tileMode, SBlendMode blendMode);
        void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center);
        void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation);
        void DrawLine(SPointF start, SPointF end, SColor color, float thickness);
        void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation);
        void DrawImage(SImage image, SRectF bounds, double opacity = 1d, SAngle? rotation = null);
        void DrawCroppedImage(SImage image, SRectF sourceRect, SRectF destRect, double opacity = 1d);
        void DrawPoint(SPointF point, SColor color);
        void DrawPath(IEnumerable<SPathCommand> pathCommands, SPathStyle pathStyle);

        float FitTextToWidth(string text, SFont font, float width);
        float FitTextToHeight(string text, SFont font, float height);
        SSizeF MeasureText(string text, SFont font);
        SFontMetrics GetFontMetrics(SFont font);

        IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size);
    }
}
