using Celarix.Starfall.Rendering.Models;
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
        void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center);
        void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation);
        void DrawLine(SPointF start, SPointF end, SColor color, float thickness);
        void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation);
        void DrawImage(SImage image, SRectF bounds);

        float FitTextToWidth(string text, SFont font, float width);
        float FitTextToHeight(string text, SFont font, float height);
        SSizeF MeasureText(string text, SFont font);

        IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size);
    }
}
