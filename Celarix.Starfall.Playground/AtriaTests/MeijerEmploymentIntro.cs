using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    internal sealed class MeijerEmploymentIntro : AtriaSlide
    {
        private sealed class DayTypeDisplay : AtriaElement
        {
            private readonly SColor[] _typeColors;
            private readonly int[] _dayTypes;
            public float RevealedPart { get; set; }

            public DayTypeDisplay(int[] dayTypes)
            {
                _dayTypes = dayTypes;
                _typeColors =
                [
                    SColor.Orange,    // Working
                    SColor.Blue,      // Non-working
                    SColor.LightBlue, // Paid time off
                    SColor.LightGreen // Unpaid time off
                ];
            }

            public override void Render(IRenderTarget target)
            {
                var revealedPixels = (int)(RevealedPart * _dayTypes.Length);

                for (var i = 0; i < revealedPixels; i++)
                {
                    var color = _typeColors[_dayTypes[i]];
                    var rect = new SRectF(Position.X + i, Position.Y, 1d, Size.Height);
                    target.DrawRectangle(rect, color, SPaintStyle.Fill, SAngle.Zero);
                }
            }
        }

        internal enum State
        {
            Initial,
            ShowTitle,
            ShowCenterDot,
            ExpandTimeline,
            AddYearTicks,
            AddMonthTicks,
            AddDaysAndLegend,
            FadeOut
        }

        private const int CircleWidth = 20;
        private static readonly DateOnly StartDate = new(2013, 9, 17);
        private static readonly DateOnly EndDate = new(2018, 5, 19);
        private static readonly int TotalDays = EndDate.DayNumber - StartDate.DayNumber;

        private StateMachine<State> _stateMachine;

        public MeijerEmploymentIntro(int width, int height) : base(width, height) { }

        public override void Initialize()
        {
            // DebugMode.SetStateImmediate(State.AddYearTicks);
            DebugMode.ShowAnchors = true;

            BackgroundColor = SColor.DarkBlue;
            _stateMachine = new StateMachine<State>(this, State.Initial);
        }

        [StateTransition<State>(State.Initial, State.ShowTitle)]
        private void ToShowTitle()
        {
            var topText = new TextBlock("#topText")
            {
                Text = "Meijer Employment Statistics",
                FontSize = 48,
                Color = SColor.White,
                FontFamily = "Calibri"
            };
            var bottomText = new TextBlock("#bottomText")
            {
                Text = "A Look at the Data",
                FontSize = 32,
                Color = SColor.White,
                FontFamily = "Calibri"
            };

            // Figure out placement
            var vCenterBasis = new BasisLine(TopCenter, BottomCenter);
            var topTextHeight = topText.MeasureText(MeasurementService).Height;
            var margin = topTextHeight / 2; // mess with this until you like the spacing

            var topTextAnchor = new BasisPoint(vCenterBasis.Center
                    .Up(margin)
                    .Up(topTextHeight / 2), "#topTextAnchor");
            topText.AnchorCenterTo(topTextAnchor);
            var bottomTextAnchor = new BasisPoint(vCenterBasis.Center
                    .Down(margin)
                    .Down(topTextHeight / 2), "#bottomTextAnchor");
            bottomText.AnchorCenterTo(bottomTextAnchor);

            Add([topText, bottomText, topTextAnchor, bottomTextAnchor])
                .AnimateBasic(1.0, AnimationTypes.FadeIn, Easings.Linear);
        }


    }
}
