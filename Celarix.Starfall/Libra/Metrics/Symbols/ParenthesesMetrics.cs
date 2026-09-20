using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Metrics.Symbols
{
    public sealed record ParenthesesMetrics
    {
        public double ParenthesesWidthEm { get; set; } = 0.2d;
        public double ParenthesesHeightEm { get; set; } = 1.0d;

        public double EndThicknessEm { get; set; } = 0.035d;
        public double EndVerticalInsetEm { get; set; } = 0.035d;

        /// <summary>
        /// Gets or sets how far left (or right) the control points of the inner curve are pulled.
        /// Units are in fractions of the total width.
        /// </summary>
        public double InnerBulge { get; set; } = 0.55d;

        /// <summary>
        /// Gets or sets how far left (or right) the control points of the outer curve are pulled.
        /// Expressed as a fraction of the total width.
        /// </summary>
        public double OuterBulge { get; set; } = 0.95d;

        public double UpperControlYFraction = 0.18d;
        public double LowerControlYFraction = 0.82d;
    }
}
