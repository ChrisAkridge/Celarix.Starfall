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
    internal sealed class SlideFP_08_FloatingPointIsScientificNotation : AtriaSlide
    {
        private enum State
        {
            Initial,
            ShowScientificNotation,
            ShowDecimalValue,
            MantissaSweep,
            ExponentSweep
        }
        
        private const int MinMantissa = 0;
        private const int MaxMantissa = 8388608; // 2^23 - 1
        private const int MinExponent = -126;
        private const int MaxExponent = 127;

        private bool _sign;
        private int _mantissa;
        private int _exponent;

        private StateMachine<State> _stateMachine;
        private AnimationContext _animationContext;

        private float Value
        {
            get
            {
                // Build the float value into the raw bits and then convert it to a float
                int sign = _sign ? unchecked((int)0x8000_0000) : 0; // Sign bit
                int biasedExponent = _exponent + 127; // Bias the exponent
                int exponentBits = (biasedExponent & 0xFF) << 23; // Exponent bits
                int mantissaBits = _mantissa & 0x7FFFFF; // Mantissa bits (23 bits)
                int rawBits = sign | exponentBits | mantissaBits; // Combine all bits
                return BitConverter.Int32BitsToSingle(rawBits); // Convert raw bits to float
            }
        }

        private string ScientificNotation
        {
            get
            {
                var signPart = _sign ? "-" : "";
                var impliedLeadingBit = _exponent == MinExponent ? "0" : "1"; // Denormalized numbers have an implied leading 0
                var mantissaBits = Convert.ToString(_mantissa, 2).PadLeft(23, '0'); // Mantissa in binary
                var superscriptExponent = _exponent.ToString().Select(c => FPHelpers.ToUnicodeSuperscript(c)); // Exponent in superscript
                return $"{signPart}{impliedLeadingBit}.{mantissaBits} × 2{string.Concat(superscriptExponent)}";
            }
        }

        public SlideFP_08_FloatingPointIsScientificNotation(int width, int height) : base(width, height)
        {
            _stateMachine = new StateMachine<State>(this, State.Initial);
            _animationContext = new AnimationContext();
            _sign = false;
            _mantissa = 0;
            _exponent = 0;
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
            if (_stateMachine.CurrentState == State.ExponentSweep)
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

        [StateTransition<State>(State.Initial, State.ShowScientificNotation)]
        private void ShowScientificNotation()
        {
            var scientificNotation = new TextBlock("#scientificNotation")
            {
                Text = ScientificNotation,
                FontFamily = "Consolas",
                FontSize = 48f,
                Color = SColor.White
            };
            var scientificNotationAnchor = new BasisPoint(Center, "#scientificNotationAnchor");
            scientificNotation.AnchorCenterTo(scientificNotationAnchor);

            Add([scientificNotationAnchor, scientificNotation])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowScientificNotation, State.ShowDecimalValue)]
        private void ShowDecimalValue()
        {
            var scientificNotationAnchor = (BasisPoint)QueryBasis("#scientificNotationAnchor").Single();
            var scientificNotationInitialPosition = scientificNotationAnchor.Point;
            var scientificNotationTargetPosition = TopCenter.Down(Size.Height / 3f);
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                scientificNotationAnchor.Point = MathHelpers.Ease(scientificNotationInitialPosition, scientificNotationTargetPosition, p, Easings.Land);
            }));

            var decimalValue = new TextBlock("#decimalValue")
            {
                Text = Value.ToString("R"), // "R" format specifier gives the full precision of the float
                FontFamily = "Consolas",
                FontSize = 48f,
                Color = SColor.White
            };
            var decimalValueAnchor = new BasisPoint(BottomCenter.Up(Size.Height / 3f), "#decimalValueAnchor");
            decimalValue.AnchorCenterTo(decimalValueAnchor);

            Add([decimalValue, decimalValueAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
        }

        [StateTransition<State>(State.ShowDecimalValue, State.MantissaSweep)]
        private void MantissaSweep()
        {
            var scientificNotationElement = (TextBlock)Query("#scientificNotation").Single();
            var decimalValueElement = (TextBlock)Query("#decimalValue").Single();

            // We start at 0, sweep to the max mantissa, then to the maximum negative mantissa, and back to 0.
            // Are you thinking what I'm thinking? That's right, a sine wave!
            Func<double, int> sweepFunction = p =>
            {
                // p is in range 0 to 1, we want it from 0 to 2 * pi.
                var angle = p * 2 * Math.PI;
                var sine = Math.Sin(angle);
                return (int)(sine * MaxMantissa);
            };

            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(4d), p =>
            {
                var mantissa = sweepFunction(p);
                if (mantissa < 0)
                {
                    _sign = true;
                    _mantissa = -mantissa;
                }
                else
                {
                    _sign = false;
                    _mantissa = mantissa;
                }

                scientificNotationElement.Text = ScientificNotation;
                decimalValueElement.SetTextAndKeepFontSize(MeasurementService, Value.ToString("R"));
            }));
        }

        [StateTransition<State>(State.MantissaSweep, State.ExponentSweep)]
        private void ExponentSweep()
        {
            var scientificNotationElement = (TextBlock)Query("#scientificNotation").Single();
            var decimalValueElement = (TextBlock)Query("#decimalValue").Single();
            var exponentRange = MaxExponent - MinExponent;

            // We start at 0, sweep to the max exponent, then to the minimum exponent, and back to 0.
            Func<double, int> sweepFunction = p =>
            {
                // p is in range 0 to 1, we want it from 0 to 2 * pi.
                var angle = p * 2 * Math.PI;
                var sine = Math.Sin(angle);
                var exponentOffset = (int)(sine * exponentRange);
                return (exponentOffset + MinExponent + MaxExponent) / 2; // Center the sweep around the midpoint of the exponent range
            };
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(4d), p =>
            {
                _exponent = sweepFunction(p);
                scientificNotationElement.Text = ScientificNotation;
                decimalValueElement.SetTextAndKeepFontSize(MeasurementService, Value.ToString("R"));
            }));
        }
    }
}
