using Celarix.Starfall.Rendering.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Converters
{
    internal static class SkiaConverters
    {
        public static SKRect ToSKRect(this SRectF rect)
        {
            return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }
    }
}
