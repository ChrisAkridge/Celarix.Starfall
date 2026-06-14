using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Basis
{
    public sealed class BasisLine : BasisElement
    {
        public override AtriaId Id { get; protected set; }
        public SPointF From { get; set; }
        public SPointF To { get; set; }

        public SPointF Center => new((From.X + To.X) / 2, (From.Y + To.Y) / 2);

        public BasisLine(SPointF from, SPointF to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Splits the basis line at the specified fraction and returns the left portion as a new BasisLine. The original line is not modified.
        /// </summary>
        /// <param name="fraction">The fraction at which to split the line, between 0 and 1.</param>
        /// <returns>The left portion of the line as a new BasisLine.</returns>
        public BasisLine SplitAndTakeLeft(float fraction)
        {
            var splitPoint = new SPointF(From.X + (To.X - From.X) * fraction, From.Y + (To.Y - From.Y) * fraction);
            var leftLine = new BasisLine(From, splitPoint);
            return leftLine;
        }

        /// <summary>
        /// Splits the basis line at the specified fraction and returns the right portion as a new BasisLine. The original line is not modified.
        /// </summary>
        /// <param name="fraction">The fraction at which to split the line, between 0 and 1.</param>
        /// <returns>The right portion of the line as a new BasisLine.</returns>
        public BasisLine SplitAndTakeRight(float fraction)
        {
            var splitPoint = new SPointF(From.X + (To.X - From.X) * fraction, From.Y + (To.Y - From.Y) * fraction);
            var rightLine = new BasisLine(splitPoint, To);
            return rightLine;
        }

        public override void RenderDebug(IRenderTarget target)
        {
            target.DrawLine(From, To, SColor.Yellow, 3f);
        }
    }
}
