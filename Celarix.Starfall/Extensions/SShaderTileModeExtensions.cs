using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class SShaderTileModeExtensions
    {
        public static SKShaderTileMode ToSKShaderTileMode(this SShaderTileMode tileMode) => tileMode switch
        {
            SShaderTileMode.Clamp => SKShaderTileMode.Clamp,
            SShaderTileMode.Decal => SKShaderTileMode.Decal,
            SShaderTileMode.Repeat => SKShaderTileMode.Repeat,
            SShaderTileMode.Mirror => SKShaderTileMode.Mirror,
            _ => throw new ArgumentOutOfRangeException(nameof(tileMode))
        };
    }
}
