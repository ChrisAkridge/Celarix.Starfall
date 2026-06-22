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
    internal sealed class SlideFP_07_RulesForMantissas : AtriaSlide
    {
        private enum State
        {
            Initial,
            ShowScientificNotationNumber,
            ShowPartsLabelsAndLines,
            HidePartsLabelsAndLines,
            ShowBadExamples,
            ColorHighMantissaRed,
            ColorLowMantissaRed
        }

        private static readonly SFontFamily _slideFont = new SFontFamily("Consolas", 72f);
        private static readonly double partMargin = 10d;

        private StateMachine<State> _stateMachine;
        private AnimationContext _animationContext;

        public SlideFP_07_RulesForMantissas(int width, int height) : base(width, height)
        {
            _animationContext = new AnimationContext();
            _stateMachine = new StateMachine<State>(this, State.Initial);
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine.CurrentState == State.ColorLowMantissaRed)
            {
                return SlideAdvanceResult.CanAdvance;
            }

            var nextState = _stateMachine.CurrentState + 1;
            _stateMachine.GoToState(nextState);
            return SlideAdvanceResult.InternalStateChanged;
        }

        public override SlideAdvanceResult Rewind()
        {
            if (_stateMachine.CurrentState == State.Initial)
            {
                return SlideAdvanceResult.CanRewind;
            }

            var previousState = _stateMachine.CurrentState - 1;
            _stateMachine.GoToState(previousState);
            return SlideAdvanceResult.InternalStateChanged;
        }

        [StateTransition<State>(State.Initial, State.ShowScientificNotationNumber)]
        private void ShowScientificNotationNumber()
        {
            var goodExample = new TextBlock("#goodExample")
            {
                Text = "8.300 × 10⁹",
                FontFamily = "Consolas",
                FontSize = 72f,
                Color = SColor.White
            };
            var goodAnchor = new BasisPoint(Center, "#goodExampleAnchor");
            goodExample.AnchorCenterTo(goodAnchor);

            Add([goodExample, goodAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowScientificNotationNumber, State.ShowPartsLabelsAndLines)]
        private void ShowPartsLabelsAndLines()
        {
            var goodExample = Query("#goodExample").Single() as TextBlock;
            var characterSize = new SSizeF(goodExample.Size.Width / goodExample.Text.Length,
                goodExample.Size.Height);
            var mantissaTargetX = characterSize.Width * 2.5d;
            var baseTargetX = characterSize.Width * 9d;
            var exponentTargetX = characterSize.Width * 10.5d;
            var exponentTargetY = goodExample.Size.Height * 0.25d;

            var vBasisLine = new BasisLine(TopCenter, BottomCenter);
            var hBasisHeight = vBasisLine.SplitAndTakeLeft(1f / 3f).Center.Y;
            var hBasisLine = new BasisLine(TopLeft.Down(hBasisHeight), TopRight.Down(hBasisHeight));
            var mantissaLabelAnchor = new BasisPoint(hBasisLine.SplitAndTakeLeft(1f / 3f).Center, "#mantissaLabelAnchor");
            var baseLabelAnchor = new BasisPoint(hBasisLine.Center, "#baseLabelAnchor");
            var exponentLabelAnchor = new BasisPoint(hBasisLine.SplitAndTakeRight(3f / 4f).Center, "#exponentLabelAnchor");

            var mantissaLabel = new TextBlock("#mantissaLabel")
            {
                Text = "Mantissa",
                FontFamily = "Consolas",
                FontSize = 36f,
                Color = SColor.White
            };
            mantissaLabel.AnchorCenterTo(mantissaLabelAnchor);

            var baseLabel = new TextBlock("#baseLabel")
            {
                Text = "Base",
                FontFamily = "Consolas",
                FontSize = 36f,
                Color = SColor.White
            };
            baseLabel.AnchorCenterTo(baseLabelAnchor);

            var exponentLabel = new TextBlock("#exponentLabel")
            {
                Text = "Exponent",
                FontFamily = "Consolas",
                FontSize = 36f,
                Color = SColor.White
            };
            exponentLabel.AnchorCenterTo(exponentLabelAnchor);

            var mantissaLine = LineElement.Between(
                mantissaLabelAnchor.Point,
                new SPointF(mantissaTargetX + goodExample.Position.X, goodExample.Bounds.Center.Y),
                "#mantissaLine",
                SColor.White.WithOpacity(0.25d),
                4d);
            mantissaLine.AnchorTopLeftTo(mantissaLabelAnchor);

            var baseLine = LineElement.Between(
                baseLabelAnchor.Point,
                new SPointF(baseTargetX + goodExample.Position.X, goodExample.Bounds.Center.Y),
                "#baseLine",
                SColor.White.WithOpacity(0.25d),
                4d);
            baseLine.AnchorTopLeftTo(baseLabelAnchor);

            var exponentLine = LineElement.Between(
                exponentLabelAnchor.Point,
                new SPointF(exponentTargetX + goodExample.Position.X, exponentTargetY + goodExample.Position.Y),
                "#exponentLine",
                SColor.White.WithOpacity(0.25d),
                4d);
            exponentLine.AnchorTopLeftTo(exponentLabelAnchor);

            Add([mantissaLabel, baseLabel, exponentLabel, mantissaLine, baseLine, exponentLine, mantissaLabelAnchor, baseLabelAnchor, exponentLabelAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowPartsLabelsAndLines, State.HidePartsLabelsAndLines)]
        private void HidePartsLabelsAndLines()
        {
            // CANIMPROVE: Fading out elements and then removing them can be done a LOT better than this.
            AtriaElement[] elementsToFadeOut = [
                Query("#mantissaLabel").Single(),
                Query("#baseLabel").Single(),
                Query("#exponentLabel").Single(),
                Query("#mantissaLine").Single(),
                Query("#baseLine").Single(),
                Query("#exponentLine").Single()
            ];
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                foreach (var element in elementsToFadeOut)
                {
                    element.Opacity = 1d - p;
                }
            }, () =>
            {
                Remove(elementsToFadeOut);
                Remove([(ISlideAddable)QueryBasis("#mantissaLabelAnchor").Single(),
                    (ISlideAddable)QueryBasis("#baseLabelAnchor").Single(),
                    (ISlideAddable)QueryBasis("#exponentLabelAnchor").Single()]);
            }));
        }

        [StateTransition<State>(State.HidePartsLabelsAndLines, State.ShowBadExamples)]
        private void ShowBadExamples()
        {
            var goodExampleAnchor = (BasisPoint)QueryBasis("#goodExampleAnchor").Single();
            var vBasisLine = new BasisLine(TopCenter, BottomCenter);
            var startPosition = goodExampleAnchor.Point;
            var targetPosition = new SPointF(goodExampleAnchor.Point.X, vBasisLine.SplitAndTakeLeft(1f / 3f).Center.Y);
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                goodExampleAnchor.Point = MathHelpers.Ease(startPosition, targetPosition, p, Easings.Land);
            }));

            // Make and show the bad examples immediately
            var badExample1 = new TextBlock("#badExample1")
            {
                Text = "83.00 × 10⁸",
                FontFamily = "Consolas",
                FontSize = 72f,
                Color = SColor.White
            };
            var badAnchor = new BasisPoint(Center, "#badExample1Anchor");
            badExample1.AnchorCenterTo(badAnchor);

            var badExample2 = new TextBlock("#badExample2")
            {
                Text = "0.8300 × 10¹⁰",
                FontFamily = "Consolas",
                FontSize = 72f,
                Color = SColor.White
            };
            var bad2Anchor = new BasisPoint(vBasisLine.SplitAndTakeRight(2f / 3f).Center, "#badExample2Anchor");
            badExample2.AnchorCenterTo(bad2Anchor);

            Add([badExample1, badAnchor, badExample2, bad2Anchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowBadExamples, State.ColorHighMantissaRed)]
        private void ColorHighMantissaRed()
        {
            var badExample1 = (TextBlock)Query("#badExample1").Single();
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                badExample1.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Red, p);
            }));
        }

        [StateTransition<State>(State.ColorHighMantissaRed, State.ColorLowMantissaRed)]
        private void ColorLowMantissaRed()
        {
            var badExample2 = (TextBlock)Query("#badExample2").Single();
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                badExample2.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Red, p);
            }));
        }
    }
}
