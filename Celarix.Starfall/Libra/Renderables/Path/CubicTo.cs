using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables.Path
{
    public sealed record CubicTo(double Control1X,
        double Control1Y,
        double Control2X,
        double Control2Y,
        double X,
        double Y) : LibraPathCommand;
}
