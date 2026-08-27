using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class MinMaxDownsamplingStrategy : IDownsamplingStrategy
{

    // TODO STATS PASS TWO!!! 
    public IEnumerable<DataPoint> Downsample(IReadOnlyList<DataPoint> points)
    {
        throw new NotImplementedException();
    }
}
