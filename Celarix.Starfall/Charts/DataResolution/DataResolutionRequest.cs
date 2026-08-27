using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public readonly record struct DataResolutionRequest(
    XRange VisibleRange,
    double PixelWidth,
    double TargetBucketWidth
);
