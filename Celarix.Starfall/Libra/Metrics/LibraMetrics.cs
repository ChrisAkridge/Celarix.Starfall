using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Metrics
{
    public sealed record LibraMetrics
    {
        public required BinaryExpressionMetrics BinaryExpressions { get; init; }
        public required FenceMetrics Fences { get; init; }
        public required FractionMetrics Fractions { get; init; }
        public required ScriptsMetrics Scripts { get; init; }
    }
}
