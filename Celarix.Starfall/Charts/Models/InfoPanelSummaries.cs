using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

[Flags]
public enum InfoPanelSummaries
{
    /// <summary>
    /// Displays the last element of the data series (e.g. "Current: 72.3°F").
    /// </summary>
    CurrentValue = 0x1,

    /// <summary>
    /// Displays a graphic showing the minimum, maximum, range, and midpoint of the data series.
    /// </summary>
    RangeLine = 0x2,

    /// <summary>
    /// Displays the mean of the data series (e.g. "Mean: 71.8°F").
    /// </summary>
    Mean = 0x4,

    /// <summary>
    /// Displays the median of the data series (e.g. "Median: 71.5°F").
    /// </summary>
    Median = 0x8,

    /// <summary>
    /// Displays the mode or modes of the data series (e.g. "Mode: 70°F, 72°F").
    /// </summary>
    Mode = 0x10,

    /// <summary>
    /// Displays the population standard deviation of the data series (e.g. "Population Std Dev: 1.2°F").
    /// </summary>
    PopulationStandardDeviation = 0x20,

    /// <summary>
    /// Displays the sample standard deviation of the data series (e.g. "Sample Std Dev: 1.3°F").
    /// </summary>
    SampleStandardDeviation = 0x40,

    /// <summary>
    /// Displays a list of desired percentiles of the data series (e.g. "P1: 70°F, P25: 71°F, P50: 71.5°F, P75: 72°F, P99: 73°F").
    /// </summary>
    Percentiles = 0x80,

    /// <summary>
    /// Displays the count and sum of the data series (e.g. "Count: 10, Sum: 718°F⋅day").
    /// </summary>
    CountAndSum = 0x100
}
