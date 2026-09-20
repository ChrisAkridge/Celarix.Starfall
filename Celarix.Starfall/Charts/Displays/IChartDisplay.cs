using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public interface IChartDisplay
{
    event EventHandler? DataChanged;
    AnimationContext? AnimationContext { get; set; }
    void Render(IRenderTarget target, SRectF displayBounds);
    void OnContainerChanged();
    InfoPanelText GetInfoPanelText(IEnumerable<decimal> percentiles);
}
