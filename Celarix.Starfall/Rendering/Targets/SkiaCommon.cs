using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Converters;
using Celarix.Starfall.Rendering.Models;
using HarfBuzzSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Celarix.Starfall.Rendering.Targets
{
    internal static class SkiaCommon
    {
        public static void Clear(SKCanvas canvas, SColor color)
        {
            canvas.Clear(color.ToSKColor());
        }

        public static void DrawRectangle(SKCanvas canvas, SRectF bounds, SColor color, SPaintStyle paintStyle)
        {
            // TODO: implement rotation
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                Style = paintStyle.ToSKPaintStyle(),
                IsAntialias = true
            };
            canvas.DrawRect(bounds.ToSKRect(), paint);
        }

        public static void DrawText(SKCanvas canvas,
            string text,
            SFont font,
            SRectF bounds,
            SColor color,
            SAngle rotation,
            Alignment alignment = Alignment.Center)
        {
            // TODO: Implement rotation
            SKFont skFont;
            var useFontSize = font.Size != null;
            var skColor = color.ToSKColor();

            using var paint = new SKPaint
            {
                Color = skColor,
                IsAntialias = true
            };

            // Expand the text to fit the widest dimension it can. We then want to center the text in
            // the bounds, which could mean quite a big offset on the other dimension (i.e. if the bounds
            // were 1000x100000 or something).
            float fittedSize;
            if (bounds.Height >= bounds.Width)
            {
                // ###
                // ###
                // ###
                // ###
                // ###, so the text can't be very wide
                fittedSize = SkiaTextRendering.FitTextToWidth(text, font, (float)bounds.Width);
            }
            else
            {
                // #####
                // #####, so the text can't be very tall
                fittedSize = SkiaTextRendering.FitTextToHeight(text, font, (float)bounds.Height);
            }

            skFont = useFontSize
                ? SkiaTextRendering.GetFont(font)
                : SkiaTextRendering.GetFont(font.WithSize(fittedSize));

            SSizeF measuredNewSize = skFont.MeasureShapedText(text);
            if (!useFontSize)
            {
                bounds = AlignmentHelper.Align(alignment, bounds, measuredNewSize).WithSize(measuredNewSize);
            }
            else if (measuredNewSize.Width > bounds.Width || measuredNewSize.Height > bounds.Height)
            {
                // Too big! Reduce the font size until it fits. We can do this by finding the longest
                // axis of the measured size and computing the scale factor between that and the corresponding
                // axis of the bounds, then applying that scale factor to the font size.
                var widthScale = (float)bounds.Width / measuredNewSize.Width;
                var heightScale = (float)bounds.Height / measuredNewSize.Height;
                var scale = Math.Min(widthScale, heightScale);
                skFont = font.WithSize(skFont.Size * (float)scale).ToSKFont();
            }

            var alignedPosition = AlignmentHelper.Align(alignment, bounds, measuredNewSize);
            bounds = new SRectF(alignedPosition.X, alignedPosition.Y, measuredNewSize.Width, measuredNewSize.Height);

            canvas.DrawShapedText(text,
                (float)bounds.Left,
                (float)bounds.Top - skFont.Metrics.Ascent,
                skFont,
                paint);
        }

        public static void DrawTextDirectly(SKCanvas canvas,
            string text,
            SFont font,
            SRectF bounds,
            SColor color,
            SAngle rotation)
        {
            // TODO: Implement rotation
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                IsAntialias = true
            };

            // Use the font size provided by the layout system as-is
            var skFont = SkiaTextRendering.GetFont(font);

            // Measure for alignment only; DO NOT rescale the font
            var measured = skFont.MeasureShapedText(text);

            var drawLeft = (float)bounds.Left;
            var drawTop = (float)bounds.Top;

            // Convert top-based coordinate to baseline for Skia
            var baselineY = drawTop - skFont.Metrics.Ascent;

            canvas.DrawShapedText(text, drawLeft, baselineY, skFont, paint);
        }

        public static void DrawLine(SKCanvas canvas, SPointF start, SPointF end, SColor color, float thickness)
        {
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                StrokeWidth = thickness,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            canvas.DrawLine(start.ToSKPoint(), end.ToSKPoint(), paint);
        }

        public static void DrawImageFromFile(SKCanvas canvas, string filePath, SRectF bounds, double opacity, SAngle rotation)
        {
            // TODO: Implement rotation
            var bitmap = SkiaImageCache.GetImage(filePath);
            using var paint = new SKPaint
            {
                Color = SKColors.White.WithAlpha((byte)(opacity * 255)),
                IsAntialias = true
            };
            if (bitmap != null)
            {
                canvas.DrawBitmap(bitmap, bounds.ToSKRect(), paint);
            }
        }

        public static void DrawImage(SKCanvas canvas, SImage image, SRectF bounds)
        {
            canvas.DrawImage(image.ToSKImage(), bounds.ToSKRect());
        }
    }
}
