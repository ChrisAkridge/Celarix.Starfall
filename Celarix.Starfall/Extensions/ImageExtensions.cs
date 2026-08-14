using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class ImageExtensions
    {
        public static SKImage CreateSKImageFromImageSharp(this Image<Rgba32> image)
        {
            var buffer = new byte[image.Width * image.Height * 4];
            var pixel = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        var pixelData = row[x];
                        buffer[pixel++] = pixelData.R;
                        buffer[pixel++] = pixelData.G;
                        buffer[pixel++] = pixelData.B;
                        buffer[pixel++] = pixelData.A;
                    }
                }
            });

            var skImage = SKImage.FromPixelCopy(
                new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888),
                buffer,
                image.Width * 4);
            return skImage;
        }
    }
}
