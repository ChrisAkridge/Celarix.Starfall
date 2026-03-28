using Celarix.Starfall.Layout.Helium.Delphinus.Tokens;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus
{
    internal static class Measurement
    {
        private static void Measure(MeasurementService service, SFont font, Token token, RealizedStyleContext? parentContext)
        {
            var context = RealizeContext(token.StyleContext ?? StyleContext.StartingContext, parentContext);

            if (token is GlyphRun glyphRun)
            {
                token.Size = MeasureTextWithStyle(service, glyphRun.Text, context);
            }
            else if (token is Space)
            {
                token.Size = MeasureTextWithStyle(service, " ", context);
            }
            else if (token is ForcedBreak or HorizontalRule)
            {
                var height = DefaultLineHeightForFont(service, context.Font, 1f);
                token.Size = new SSizeF(double.PositiveInfinity, height);
            }
            else if (token is ImageToken imageToken)
            {
                // TODO: We don't actually have any of the image rendering stuff yet, so we'll just
                // use a default for now.
                var height = DefaultLineHeightForFont(service, context.Font, 1f);
                token.Size = new SSizeF(height, height);
            }
        }

        private static RealizedStyleContext RealizeContext(StyleContext context, RealizedStyleContext? parent)
        {
            float fontSize = 12f;
            if (context.FontSize.EndsWith('%'))
            {
                var percentagePart = context.FontSize[..^1];
                if (float.TryParse(percentagePart, out var percentageValue))
                {
                    if (parent != null)
                    {
                        fontSize = (parent.Font.Size ?? 12f) * (percentageValue / 100f);
                    }
                    else
                    {
                        // No parent context to inherit from, so we can't apply the percentage
                        // We'll just ignore it and use the default font size
                    }
                }
            }
            else if (float.TryParse(context.FontSize, out var absoluteFontSize))
            {
                fontSize = absoluteFontSize;
            }
            var font = new SFontFamily(context.FontFamily, fontSize);
            var color = SColor.FromHtmlAttribute(context.Color, SColor.FromName(context.Color, SColor.Black));
            var bgColor = SColor.FromHtmlAttribute(context.BgColor, SColor.FromName(context.BgColor, SColor.Transparent));

            double lineMargin = 0d;
            if (double.TryParse(context.LineMargin, out var parsedLineMargin))
            {
                // Permit negative values here
                // It'll look odd, but hey, it's your presentation
                lineMargin = parsedLineMargin;
            }

            return new RealizedStyleContext(
                font,
                bgColor,
                color,
                context.Id,
                context.Classes,
                context.HAlign,
                context.VAlign,
                lineMargin
            );
        }
    
        private static SSizeF MeasureTextWithStyle(MeasurementService service, string text, RealizedStyleContext context)
        {
            var font = context.Font;
            var baseSize = service.MeasureText(text, font);
            var sizeRatio = context.Font.Size / font.Size;
            var defaultLineHeight = DefaultLineHeightForFont(service, font, sizeRatio ?? 1f);
            var actualLineMargin = defaultLineHeight * context.LineMargin;
            return new SSizeF(baseSize.Width, baseSize.Height + (actualLineMargin * 2));
        }

        private static double DefaultLineHeightForFont(MeasurementService service, SFont font, float sizeRatio)
        {
            // Very hackish way to get the height of a "normal" line of text in this font, but it should
            // be good enough for our purposes.
            const string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var measuredSize = service.MeasureText(text, font);
            return measuredSize.Height * sizeRatio;
        }
    }
}
