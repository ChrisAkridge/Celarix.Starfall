using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Delphinus
{
    internal record RealizedStyleContext(
        SFont Font,
        SColor BgColor,
        SColor Color,
        string? Id,
        string? Classes,
        string HAlign,
        string VAlign,
        double LineMargin
    );
}
