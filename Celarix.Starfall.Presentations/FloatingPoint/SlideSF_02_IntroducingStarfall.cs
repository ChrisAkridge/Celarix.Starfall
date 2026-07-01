using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideSF_02_IntroducingStarfall : AtriaSlide
    {
        private int _state = 0;

        public SlideSF_02_IntroducingStarfall(int width, int height) : base(width, height)
        {
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.StarfallBackground;
        }

        public override SlideAdvanceResult Advance()
        {
            if (_state == 0)
            {
                var title = new TextBlock("#title")
                {
                    Text = "Starfall",
                    FontFamily = "Calibri",
                    FontSize = 72f,
                    Color = SColor.White
                };
                var titleAnchor = new BasisPoint(Center, "#titleAnchor");
                title.AnchorCenterTo(titleAnchor);
                Add([titleAnchor, title])
                    .AnimateBasic(1f, AnimationTypes.FadeIn, Easings.Linear);
                _state = 1;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (_state == 1)
            {
                var subtitle = new TextBlock("#subtitle")
                {
                    Text = "Code-first presentations",
                    FontFamily = "Calibri",
                    FontSize = 36f,
                    Color = SColor.White
                };
                var subtitleAnchor = new BasisPoint(new SPointF(Center.X, Center.Y + 90), "#subtitleAnchor");
                subtitle.AnchorCenterTo(subtitleAnchor);
                Add([subtitleAnchor, subtitle])
                    .AnimateBasic(1f, AnimationTypes.FadeIn, Easings.Linear);
                _state = 2;
                return SlideAdvanceResult.InternalStateChanged;

            }
            else
            {
                return SlideAdvanceResult.CanAdvance;
            }
        }
    }
}
