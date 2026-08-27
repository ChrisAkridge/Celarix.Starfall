using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public static class DataResolver
{
    public static IReadOnlyList<ResolvedDataPoint> Resolve(
        IDataSource dataSource,
        DataResolutionRequest request,
        IResolutionStrategy resolutionStrategy
    )
    {
        throw new NotImplementedException();
    }

    private static IReadOnlyList<XRange> CreateBuckets(DataResolutionRequest request)
    {
        throw new NotImplementedException();
    }
}
