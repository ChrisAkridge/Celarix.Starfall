using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public readonly record struct DownsamplingContext(
    int BucketsBefore,
    int BucketsAfter
)
{
    public static DownsamplingContext None => new(0, 0);
}