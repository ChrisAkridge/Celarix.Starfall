using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models.Path
{
    public sealed record SQuadraticTo(double ControlX,
        double ControlY,
        double X,
        double Y) : SPathCommand;
}
