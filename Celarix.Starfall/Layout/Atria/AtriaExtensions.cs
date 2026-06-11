using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public static class AtriaExtensions
    {
        public static SSizeF MeasureText(this MeasurementService measurementService, TextBlock textBlock)
        {
            var font = new SFontFamily(textBlock.FontFamily ?? "Calibri", (float)textBlock.FontSize);
            return measurementService.MeasureText(textBlock.Text ?? string.Empty, font);
        }
    }
}
