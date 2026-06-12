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
    internal sealed class SlideFP_03_FloatsAreGoodAtMath : AtriaSlide
    {
        internal enum State
        {
            Initial,
            ShowDivisionProblem,
            ShowMultiplicationProblem,
            ShowAdditionProblem
        }

        private StateMachine<State> _stateMachine;

        public SlideFP_03_FloatsAreGoodAtMath(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _stateMachine = new StateMachine<State>(this, State.Initial);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine.CurrentState == State.ShowAdditionProblem)
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
        [StateTransition<State>(State.Initial, State.ShowDivisionProblem)]
        private void ShowDivisionProblem()
        {
            AddProblem(1, "quotient", "4", "8", Helpers.ExactStringSingle(4f / 8f), "÷");
        }

        [StateTransition<State>(State.ShowDivisionProblem, State.ShowMultiplicationProblem)]
        private void ShowMultiplicationProblem()
        {
            AddProblem(2, "product", "5", "3", Helpers.ExactStringSingle(5f * 3f), "×");
        }

        [StateTransition<State>(State.ShowMultiplicationProblem, State.ShowAdditionProblem)]
        private void ShowAdditionProblem()
        {
            AddProblem(3, "sum", "0.1", "0.2", Helpers.ExactStringSingle(0.1f + 0.2f), "+");
        }

        // Reverse transitions
        [StateTransition<State>(State.ShowDivisionProblem, State.Initial)]
        private void ToInitial()
        {
            // Just remove the elements instead of doing a fadeout for now.
            var addendElements = QueryMultiple("#quotient0", "#quotient1", "#quotientResult", "#quotientResultLine", "#quotientResultLineAnchor", "#quotient1Anchor", "#quotient0Anchor");
            Remove(addendElements);
        }

        [StateTransition<State>(State.ShowMultiplicationProblem, State.ShowDivisionProblem)]
        private void ToShowDivisionProgramReverse()
        {
            var factorElements = QueryMultiple("#product0", "#product1", "#productResult", "#productResultLine", "#productResultLineAnchor", "#product1Anchor", "#product0Anchor");
            Remove(factorElements);
        }

        [StateTransition<State>(State.ShowAdditionProblem, State.ShowMultiplicationProblem)]
        private void ToShowMultiplicationProblemReverse()
        {
            var quotientElements = QueryMultiple("#sum0", "#sum1", "#sumResult", "#sumResultLine", "#sumResultLineAnchor", "#sum1Anchor", "#sum0Anchor");
            Remove(quotientElements);
        }

        private void AddProblem(int problemIndex, string identifier, string firstTerm, string secondTerm, string result, string operatorSymbol)
        {
            var firstTermBlock = new TextBlock($"#{identifier}0")
            {
                Text = $"  {firstTerm}",
                FontSize = 48,
                Color = SColor.White,
                FontFamily = "Consolas"
            };
            var secondTermBlock = new TextBlock($"#{identifier}1")
            {
                Text = $"{operatorSymbol} {secondTerm}",
                FontSize = 48,
                Color = SColor.White,
                FontFamily = "Consolas"
            };

            var resultBlock = new TextBlock($"#{identifier}Result")
            {
                Text = $"  {result}",
                FontSize = 48,
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

            // Stack them vertically this time since the results are wider
            var problemCenterGap = Size.Height / 4d;
            var termLeft = TopCenter.Left(Size.Width / 4);

            var problemIndexCopy = problemIndex;
            while (problemIndexCopy > 0)
            {
                termLeft = termLeft.Down(problemCenterGap);
                problemIndexCopy--;
            }

            // CANIMPROVE: Wanting to make changes to an existing layout kind of sucks
            // because the primitives we've built up are in code and not the engine. For example, here
            // I want to make the margin between items wider. I can do it...
            if (problemIndex == 1)
            {
                termLeft = termLeft.Up(15d);
            }
            else if (problemIndex == 3)
            {
                termLeft = termLeft.Down(15d);
            }
            // ...but it's kind of ugly. I don't think this is unsolvable - ideally, "math problem"
            // becomes an element and we can have some kind of positioning "wrapper" that isn't really
            // an element wrapper but divides up a BasisLine into padding and then equally-spaced anchors.

            // Build a bounding box for the problem to left-align the elements within.
            var boundingBoxWidth = widestTextWidth;
            var boundingBoxHeight = firstTermSize.Height + secondTermSize.Height + resultSize.Height + 4d;

            var term0Position = SPointF.Zero;
            var term1Position = term0Position.Down(firstTermSize.Height + 2d);
            var resultLinePosition = term1Position.Down(secondTermSize.Height + 2d);
            var resultPosition = resultLinePosition.Down(4d);

            // Place the line on the center line
            var resultLineAnchor = new BasisPoint(termLeft, $"#{identifier}ResultLineAnchor");
            resultLine.AnchorLeftCenterTo(resultLineAnchor);

            // Place the first term in its relative position to the line
            var firstTermRelativePosition = term0Position - resultLinePosition;
            var firstTermAnchor = new BasisPoint(termLeft + firstTermRelativePosition, $"#{identifier}0Anchor");
            firstTermBlock.AnchorTopLeftTo(firstTermAnchor);

            // Then the second term in the same way
            var secondTermRelativePosition = term1Position - resultLinePosition;
            var secondTermAnchor = new BasisPoint(termLeft + secondTermRelativePosition, $"#{identifier}1Anchor");
            secondTermBlock.AnchorTopLeftTo(secondTermAnchor);

            // And the result as well
            var resultRelativePosition = resultPosition - resultLinePosition;
            var resultAnchor = new BasisPoint(termLeft + resultRelativePosition, $"#{identifier}ResultAnchor");
            resultBlock.AnchorTopLeftTo(resultAnchor);

            // Actually add the elements
            Add([firstTermBlock, secondTermBlock, resultLine, resultBlock, firstTermAnchor, secondTermAnchor, resultAnchor])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }
    }
}
