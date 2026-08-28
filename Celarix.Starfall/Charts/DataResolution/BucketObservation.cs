using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed record BucketObservation(
    XRange Range,
    IReadOnlyList<DataPoint> Points
);
