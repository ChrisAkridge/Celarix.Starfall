using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements
{
    public sealed class TextBlock : AtriaElement
    {
        public string? Text { get; set; }
        public string? FontFamily { get; set; }
        public double FontSize { get; set; }
        public SColor Color { get; set; }

        public SFont Font
        {
            get
            {
                var fontFamily = FontFamily ?? "Calibri";
                var fontSize = FontSize > 0 ? FontSize : 12;
                return new SFontFamily(fontFamily, (float)fontSize);
            }
        }

        public TextBlock(string atriaIdString)
        {
            Id = AtriaId.Parse(atriaIdString);
        }

        public override void Render(IRenderTarget target)
        {
            target.DrawTextDirectly(Text ?? string.Empty, Font, Position.WithSize(Size), Color, SAngle.Zero);
        }

        public SSizeF MeasureText(MeasurementService measurementService)
        {
            if (Text == null) { return SSizeF.Zero; }
            return measurementService.MeasureText(Text, Font);
        }
    }
}
