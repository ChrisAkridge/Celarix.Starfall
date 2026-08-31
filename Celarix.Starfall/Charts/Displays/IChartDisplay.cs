using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public interface IChartDisplay
{
    void Render(IRenderTarget target, SRectF displayBounds);
}
