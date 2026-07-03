using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideSF_06_ThankYou : AtriaSlide
    {
        private bool _elementAdded;
        private readonly AnimationContext _animationContext = new();

        public SlideSF_06_ThankYou(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.StarfallBackground;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override SlideAdvanceResult Advance()
        {
            if (!_elementAdded)
            {
                _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
                {
                    BackgroundColor = MathHelpers.InterpolateColor(Constants.StarfallBackground, Constants.FloatingPointBackground, p);
                }, AddElementAndText));
            }

            return SlideAdvanceResult.InternalStateChanged;
        }

        private void AddElementAndText()
        {
            var starfallElement = new StarfallElement("#starfallElement")
            {
                Position = SPointF.Zero,
                Size = Size
            };

            var thankYouText = new TextBlock("#thankYou")
            {
                Text = "Thank you!",
                FontFamily = "Calibri",
                FontSize = 96f,
                Color = SColor.White
            };
            var thankYouAnchor = new BasisPoint(Center, "#thankYouAnchor");
            thankYouText.AnchorCenterTo(thankYouAnchor);

            Add([starfallElement, thankYouAnchor, thankYouText])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }
    }
}
