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
    internal sealed class SlideSF_01_ThisShouldBeProgrammable : AtriaSlide
    {
        private AnimationContext _animationContext = new();

        public SlideSF_01_ThisShouldBeProgrammable(int width, int height) : base(width, height)
        {
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
            {
                BackgroundColor = MathHelpers.InterpolateColor(Constants.FloatingPointBackground, Constants.StarfallBackground, p);
            }));
        }

        public override SlideAdvanceResult Advance()
        {
            var queryResult = Query("#slideText").Any();
            if (!queryResult)
            {
                var slideText = new TextBlock("#slideText")
                {
                    Text = "...this should be programmable.",
                    FontFamily = "Calibri",
                    FontSize = 60d,
                    Color = SColor.White
                };
                var vBasisLine = new BasisLine(TopCenter, BottomCenter);
                var vBasisLineSplit = vBasisLine.SplitAndTakeRight(2f / 3f);
                var slideAnchor = new BasisPoint(vBasisLineSplit.Center, "#slideAnchor");
                slideText.AnchorCenterTo(slideAnchor);

                Add([slideText, slideAnchor])
                    .AnimateBasic(1f, AnimationTypes.FadeIn, Easings.Linear);
                return SlideAdvanceResult.InternalStateChanged;
            }
            return SlideAdvanceResult.CanAdvance;
        }
    }
}
