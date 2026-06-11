using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    public sealed class GigasecondSlide : AtriaSlide
    {
        private static readonly DateTimeOffset BirthTimestamp = new DateTimeOffset(1994, 9, 8, 18, 2, 0, TimeSpan.FromHours(-4d));
        // Source - https://stackoverflow.com/a/17527989
        // Posted by Tim S., modified by community. See post 'Timeline' for change history
        // Retrieved 2026-05-17, License - CC BY-SA 3.0
        private readonly NumberFormatInfo _thousandsSpacedFormatInfo;
        private readonly TextBlock secondsBlock;

        public GigasecondSlide(int width, int height) : base(width, height)
        {
            BackgroundColor = new(255, 255, 255, 255);
            var textElement = new TextBlock("#seconds")
            {
                Text = "0",
                FontSize = 48,
                Color = new SColor(0, 0, 0, 255),
                FontFamily = "Calibri",
                Size = new SSizeF(400f, 200f)
            };
            secondsBlock = textElement;

            var centerBasis = new BasisPoint(Center, "#center");
            textElement.AnchorCenterTo(centerBasis);
            Add([textElement, centerBasis]);

            _thousandsSpacedFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            _thousandsSpacedFormatInfo.NumberGroupSeparator = " ";
        }

        public override void Initialize()
        {
            
        }

        public override void Update(double deltaTime)
        {
            var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-4d));
            var secondsElapsed = (long)(now - BirthTimestamp).TotalSeconds;
            secondsBlock.Text = secondsElapsed.ToString("#,###", _thousandsSpacedFormatInfo);
        }
    }
}
