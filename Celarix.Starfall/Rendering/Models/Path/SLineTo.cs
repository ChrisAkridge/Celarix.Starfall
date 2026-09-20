using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models.Path
{
    public sealed record SLineTo(double X, double Y) : SPathCommand;
}
