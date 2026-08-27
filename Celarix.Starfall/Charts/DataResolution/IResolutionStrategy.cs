using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public interface IResolutionStrategy
{
    ResolvedDataPoint Resolve(
        IReadOnlyList<DataPoint> points,
        XRange range);
}
