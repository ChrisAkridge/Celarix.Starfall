using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

// TODO STATS PASS TWO!!! 
public interface IDownsamplingStrategy
{
    IEnumerable<DataPoint> Downsample(IReadOnlyList<DataPoint> points);
}
