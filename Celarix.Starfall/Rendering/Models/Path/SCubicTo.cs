using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models.Path
{
    public sealed record SCubicTo(double Control1X,
        double Control1Y,
        double Control2X,
        double Control2Y,
        double X,
        double Y) : SPathCommand;
}
