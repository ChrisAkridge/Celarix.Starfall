using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements
{
    public sealed class LineElement : AtriaElement
    {
        private SPointF toPoint;

        public double StrokeWidth { get; set; }
        public SColor StrokeColor { get; set; }

        /// <summary>
        /// Gets or sets the line's destination point. The line's "from point" is implicitly at (0, 0).
        /// This creates a bounding box depending on the stroke width and this point, which is used
        /// by layout and anchoring to place the line on the slide.
        /// </summary>
        public SPointF ToPoint
        {
            get => toPoint;
            set
            {
                toPoint = value;
                var halfStroke = StrokeWidth / 2f;
                var minX = Math.Min(0, toPoint.X) - halfStroke;
                var maxX = Math.Max(0, toPoint.X) + halfStroke;
                var minY = Math.Min(0, toPoint.Y) - halfStroke;
                var maxY = Math.Max(0, toPoint.Y) + halfStroke;
                Size = new SSizeF(maxX - minX, maxY - minY);
            }
        }

        public LineElement(string atriaId)
        {
            Id = AtriaId.Parse(atriaId);
        }

        public static LineElement Between(SPointF from, SPointF to, string atriaId, SColor? color = null, double strokeWidth = 2d)
        {
            color ??= SColor.White;
            var line = new LineElement(atriaId)
            {
                Position = from,
                ToPoint = to - from,  // Calculate relative offset
                StrokeColor = color.Value,
                StrokeWidth = strokeWidth
            };
            return line;
        }

        public override void Render(IRenderTarget target)
        {
            var fromPoint = Position;
            var toPoint = Position + ToPoint;
            target.DrawLine(fromPoint, toPoint, StrokeColor.WithOpacity(Opacity), (float)StrokeWidth);
        }
    }
}
