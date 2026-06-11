using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_01_TitleSlide : AtriaSlide
    {
        private const double TitleFontSize = 64d;
        private const double SubtitleFontSize = TitleFontSize / 2d;
        private const double MeetingFontSize = SubtitleFontSize / 2d;
        private const string TitleFontFamily = "Calibri";

        private bool _titlesShown;

        public SlideFP_01_TitleSlide(int width, int height, MeasurementService measurementService) : base(width, height)
        {
            BackgroundColor = Constants.FloatingPointBackground;

            var title = new TextBlock("#title")
            {
                Text = "Floating Point Numbers, Visualized",
                FontSize = TitleFontSize,
                Color = SColor.White,
                FontFamily = TitleFontFamily,
                Opacity = 0d
            };
            var subtitle = new TextBlock("#subtitle")
            {
                Text = "by Chris Akridge",
                FontSize = SubtitleFontSize,
                Color = SColor.White,
                FontFamily = TitleFontFamily,
                Opacity = 0d
            };

            var hCenterBasis = new BasisLine(LeftCenter, RightCenter);
            var titleHeight = title.MeasureText(measurementService).Height;
            var margin = titleHeight / 2d;

            var titleAnchor = new BasisPoint(hCenterBasis.Center
                .Up(margin)
                .Up(titleHeight / 2d), "#titleAnchor");
            title.AnchorCenterTo(titleAnchor);
            var subtitleAnchor = new BasisPoint(hCenterBasis.Center
                .Down(margin)
                .Down(titleHeight / 2d), "#subtitleAnchor");
            subtitle.AnchorCenterTo(subtitleAnchor);

            Add([title, subtitle, titleAnchor, subtitleAnchor]);

            var meeting = new TextBlock("#meeting")
            {
                Text = "KYOSS July 2026",
                FontSize = MeetingFontSize,
                Color = SColor.White,
                FontFamily = TitleFontFamily,
                Opacity = 0d
            };
            var meetingAnchor = new BasisPoint(BottomRight
                .Up(16d)
                .Left(16d), "#meetingAnchor");
            meeting.AnchorBottomRightTo(meetingAnchor);
            Add([meeting, meetingAnchor]);
        }

        public override void Initialize() { }

        public override SlideAdvanceResult Rewind()
        {
            if (!_titlesShown)
            {
                return SlideAdvanceResult.CanRewind;
            }

            _titlesShown = false;

            var elements = QueryMultiple("#title", "#subtitle", "#meeting");
            foreach (var element in elements)
            {
                element.Animate(e => e.Opacity, 0.75d, 1d, 0d);
            }

            return SlideAdvanceResult.InternalStateChanged;
        }

        public override SlideAdvanceResult Advance()
        {
            if (_titlesShown)
            {
                return SlideAdvanceResult.CanAdvance;
            }

            _titlesShown = true;

            var elements = QueryMultiple("#title", "#subtitle", "#meeting");
            foreach (var element in elements)
            {
                element.Animate(e => e.Opacity, 0.75d, 0d, 1d);
            }

            return SlideAdvanceResult.InternalStateChanged;
        }
    }
}
