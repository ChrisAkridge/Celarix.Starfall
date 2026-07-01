using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public sealed class SImage
    {
        private SKImage? _skImage;

        public int Width => _skImage?.Width ?? throw new InvalidOperationException("SImage does not contain a valid SKImage.");
        public int Height => _skImage?.Height ?? throw new InvalidOperationException("SImage does not contain a valid SKImage.");

        public static SImage FromSKImage(SKImage skImage)
        {
            return new SImage { _skImage = skImage };
        }

        public static SImage FromSixLaborsImage(Image<Rgba32> sixLaborsImage)
        {
            // From https://stackoverflow.com/a/79086112/2709212
            var buffer = new byte[sixLaborsImage.Width * sixLaborsImage.Height * 4];
            int pixel = 0;
            sixLaborsImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        var pixelData = row[x];
                        buffer[pixel++] = pixelData.R;
                        buffer[pixel++] = pixelData.G;
                        buffer[pixel++] = pixelData.B;
                        buffer[pixel++] = pixelData.A;
                    }
                }
            });

            var image = SKImage.FromPixelCopy(new SKImageInfo(sixLaborsImage.Width, sixLaborsImage.Height, SKColorType.Rgba8888), buffer, sixLaborsImage.Width * 4);
            return FromSKImage(image);
        }

        public static SImage FromFile(string filePath)
        {
            using var skBitmap = SKBitmap.Decode(filePath);
            var skImage = SKImage.FromBitmap(skBitmap);
            return FromSKImage(skImage);
        }

        internal SKImage ToSKImage() => _skImage ?? throw new InvalidOperationException("SImage does not contain a valid SKImage.");
    }
}
