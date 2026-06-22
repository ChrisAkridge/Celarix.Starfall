using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_05_ButWellJustPickBinary : AtriaSlide
    {
        private enum State
        {
            Initial,
            ShowBinaryPlaceValues,
            ShowBinaryExponents,
            ShowDecimalElement,
            ShowDecimalPlaceValuesAndExponents
        }

        private StateMachine<State> _stateMachine;
        private AnimationContext _animationContext;

        public SlideFP_05_ButWellJustPickBinary(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _stateMachine = new StateMachine<State>(this, State.Initial);
            _animationContext = new AnimationContext();

            var binaryElement = new FloatingPointWindowElement("#binaryElement", MeasurementService)
            {
                Size = new SSizeF(Size.Width, Size.Height / 3f),
                ArrowOpacity = 0d,
                WindowOpacity = 0d
            };
            var binaryAnchor = new BasisPoint(Center, "#binaryAnchor");
            binaryElement.AnchorCenterTo(binaryAnchor);

            Add([binaryElement, binaryAnchor])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine.CurrentState == State.ShowDecimalPlaceValuesAndExponents)
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

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
            base.Update(deltaTime);
        }

        // Forward transitions
        [StateTransition<State>(State.Initial, State.ShowBinaryPlaceValues)]
        private void ToBinaryPlaceValues()
        {
            var binaryElement = (FloatingPointWindowElement)Query("#binaryElement").Single();
            binaryElement.SetShowPlaceValues(true);
        }

        [StateTransition<State>(State.ShowBinaryPlaceValues, State.ShowBinaryExponents)]
        private void ToBinaryExponents()
        {
            var binaryElement = (FloatingPointWindowElement)Query("#binaryElement").Single();
            binaryElement.SetShowExponents(true);
        }

        [StateTransition<State>(State.ShowBinaryExponents, State.ShowDecimalElement)]
        private void ToDecimalElement()
        {
            var binaryAnchor = (BasisPoint)QueryBasis("#binaryAnchor").Single();
            var oldBinaryAnchorPosition = binaryAnchor.Point;
            var newBinaryAnchorPosition = new BasisLine(TopCenter, BottomCenter).SplitAndTakeRight(2f / 3f).Center;
            var moveBinaryAnchorAnimation = new FixedDurationAnimation(AtriaLayoutEngine.GlobalFrameNumber, 30, p =>
            {
                binaryAnchor.Point = MathHelpers.Ease(oldBinaryAnchorPosition, newBinaryAnchorPosition, p, Easings.Land);
            });

            var decimalElement = new FloatingPointWindowElement("#decimalElement", MeasurementService)
            {
                Size = new SSizeF(Size.Width, Size.Height / 3f)
            };
            decimalElement.SetDisplayedExponentBase(10);
            decimalElement.ArrowOpacity = 0d;
            decimalElement.WindowOpacity = 0d;
            var decimalAnchor = new BasisPoint(Center, "#decimalAnchor");
            decimalElement.AnchorCenterTo(decimalAnchor);
            Add([decimalElement, decimalAnchor])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);

            // I know we're adding an anchor just to move it immediately but I think it'll look cool
            var oldDecimalAnchorPosition = decimalAnchor.Point;
            var newDecimalAnchorPosition = new BasisLine(TopCenter, BottomCenter).SplitAndTakeLeft(1f / 3f).Center;
            var moveDecimalAnchorAnimation = new FixedDurationAnimation(AtriaLayoutEngine.GlobalFrameNumber, 30, p =>
            {
                decimalAnchor.Point = MathHelpers.Ease(oldDecimalAnchorPosition, newDecimalAnchorPosition, p, Easings.Land);
            });

            _animationContext.ScheduleAnimation(moveBinaryAnchorAnimation);
            _animationContext.ScheduleAnimation(moveDecimalAnchorAnimation);
        }

        [StateTransition<State>(State.ShowDecimalElement, State.ShowDecimalPlaceValuesAndExponents)]
        private void ToDecimalPlaceValuesAndExponents()
        {
            var decimalElement = (FloatingPointWindowElement)Query("#decimalElement").Single();
            decimalElement.SetShowPlaceValues(true);
            decimalElement.SetShowExponents(true);
        }
    }
}
