using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public interface IChartDisplay
{
    AnimationContext? AnimationContext { get; set; }
    void Render(IRenderTarget target, SRectF displayBounds);
}
