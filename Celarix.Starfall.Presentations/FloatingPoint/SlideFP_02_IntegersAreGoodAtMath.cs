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

        [StateTransition<State>(State.Initial, State.ShowAdditionProblem)]
        private void ToShowAdditionProblem()
        {
            // CANIMPROVE: Make a StackMathProblemElement that can display problems like this
            // and handle the layout, presenting itself as a single box. Just use Skia rendering
            // directly, don't worry about wrapping Elements inside it. We can add stuff like
            // multiple terms, carries/borrows, step-by-step later.
            var addend0 = new TextBlock("#addend0")
            {
                Text = "  5",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };
            var addend1 = new TextBlock("#addend1")
            {
                Text = "+ 3",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };

            var sum = new TextBlock("#sum")
            {
                Text = "  8",
                FontSize = 64,
                Color = SColor.White,
                FontFamily = "Consolas"
            };

            var addend0Size = MeasurementService.MeasureText(addend0);
            var addend1Size = MeasurementService.MeasureText(addend1);
            var sumSize = MeasurementService.MeasureText(sum);

            var widestTextWidth = Math.Max(addend0Size.Width, Math.Max(addend1Size.Width, sumSize.Width));
            var sumLine = new LineElement("#sumLine")
            {
                StrokeWidth = 4d,
                StrokeColor = SColor.White,
                // From point is implicitly (0, 0) and we let anchoring handle the positioning
                ToPoint = SPointF.Zero.Right(widestTextWidth)
            };

            var problemCenterGap = Size.Width / 3d;
            var addendCenter = LeftCenter.Right(problemCenterGap);

            // Place the line on the center line
            var sumLineAnchor = new BasisPoint(addendCenter, "#sumLineAnchor");
            sumLine.AnchorCenterTo(sumLineAnchor);

            // Place addend1 directly above the line with some margin
            var margin = 4d;
            double addend1DistanceUp = (addend1Size.Height) / 2d + margin;
            var addend1Anchor = new BasisPoint(addendCenter.Up(addend1DistanceUp), "#addend1Anchor");
            addend1.AnchorCenterTo(addend1Anchor);

            // Place addend0 directly above addend1 with some margin
            double addend0DistanceUp = (addend1Size.Height) + (addend0Size.Height / 2d) + (margin * 2d);
            var addend0Anchor = new BasisPoint(addendCenter.Up(addend0DistanceUp), "#addend0Anchor");
            addend0.AnchorCenterTo(addend0Anchor);

            // Place sum directly below the line with some margin
            double sumDistanceDown = (sumSize.Height / 2d) + margin;
            var sumAnchor = new BasisPoint(addendCenter.Down(sumDistanceDown), "#sumAnchor");
            sum.AnchorCenterTo(sumAnchor);

            // Actually add the elements
            Add([addend0, addend1, sumLine, sum, addend0Anchor, addend1Anchor, sumAnchor])
                .AnimateBasic(0.35d, AnimationTypes.FadeIn, Easings.Linear);
        }
    }
}
