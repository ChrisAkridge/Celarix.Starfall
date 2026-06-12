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
    internal sealed class SlideFP_02_IntegersAreGoodAtMath : AtriaSlide
    {
        internal enum State
        {
            Initial,
            ShowAdditionProblem,
            ShowMultiplicationProblem,
            ShowDivisionProblem,
        }

        private StateMachine<State> _stateMachine;

        public SlideFP_02_IntegersAreGoodAtMath(int width, int height) : base(width, height) { }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _stateMachine = new StateMachine<State>(this, State.Initial);
        }

        // CANIMPROVE: Maybe make some kind of derived type from AtriaSlide
        // that's like LinearAtriaSlide<TState> where the state machine doesn't
        // need to worry about anything other than just advancing back and forth
        // through a set of states. No state 0 to any of state 2, 3, 7, etc.
        // Just 0 to 1 to 2 to 3, and back down.
        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine.CurrentState == State.ShowDivisionProblem)
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

        // Forward transitions

        [StateTransition<State>(State.Initial, State.ShowAdditionProblem)]
        private void ToShowAdditionProblem()
        {
            // CANIMPROVE: Make a StackMathProblemElement that can display problems like this
            // and handle the layout, presenting itself as a single box. Just use Skia rendering
            // directly, don't worry about wrapping Elements inside it. We can add stuff like
            // multiple terms, carries/borrows, step-by-step later.
            AddProblem(1, "addend", 5, 3, 8, "+");
        }

        [StateTransition<State>(State.ShowAdditionProblem, State.ShowMultiplicationProblem)]
        private void ToShowMultiplicationProblem()
        {
            AddProblem(2, "factor", 5, 3, 15, "×");
        }

        [StateTransition<State>(State.ShowMultiplicationProblem, State.ShowDivisionProblem)]
        private void ToShowDivisionProblem()
        {
            // Rounding on purpose to show that integers cannot always represent exact quotients
            // which is why we need floating point numbers!
            AddProblem(3, "quotient", 4, 8, 0, "÷");
        }

        // Reverse transitions
        [StateTransition<State>(State.ShowAdditionProblem, State.Initial)]
        private void ToInitial()
        {
            // Just remove the elements instead of doing a fadeout for now.
            var addendElements = QueryMultiple("#addend0", "#addend1", "#addendResult", "#addendResultLine", "#addendResultLineAnchor", "#addend1Anchor", "#addend0Anchor");
            Remove(addendElements);
        }

        [StateTransition<State>(State.ShowMultiplicationProblem, State.ShowAdditionProblem)]
        private void ToShowAdditionProblemReverse()
        {
            var factorElements = QueryMultiple("#factor0", "#factor1", "#factorResult", "#factorResultLine", "#factorResultLineAnchor", "#factor1Anchor", "#factor0Anchor");
            Remove(factorElements);
        }

        [StateTransition<State>(State.ShowDivisionProblem, State.ShowMultiplicationProblem)]
        private void ToShowMultiplicationProblemReverse()
        {
            var quotientElements = QueryMultiple("#quotient0", "#quotient1", "#quotientResult", "#quotientResultLine", "#quotientResultLineAnchor", "#quotient1Anchor", "#quotient0Anchor");
            Remove(quotientElements);
        }

        private void AddProblem(int problemIndex, string identifier, int firstTerm, int secondTerm, int result, string operatorSymbol)
        {
            var firstTermBlock = new TextBlock($"#{identifier}0")
            {
                Text = $"  {firstTerm}",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };
            var secondTermBlock = new TextBlock($"#{identifier}1")
            {
                Text = $"{operatorSymbol} {secondTerm}",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };

            var resultBlock = new TextBlock($"#{identifier}Result")
            {
                Text = $"  {result}",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };

            var firstTermSize = MeasurementService.MeasureText(firstTermBlock);
            var secondTermSize = MeasurementService.MeasureText(secondTermBlock);
            var resultSize = MeasurementService.MeasureText(resultBlock);

            var widestTextWidth = Math.Max(firstTermSize.Width, Math.Max(secondTermSize.Width, resultSize.Width));
            var resultLine = new LineElement($"#{identifier}ResultLine")
            {
                StrokeWidth = 4d,
                StrokeColor = SColor.White,
                // From point is implicitly (0, 0) and we let anchoring handle the positioning
                ToPoint = SPointF.Zero.Right(widestTextWidth)
            };

            var problemCenterGap = Size.Width / 4d;
            var termCenter = LeftCenter;

            while (problemIndex > 0)
            {
                termCenter = termCenter.Right(problemCenterGap);
                problemIndex--;
            }

            // Place the line on the center line
            var resultLineAnchor = new BasisPoint(termCenter, $"#{identifier}ResultLineAnchor");
            resultLine.AnchorCenterTo(resultLineAnchor);

            // Place addend1 directly above the line with some margin
            var margin = 4d;
            double addend1DistanceUp = (secondTermSize.Height) / 2d + margin;
            var secondTermAnchor = new BasisPoint(termCenter.Up(addend1DistanceUp), $"#{identifier}1Anchor");
            secondTermBlock.AnchorCenterTo(secondTermAnchor);

            // Place addend0 directly above addend1 with some margin
            double addend0DistanceUp = (secondTermSize.Height) + (firstTermSize.Height / 2d) + (margin * 2d);
            var firstTermAnchor = new BasisPoint(termCenter.Up(addend0DistanceUp), $"#{identifier}0Anchor");
            firstTermBlock.AnchorCenterTo(firstTermAnchor);

            // Place sum directly below the line with some margin
            double sumDistanceDown = (resultSize.Height / 2d) + margin;
            var resultAnchor = new BasisPoint(termCenter.Down(sumDistanceDown), $"#{identifier}ResultAnchor");
            resultBlock.AnchorCenterTo(resultAnchor);

            // Actually add the elements
            Add([firstTermBlock, secondTermBlock, resultLine, resultBlock, firstTermAnchor, secondTermAnchor, resultAnchor])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }
    }
}
