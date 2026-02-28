using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SSizeF
    {
        public double Width { get; }
        public double Height { get; }
        public SSizeF(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}
