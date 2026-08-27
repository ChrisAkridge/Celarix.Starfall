using Celarix.Starfall.Charts.Models;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Displays;

public sealed class BarChartDisplay : IChartDisplay
{
    private readonly DataSeries _dataSeries;

    public BarChartProperties Properties { get; }
    public AxisProperties<BigInteger> XAxisProperties { get; }
    public AxisProperties<double> YAxisProperties { get; }

    public BarChartDisplay(
        DataSeries dataSeries,
        BarChartProperties properties,
        AxisProperties<BigInteger> xAxisProperties,
        AxisProperties<double> yAxisProperties)
    {
        _dataSeries = dataSeries;
        Properties = properties;
        XAxisProperties = xAxisProperties;
        YAxisProperties = yAxisProperties;
    }

    public void Render(IRenderTarget target, SRectF displayBounds)
    {
        var barChartBounds = GetBarChartBounds(displayBounds);
        var visibleSlots = (Properties.XMaximum - Properties.XMinimum) + 1;
        var slotWidth = barChartBounds.Width / (float)visibleSlots;
    }

    private SRectF GetBarChartBounds(SRectF displayBounds)
    {
        var xAxisHeight = displayBounds.Height * XAxisProperties.SizeRatioOfParent;
        var yAxisWidth = displayBounds.Width * YAxisProperties.SizeRatioOfParent;
        return new SRectF(
            displayBounds.X + yAxisWidth,
            displayBounds.Y,
            displayBounds.Width - yAxisWidth,
            displayBounds.Height - xAxisHeight
        );
    }
}
