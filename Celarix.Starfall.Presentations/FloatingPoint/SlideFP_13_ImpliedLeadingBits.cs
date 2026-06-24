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
    internal sealed class SlideFP_13_ImpliedLeadingBits : AtriaSlide
    {
        private enum State
        {
            Initial,
            ShowDecimalValues,
            ColorDecimalValues,
            RemoveDecimalValuesAndShowBinaryValues,
            ColorBinaryValues,
            RemoveBadBinaryValue
        }

        private StateMachine<State> _stateMachine;
        private AnimationContext _animationContext = new();
        internal static readonly string[] sourceArray = ["Left", "Right"];

        public SlideFP_13_ImpliedLeadingBits(int width, int height) : base(width, height)
        {
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
            if (_stateMachine.CurrentState == State.RemoveBadBinaryValue)
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

        [StateTransition<State>(State.Initial, State.ShowDecimalValues)]
        private void ShowDecimalValues()
        {
            var hBasisLine = new BasisLine(LeftCenter, RightCenter);
            var leftColumnX = hBasisLine.SplitAndTakeLeft(1f / 2f).Center.X;
            var rightColumnX = hBasisLine.SplitAndTakeRight(1f / 2f).Center.X;
            var leftBasisLine = new BasisLine(new(leftColumnX, 0d), new(leftColumnX, Size.Height));
            var rightBasisLine = new BasisLine(new(rightColumnX, 0d), new(rightColumnX, Size.Height));

            var leftColumnYPositions = MathHelpers.EquallySpaceCenteredPoints(0, Size.Height, 5);
            var rightColumnYPositions = MathHelpers.EquallySpaceCenteredPoints(0, Size.Height, 5);

            var leftText = new string[]
            {
                "0.278 × 10⁰",
                "1.309 × 10⁷",
                "2.184 × 10³",
                "3.141 × 10¹",
                "4.669 × 10⁴"
            };

            var rightText = new string[]
            {
                "5.989 × 10⁰",
                "6.467 × 10⁷",
                "7.315 × 10³",
                "8.421 × 10¹",
                "9.997 × 10⁴"
            };

            var elementsToAdd = new List<ISlideAddable>();
            for (var i = 0; i < 5; i++)
            {
                var leftTextBlock = new TextBlock($"#textLeft{i}")
                {
                    Text = leftText[i],
                    FontSize = 48d,
                    Color = SColor.White,
                    FontFamily = "Calibri"  // by now you're probably sick of monospaced text, so let's switch it up
                };
                var leftAnchor = new BasisPoint(new(leftColumnX, leftColumnYPositions[i]), $"#anchorLeft{i}");
                leftTextBlock.AnchorCenterTo(leftAnchor);
                elementsToAdd.Add(leftTextBlock);
                elementsToAdd.Add(leftAnchor);

                var rightTextBlock = new TextBlock($"#textRight{i}")
                {
                    Text = rightText[i],
                    FontSize = 48d,
                    Color = SColor.White,
                    FontFamily = "Calibri"
                };
                var rightAnchor = new BasisPoint(new(rightColumnX, rightColumnYPositions[i]), $"#anchorRight{i}");
                rightTextBlock.AnchorCenterTo(rightAnchor);
                elementsToAdd.Add(rightTextBlock);
                elementsToAdd.Add(rightAnchor);
            }

            Add(elementsToAdd)
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowDecimalValues, State.ColorDecimalValues)]
        private void ColorDecimalValues()
        {
            var badText = (TextBlock)Query("#textLeft0").Single();
            var goodText = sourceArray.SelectMany(side => Enumerable.Range(0, 5).Select(i => $"#text{side}{i}"))
                .Where(id => id != "#textLeft0")
                .Select(id => (TextBlock)Query(id).Single())
                .ToArray();

            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    badText.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Red, p);
                    foreach (var text in goodText)
                    {
                        text.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Green, p);
                    }
                }));
        }

        [StateTransition<State>(State.ColorDecimalValues, State.RemoveDecimalValuesAndShowBinaryValues)]
        private void RemoveDecimalValuesAndShowBinaryValues()
        {
            var elementsToRemove = sourceArray.SelectMany(side => Enumerable.Range(0, 5).Select(i => $"#text{side}{i}"))
                .Select(id => Query(id).Single())
                .ToArray();
            Remove(elementsToRemove);

            var hBasisLine = new BasisLine(LeftCenter, RightCenter);
            var points = MathHelpers.EquallySpaceCenteredPoints(0, Size.Width, 2);

            var badBinaryText = new TextBlock("#badBinary")
            {
                Text = "0.010 × 2⁰",
                FontSize = 48d,
                Color = SColor.White,
                FontFamily = "Calibri"
            };
            var badBinaryAnchor = new BasisPoint(new(points[0], Size.Height / 2d), "#anchorBadBinary");
            badBinaryText.AnchorCenterTo(badBinaryAnchor);

            var goodBinaryText = new TextBlock("#goodBinary")
            {
                Text = "1.001 × 2⁷",
                FontSize = 48d,
                Color = SColor.White,
                FontFamily = "Calibri"
            };
            var goodBinaryAnchor = new BasisPoint(new(points[1], Size.Height / 2d), "#anchorGoodBinary");
            goodBinaryText.AnchorCenterTo(goodBinaryAnchor);

            Add([badBinaryText, badBinaryAnchor, goodBinaryText, goodBinaryAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.RemoveDecimalValuesAndShowBinaryValues, State.ColorBinaryValues)]
        private void ColorBinaryValues()
        {
            var badText = (TextBlock)Query("#badBinary").Single();
            var goodText = (TextBlock)Query("#goodBinary").Single();
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    badText.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Red, p);
                    goodText.Color = MathHelpers.InterpolateColor(SColor.White, SColor.Green, p);
                }));
        }

        [StateTransition<State>(State.ColorBinaryValues, State.RemoveBadBinaryValue)]
        private void RemoveBadBinaryValue()
        {
            var badText = Query("#badBinary").Single();
            var badAnchor = (BasisPoint)QueryBasis("#anchorBadBinary").Single();
            Remove([badText, badAnchor]);
        }
    }
}
