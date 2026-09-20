using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Metrics
{
    public sealed record FractionMetrics
    {
        public double ScriptScale { get; init; } = 0.8d;
        public double SidePaddingEm { get; init; } = 0.12d;
        public double NumeratorGapEm { get; init; } = 0.15d;
        public double DenominatorGapEm { get; init; } = 0.15d;
        public double VinculumThicknessEm { get; init; } = 0.06d;
    }
}
