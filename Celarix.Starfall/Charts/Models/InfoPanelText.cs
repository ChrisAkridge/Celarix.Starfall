using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed record InfoPanelText(
    ChartText? CurrentValueText,
    ChartText? CurrentValueAlternateText,
    ChartText? MinimumText,
    ChartText? MinimumAlternateText,
    ChartText? MaximumText,
    ChartText? MaximumAlternateText,
    ChartText? RangeText,
    ChartText? RangeAlternateText,
    ChartText? MidpointText,
    ChartText? MidpointAlternateText,
    ChartText? MeanText,
    ChartText? MeanAlternateText,
    ChartText? MedianText,
    ChartText? MedianAlternateText,
    ChartText? ModeText,
    ChartText? ModeAlternateText,
    ChartText? PopulationStandardDeviationText,
    ChartText? PopulationStandardDeviationAlternateText,
    ChartText? SampleStandardDeviationText,
    ChartText? SampleStandardDeviationAlternateText,
    IReadOnlyList<InfoPanelPercentileText>? Percentiles,
    ChartText? CountAndSumText
);

public sealed record InfoPanelPercentileText(
    decimal Percentile,
    ChartText PercentileText
);