using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables.Path
{
    public sealed record LineTo(double X, double Y) : LibraPathCommand;
}
