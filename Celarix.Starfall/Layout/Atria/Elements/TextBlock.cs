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
        private SColor _color;

        public string? Text { get; set; }
        public string? FontFamily { get; set; }
        public double FontSize { get; set; }
        public SColor Color
        {
            get
            {
                var adjustedAlpha = (byte)(Opacity * _color.A);
                return new SColor(_color.R, _color.G, _color.B, adjustedAlpha);
            }
            set => _color = value;
        }

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
            //target.DrawTextDirectly(Text ?? string.Empty, Font, Position.WithSize(Size), Color, SAngle.Zero);

            if (Size == SSizeF.Zero)
            {
                // If size is not set, measure the text and use that as the size
                var measuredSize = target.MeasureText(Text ?? string.Empty, Font);
                Size = measuredSize;
            }

            target.DrawText(Text ?? string.Empty, Font, Position.WithSize(Size), Color, SAngle.Zero);
        }

        public SSizeF MeasureText(MeasurementService measurementService)
        {
            if (Text == null) { return SSizeF.Zero; }
            return measurementService.MeasureText(Text, Font);
        }

        public void FitFontSize(MeasurementService measurementService)
        {
            FontSize = measurementService.FontSizeForDesiredSize(Text ?? string.Empty, Font, Size);
        }

        public void SetTextAndKeepFontSize(MeasurementService measurementService, string newText)
        {
            Text = newText;
            Size = MeasureText(measurementService);
        }
    }
}
