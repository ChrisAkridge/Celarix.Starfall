using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Color
{
    public sealed class GradientProvider
    {
        private SColor _from;
        private SColor _to;
        
        public GradientProvider(SColor from, SColor to)
        {
            _from = from;
            _to = to;
        }

        public SColor Sample(double progress)
        {
            // It's not enough to just lerp the RGBA channels, we need to be gamma-correct as well.
            // This avoid gradients looking dark and muddy in the middle. Here's how to do it:
            // 1. Convert the from and to colors to linear space (gamma expansion).
            var fromLinearR = Math.Pow(_from.R / 255.0, 2.2);
            var fromLinearG = Math.Pow(_from.G / 255.0, 2.2);
            var fromLinearB = Math.Pow(_from.B / 255.0, 2.2);
            var toLinearR = Math.Pow(_to.R / 255.0, 2.2);
            var toLinearG = Math.Pow(_to.G / 255.0, 2.2);
            var toLinearB = Math.Pow(_to.B / 255.0, 2.2);
            // TODO: make the above fields instead of recomputing them

            // 2. Lerp the linear RGB channels.
            var linearR = fromLinearR + (toLinearR - fromLinearR) * progress;
            var linearG = fromLinearG + (toLinearG - fromLinearG) * progress;
            var linearB = fromLinearB + (toLinearB - fromLinearB) * progress;

            // 3. Convert the result back to sRGB space (gamma compression).
            var r = (byte)(Math.Pow(linearR, 1.0 / 2.2) * 255);
            var g = (byte)(Math.Pow(linearG, 1.0 / 2.2) * 255);
            var b = (byte)(Math.Pow(linearB, 1.0 / 2.2) * 255);

            // 4. Lerp the alpha channel linearly (alpha is not affected by gamma).
            var a = (byte)(_from.A + (_to.A - _from.A) * progress);
            return SColor.FromArgb(a, r, g, b);
        }
    }
}
