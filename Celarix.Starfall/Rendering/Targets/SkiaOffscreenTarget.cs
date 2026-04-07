using Celarix.Starfall.Rendering.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public sealed class SkiaOffscreenTarget : IOffscreenRenderTarget
    {
        private readonly SKSurface _surface;
        public SKImage? Snapshot { get; private set; }
        public bool CanAnimate => false;
        public bool IsAnimating { get => false; set { } }
        public SImage? CompletedImage => SImage.FromSKImage(Snapshot);

        public SkiaOffscreenTarget(int width, int height, GRContext? grContext = null)
        {
            var info = new SKImageInfo(width, height);
            _surface = grContext != null
                ? SKSurface.Create(grContext, false, info)
                : SKSurface.Create(info);
        }

        public void Clear(SColor color) => SkiaCommon.Clear(_surface.Canvas, color);

        public void Complete()
        {
            Snapshot?.Dispose();
            Snapshot = _surface.Snapshot();
        }

        public IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size) => this;

        public void DrawImage(SImage image, SRectF bounds) =>
            SkiaCommon.DrawImage(_surface.Canvas, image, bounds);

        public void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation) =>
            SkiaCommon.DrawImageFromFile(_surface.Canvas, filePath, bounds, opacity, rotation);

        public void DrawLine(SPointF start, SPointF end, SColor color, float thickness) =>
            SkiaCommon.DrawLine(_surface.Canvas, start, end, color, thickness);

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation) =>
            SkiaCommon.DrawRectangle(_surface.Canvas, bounds, color, paintStyle);

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center) =>
            SkiaCommon.DrawText(_surface.Canvas, text, font, bounds, color, rotation, alignment);

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation) =>
            SkiaCommon.DrawTextDirectly(_surface.Canvas, text, font, bounds, color, rotation);

        public float FitTextToHeight(string text, SFont font, float height) =>
            SkiaTextRendering.FitTextToHeight(text, font, height);

        public float FitTextToWidth(string text, SFont font, float width) =>
            SkiaTextRendering.FitTextToWidth(text, font, width);

        public SSizeF MeasureText(string text, SFont font) =>
            SkiaTextRendering.GetFont(font).MeasureShapedText(text);

        public void Start() { }
    }
}
