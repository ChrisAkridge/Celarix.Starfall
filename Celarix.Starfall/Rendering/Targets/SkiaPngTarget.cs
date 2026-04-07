using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Converters;
using Celarix.Starfall.Rendering.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public sealed class SkiaPngTarget : IRenderTarget
    {
        // Just.
        // Code.
        // We can expand what we need to, as we need to.
        private const string FileNameFormat = "frame_{0:D8}.png";

        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private SkiaPngTargetOptions _options;
        private int _lastFrameIndex = -1;

        public SkiaPngTarget(SkiaPngTargetOptions options)
        {
            _options = options;
            _bitmap = new SKBitmap(options.Width, options.Height);
            _canvas = new SKCanvas(_bitmap);

            Directory.CreateDirectory(options.OutputPath);
        }

        public bool CanAnimate => true;

        public bool IsAnimating { get; set; }

        public void Clear(SColor color) => SkiaCommon.Clear(_canvas, color);

        public void Complete()
        {
            var image = SKImage.FromBitmap(_bitmap);
            var data = image.Encode(SKEncodedImageFormat.Png, 100);

            _lastFrameIndex += 1;
            var filePath = Path.Combine(_options.OutputPath, string.Format(FileNameFormat, _lastFrameIndex));
            var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);

            stream.Close();
            stream.Dispose();
            data.Dispose();
            image.Dispose();
            _canvas.Dispose();
            _bitmap.Dispose();

            _bitmap = new SKBitmap(_options.Width, _options.Height);
            _canvas = new SKCanvas(_bitmap);
        }

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation) =>
            SkiaCommon.DrawRectangle(_canvas, bounds, color, paintStyle);

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center) =>
            SkiaCommon.DrawText(_canvas, text, font, bounds, color, rotation, alignment);

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation) =>
            SkiaCommon.DrawTextDirectly(_canvas, text, font, bounds, color, rotation);

        public void DrawLine(SPointF start, SPointF end, SColor color, float thickness) =>
            SkiaCommon.DrawLine(_canvas, start, end, color, thickness);

        public void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation) =>
            SkiaCommon.DrawImageFromFile(_canvas, filePath, bounds, opacity, rotation);

        public float FitTextToHeight(string text, SFont font, float height) =>
            SkiaTextRendering.FitTextToHeight(text, font, height);

        public float FitTextToWidth(string text, SFont font, float width) =>
            SkiaTextRendering.FitTextToWidth(text, font, width);

        public SSizeF MeasureText(string text, SFont font) => SkiaTextRendering.GetFont(font).MeasureShapedText(text);

        public void Start() { }
    }
}
