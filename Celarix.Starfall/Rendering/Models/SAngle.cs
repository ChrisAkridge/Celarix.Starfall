using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SAngle
    {
        public decimal Degrees { get; init; }
        public readonly decimal Radians => Degrees * (decimal)(Math.PI / 180);
    }
}
