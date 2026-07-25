using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public sealed record SFontMetrics(
        double Ascent,
        double Descent,
        double Leading);
}
