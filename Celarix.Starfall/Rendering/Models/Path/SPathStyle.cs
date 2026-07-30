using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models.Path
{
    public sealed record SPathStyle(
        SColor? Fill,
        SColor? Stroke,
        double StrokeWidth,
        SStrokeCap Cap,
        SStrokeJoin Join
    )
    {
        public SPathStyle WithOpacity(double opacity)
        {
            var newFill = Fill?.WithOpacity(opacity);
            var newStroke = Stroke?.WithOpacity(opacity);
            return new SPathStyle(newFill, newStroke, StrokeWidth, Cap, Join);
        }
    }
}
