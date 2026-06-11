using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using ShimSkiaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    internal sealed class ImageTransformSlide : AtriaSlide
    {
        private readonly struct PointTransform
        {
            public readonly SPointF Start { get; }
            public readonly SPointF End { get; }
            public readonly SColor StartColor { get; }
            public readonly SColor EndColor { get; }

            public PointTransform(SPointF start, SPointF end, SColor startColor, SColor endColor)
            {
                Start = start;
                End = end;
                StartColor = startColor;
                EndColor = endColor;
            }

            public SPointF InterpolatePoint(double progress)
            {
                var x = (float)(Start.X + (End.X - Start.X) * progress);
                var y = (float)(Start.Y + (End.Y - Start.Y) * progress);
                return new SPointF(x, y);
            }

            public SColor InterpolateColorRGB(double progress)
            {
                var r = (byte)(StartColor.R + (EndColor.R - StartColor.R) * progress);
                var g = (byte)(StartColor.G + (EndColor.G - StartColor.G) * progress);
                var b = (byte)(StartColor.B + (EndColor.B - StartColor.B) * progress);
                var a = (byte)(StartColor.A + (EndColor.A - StartColor.A) * progress);
                return new SColor(r, g, b, a);
            }
        }

        private const double Duration = 1.0d;

        private readonly List<PointTransform> _pointTransforms;
        private double _elapsedTime;

        public bool Completed => _elapsedTime >= Duration;

        public ImageTransformSlide(string imagePath,
            int width,
            int height,
            Func<SPointF, SPointF> positionTransform,
            Func<SColor, SColor> colorTransform) : base(width, height)
        {
            BackgroundColor = new SColor(8, 0, 130, 255);
            var image = Image.Load<Rgba32>(imagePath);
            var imageSize = new SSizeF(image.Width, image.Height);
            var imageRect = imageSize.CenterAt(Center);
            // This is a test slide, so if you big an image too big, well, it'll overflow

            _pointTransforms = [];
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var startPoint = new SPointF(imageRect.Left + x, imageRect.Top + y);
                    var endPoint = positionTransform(startPoint);
                    var pixelColor = image[x, y];
                    var startColor = new SColor(pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);
                    var endColor = colorTransform(startColor);
                    _pointTransforms.Add(new PointTransform(startPoint, endPoint, startColor, endColor));
                }
            }
        }

        public override void Render(IRenderTarget target)
        {
            base.Render(target);

            var progress = _elapsedTime / Duration;
            foreach (var pointTransform in _pointTransforms)
            {
                var currentPoint = pointTransform.InterpolatePoint(progress);
                var currentColor = pointTransform.InterpolateColorRGB(progress);
                target.DrawPoint(currentPoint, currentColor);
            }
        }

        public override void Update(double deltaTime)
        {
            _elapsedTime += deltaTime;
        }

        public override void Initialize() { }
    }
}
