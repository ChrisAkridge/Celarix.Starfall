using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public sealed class SImage
    {
        private SKImage? _skImage;

        internal static SImage FromSKImage(SKImage skImage)
        {
            return new SImage { _skImage = skImage };
        }

        internal SKImage ToSKImage() => _skImage ?? throw new InvalidOperationException("SImage does not contain a valid SKImage.");
    }
}
