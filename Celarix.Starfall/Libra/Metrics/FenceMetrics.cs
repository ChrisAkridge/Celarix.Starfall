using Celarix.Starfall.Libra.Metrics.Symbols;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Metrics
{
    public sealed record FenceMetrics
    {
        public required ParenthesesMetrics ParenthesesMetrics { get; init; }

        public double InnerMarginEm { get; init; } = 0.225d;
        public double ProceduralThresholdEm { get; init; } = 1.4d;
        public double VerticalPaddingEm { get; init; } = 0.08d;
    }
}
