using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Models.Path;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Celarix.Starfall.Rendering.Targets
{
    /// <summary>
    /// Draws onto an <see cref="SKCanvas"/> supplied by a host for each frame, such as a canvas
    /// over a UI framework's bitmap. Call <see cref="BeginFrame"/> before asking the layout engine
    /// for a frame and <see cref="EndFrame"/> afterwards; <see cref="FrameCompleted"/> is raised
    /// when the engine finishes drawing a frame.
    /// </summary>
    public sealed class SkiaCanvasTarget : IRenderTarget, IDisposable
    {
        private readonly SKBitmap _idleBitmap = new(1, 1);
        private readonly SKCanvas _idleCanvas;
        private SKCanvas _canvas;

        public SkiaCanvasTarget()
        {
            // Draw calls outside a frame (e.g. the engine clearing with no slide) go to a 1x1 scratch canvas.
            _idleCanvas = new SKCanvas(_idleBitmap);
            _canvas = _idleCanvas;
            SkiaTextRendering.SetShaperCacheDuration(30000);
        }

        public bool CanAnimate => true;

        public bool IsAnimating { get; set; }

        /// <summary>
        /// Raised from <see cref="Complete"/> once a frame has been fully drawn.
        /// </summary>
        public event EventHandler? FrameCompleted;

        public void BeginFrame(SKCanvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void EndFrame()
        {
            _canvas = _idleCanvas;
        }

        public void Clear(SColor color) => SkiaCommon.Clear(_canvas, color);

        public void PushTransform(STransform2D transform) => SkiaCommon.PushTransform(_canvas, transform);

        public void PopTransform() => SkiaCommon.PopTransform(_canvas);

        public void Complete()
        {
            _canvas.Flush();
            FrameCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation) =>
            SkiaCommon.DrawRectangle(_canvas, bounds, color, paintStyle, rotation);

        public void DrawEllipse(SPointF center, SSizeF size, SColor color, SPaintStyle paintStyle) =>
            SkiaCommon.DrawEllipse(_canvas, center, size, color, paintStyle);

        public void DrawRadialGradientCircle(SPointF center, double radius, SColor[] colors, double[] colorPositions, SShaderTileMode tileMode, SBlendMode blendMode) =>
            SkiaCommon.DrawRadialGradientCircle(_canvas, center, radius, colors, colorPositions, tileMode, blendMode);

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center) =>
            SkiaCommon.DrawText(_canvas, text, font, bounds, color, rotation, alignment);

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation) =>
            SkiaCommon.DrawTextDirectly(_canvas, text, font, bounds, color, rotation);

        public void DrawLine(SPointF start, SPointF end, SColor color, float thickness) =>
            SkiaCommon.DrawLine(_canvas, start, end, color, thickness);

        public void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation) =>
            SkiaCommon.DrawImageFromFile(_canvas, filePath, bounds, opacity, rotation);

        public void DrawImage(SImage image, SRectF bounds, double opacity = 1d, SAngle? rotation = null) =>
            SkiaCommon.DrawImage(_canvas, image, bounds, opacity, rotation ?? SAngle.Zero);

        public void DrawCroppedImage(SImage image, SRectF sourceRect, SRectF destRect, double opacity = 1d) =>
            SkiaCommon.DrawCroppedImage(_canvas, image, sourceRect, destRect, opacity);

        public void DrawPoint(SPointF point, SColor color) =>
            SkiaCommon.DrawPoint(_canvas, point, color);

        public void DrawPath(IEnumerable<SPathCommand> pathCommands, SPathStyle pathStyle) =>
            SkiaCommon.DrawPath(_canvas, pathCommands, pathStyle);

        public float FitTextToHeight(string text, SFont font, float height) =>
            SkiaTextRendering.FitTextToHeight(text, font, height);

        public float FitTextToWidth(string text, SFont font, float width) =>
            SkiaTextRendering.FitTextToWidth(text, font, width);

        public SSizeF MeasureText(string text, SFont font) => SkiaTextRendering.GetFont(font).MeasureShapedText(text);

        public SFontMetrics GetFontMetrics(SFont font)
        {
            var f = SkiaTextRendering.GetFont(font).GetFontMetrics(out var skMetrics);
            return new SFontMetrics(skMetrics.Ascent, skMetrics.Descent, skMetrics.Leading);
        }

        public IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size) => new SkiaOffscreenTarget((int)size.Width, (int)size.Height);

        public void Start() { }

        public void Dispose()
        {
            _idleCanvas.Dispose();
            _idleBitmap.Dispose();
        }
    }
}
