using FastCache;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    internal static class SkiaImageCache
    {
        private static readonly double DefaultCacheDurationMinutes = 5d;

        public static SKBitmap GetImage(string filePath)
        {
            if (Cached<SKBitmap>.TryGet(filePath, out var cachedBitmap))
            {
                return cachedBitmap;
            }

            // TODO: have some sort of error fallback image if we can't load it
            // for now just fail loudly
            var skBitmap = SKBitmap.Decode(filePath);
            return cachedBitmap.Save(skBitmap, TimeSpan.FromMinutes(DefaultCacheDurationMinutes));
        }
    }
}
