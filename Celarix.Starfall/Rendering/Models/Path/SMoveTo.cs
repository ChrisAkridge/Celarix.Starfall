using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models.Path
{
    public sealed record SMoveTo(double X, double Y) : SPathCommand;
}
