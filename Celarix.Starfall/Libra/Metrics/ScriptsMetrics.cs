using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Metrics
{
    public sealed record ScriptsMetrics
    {
        public double SuperscriptBaselineRaiseEm { get; init; } = 0.35d;
        public double SubscriptBaselineDropEm { get; init; } = 0.20d;
        public double HorizontalMarginEm { get; init; } = 0.05d;
        public double FontSizeMultiplier { get; init; } = 0.65d;
        public double ClearanceEm { get; init; } = 0.05d;
        public double ScriptGapEm { get; init; } = 0.10d;
        public double SuperscriptSeparationShare { get; init; } = 0.20d;
    }
}
