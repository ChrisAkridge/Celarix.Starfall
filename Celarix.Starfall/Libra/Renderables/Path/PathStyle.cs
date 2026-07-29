using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Renderables.Path
{
    public sealed record PathStyle(
        SColor? Fill,
        SColor? Stroke,
        double StrokeWidth,
        SStrokeCap Cap,
        SStrokeJoin Join
    );
}
