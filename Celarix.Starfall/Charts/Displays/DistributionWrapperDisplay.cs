using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public sealed class DistributionWrapperDisplay : IChartDisplay
{
    private readonly IChartDisplay _wrappedDisplay;

    public AnimationContext? AnimationContext { get; set; }

    public void Render(IRenderTarget target, SRectF displayBounds)
    {
        _wrappedDisplay.Render(target, displayBounds);
    }
}
