using Celarix.Starfall.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public sealed class SkiaPngTarget
    {
        // Just.
        // Code.
        // We can expand what we need to, as we need to.
        public void Render()
        {
            const int width = 1920;
            const int height = 1080;
            var outputPath = $"E:\\Documents\\Files\\Programming\\Starfall\\rect_{DateTime.Now:yyyyMMdd_hhmmss}.png";

            using var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);

            // Clear the canvas
            canvas.Clear(SKColors.CornflowerBlue);

            // Pick the circle color
            SKColors.CornflowerBlue.ToHsl(out var h, out var s, out var l);
            s *= 0.7f;
            var newColor = SKColor.FromHsl(h, s, l);

            // Pick a radius
            const int outerRadius = 20;
            const int innerRadius = 15;

            var points = GetPointsForCircleLattice(width, height, outerRadius);
            foreach (var point in points)
            {
                // Draw a circle at the point
                canvas.DrawCircle(point, innerRadius, new SKPaint
                {
                    Color = newColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                });
            }

            // Save the bitmap as a PNG file
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        }

        private static List<SKPoint> GetPointsForCircleLattice(int width, int height, int circleRadius)
        {
            // The first circle is centered at (radius, radius).
            // We build the first row via adding 2 * radius to the x coordinate until we reach the end of the row.
            // The row may have a partial circle at the end, which is allowed.
            // The next row is a copy of the first, but shifted to the right by radius and down by 2 * radius.
            int diameter = circleRadius * 2;
            var evenRowXCenters = SpacedSequence<int>.AllBetween(circleRadius, circleRadius * 2, -diameter, width + diameter);
            var oddRowXCenters = SpacedSequence<int>.AllBetween(diameter, circleRadius * 2, -diameter, width + diameter);
            var yCenters = SpacedSequence<int>.AllBetween(circleRadius, circleRadius * 2, -diameter, height + diameter);

            var points = new List<SKPoint>();
            var y = circleRadius;
            for (int row = 0; y < height + diameter; row++)
            {
                var xCenters = row % 2 == 0 ? evenRowXCenters : oddRowXCenters;
                foreach (int x in xCenters)
                {
                    points.Add(new SKPoint(x, y));
                }
                y += circleRadius * 2;
            }

            return points;
        }
    }
}
