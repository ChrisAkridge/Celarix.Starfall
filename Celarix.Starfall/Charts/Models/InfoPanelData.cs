using ExtendedNumerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed record InfoPanelData(
    double CurrentValue,
    double Minimum,
    double Maximum,
    double Range,
    double Midpoint,
    double Mean,
    double Median,
    double Mode,
    double PopulationStandardDeviation,
    double SampleStandardDeviation,
    IReadOnlyList<InfoPanelPercentileData>? Percentiles,
    BigInteger Count,
    double Sum
);

public sealed record InfoPanelPercentileData(
    decimal Percentile,
    double Value
);