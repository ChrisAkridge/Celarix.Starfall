using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_09_10_11_OpenTheWindow : AtriaSlide
    {
        // Slide flow:
        // 0. Fade in a FloatingPointWindowElement with the 24-bit window not shown, centered again on bit 0.
        // 1. Zoom in (make the font size bigger) and show place values and exponents.
        // 2. Hide the place values and exponents and zoom out to the original size.
        // 3. Show the 24-bit window at exponent = 0, such that it is all visible.
        // 4. Stagger loading values below the element, showing things like current value, target value, and so forth.
        // 5. Fade the arrow in on bit 0.
        // 6. Set the current bit of the window to approach the target value. If we're not on the last bit, go to step 6, otherwise go to step 7.
        // 7. The console asks me for a number provided by an audience member. I type it in and press Enter.
        //    All 1 bits are set to 0 with a bounce, and the window, arrow, and centered bit all scroll to where the user's number is.
        //    We also show the negative flag if we need to.
        // 8. Set the current bit of the window to approach the target value. If we're not on the last bit, go to step 8, otherwise go to step 9.
        // 9. Ask me if there's another number someone wants to see. If so, go back to step 7, otherwise go to step 10.
        // 10. Final state.
        private int _state;
        private FloatingPointWindowElement _element;
        private readonly AnimationContext _animationContext = new();

        private bool _isNegative;
        private int _exponent = 0;
        private BitArray _currentMantissa = new(24);
        private BitArray _targetMantissa = new(24);
        private int _nextSetBit = 0;

        private float CurrentValue
        {
            get
            {
                var signBit = _isNegative ? unchecked((int)0x8000_0000) : 0;
                var biasedExponent = _exponent + 127;
                var exponentBits = (biasedExponent & 0xFF) << 23;
                var mantissaBits = GetMantissaValue(_currentMantissa);

                // Cheat a little bit - the window shows 24 bits even though we only store 23 bits in
                // the actual float. So if we have the top bit clear even though our exponent isn't
                // at the minimum, we need to pretend that is is.
                if ((mantissaBits & 0x0080_0000) == 0 && biasedExponent != 0)
                {
                    exponentBits = 0;
                }

            var intValue = signBit | exponentBits | mantissaBits;
                return BitConverter.Int32BitsToSingle(intValue);
            }
        }

        private float TargetValue
        {
            get
            {
                var signBit = _isNegative ? unchecked((int)0x8000_0000) : 0;
                var biasedExponent = _exponent + 127;
                var exponentBits = (biasedExponent & 0xFF) << 23;
                var mantissaBits = GetMantissaValue(_targetMantissa);
                var intValue = signBit | exponentBits | mantissaBits;
                return BitConverter.Int32BitsToSingle(intValue);
            }
        }

        // Visible values
        private string ScientificNotationString
        {
            get
            {
                var superscriptExponent = _exponent.ToString().Select(c => FPHelpers.ToUnicodeSuperscript(c)); // Exponent in superscript
                return $"{(_isNegative ? "-" : "")}1.{GetMantissaBinaryString(_currentMantissa)} × 2{string.Concat(superscriptExponent)}";
            }
        }

        private string CurrentValueString => CurrentValue.ToString("R");
        private string TargetValueString => TargetValue.ToString("R");
        private string DifferenceString => (TargetValue - CurrentValue).ToString("R");
        private string PlaceValueString
        {
            get
            {
                var exponentSuperscript = _element.ArrowExponent.ToString().Select(c => FPHelpers.ToUnicodeSuperscript(c)); // Exponent in superscript
                var exponentPart = $"2{string.Concat(exponentSuperscript)}";
                var placeValuePart = Helpers.ExactStringSingle((float)Math.Pow(2, _element.ArrowExponent));
                return $"{exponentPart} = {placeValuePart}";
            }
        }

        private string VisibleValues
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"• Current value: {CurrentValueString}");
                sb.AppendLine($"• Target value: {TargetValueString}");
                sb.AppendLine($"• Difference: {DifferenceString}");
                sb.AppendLine($"• Scientific notation: {ScientificNotationString}");
                sb.AppendLine($"• Place value of current bit: {PlaceValueString}");
                return sb.ToString();
            }
        }

        public SlideFP_09_10_11_OpenTheWindow(int width, int height) : base(width, height)
        {
            
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _element = new FloatingPointWindowElement("#floatingPointWindow", MeasurementService)
            {
                Size = new SSizeF(Size.Width, Size.Height / 4d),
                WindowOpacity = 0d,
                ArrowOpacity = 0d,
                BaseFontSize = 48
            };
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override SlideAdvanceResult Advance()
        {
            switch (_state)
            {
                case 0: return FadeWindow();
                case 1: return ZoomInAndShowExponentsAndPlaceValues();
                case 2: return ZoomOutAndHideExponentsAndPlaceValues();
                case 3: return ShowWindow();
                case 4: return ShowValues();
                default: throw new InvalidOperationException("Unreachable.");
            }
        }

        private SlideAdvanceResult FadeWindow()
        {
            var elementAnchor = new BasisPoint(TopLeft.Down(Size.Height / 6d), "#floatingPointWindowAnchor");
            _element.AnchorTopLeftTo(elementAnchor);
            Add([_element, elementAnchor])
                .AnimateBasic(2d, AnimationTypes.FadeIn, Easings.Linear);
            _state = 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ZoomInAndShowExponentsAndPlaceValues()
        {
            var fontSizeAnimation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(2d), p =>
            {
                _element.BaseFontSize = (float)MathHelpers.Ease(48d, 120d, p, Easings.Land);
            }, () =>
            {
                _element.SetShowExponents(show: true);
                _element.SetShowPlaceValues(show: true);
                _state = 2;
            });
            _animationContext.ScheduleAnimation(fontSizeAnimation);
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ZoomOutAndHideExponentsAndPlaceValues()
        {
            var fontSizeAnimation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(2d), p =>
            {
                _element.BaseFontSize = (float)MathHelpers.Ease(120d, 48d, p, Easings.Land);
            }, () =>
            {
                _element.SetShowExponents(show: false);
                _element.SetShowPlaceValues(show: false);
                _element.MoveWindowToExponent(0);
                _state = 3;
            });
            _animationContext.ScheduleAnimation(fontSizeAnimation);
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowWindow()
        {
            var windowAnimation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(2d), p =>
            {
                _element.WindowOpacity = p;
            }, () => _state = 4);
            _animationContext.ScheduleAnimation(windowAnimation);
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowValues()
        {
            var valuesBlock = new MultilineTextBlock("#values")
            {
                Text = VisibleValues,
                Font = new SFontFamily("Consolas", 36f),
                Color = SColor.White
            };
            var valuesAnchor = new BasisPoint(BottomLeft.Up(20d), "#valuesAnchor");
            valuesBlock.AnchorBottomLeftTo(valuesAnchor);

            Add([valuesBlock, valuesAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
            _state = 5;
            return SlideAdvanceResult.InternalStateChanged;
        }

        // Helpers
        private int GetMantissaValue(BitArray mantissa)
        {
            var value = 0;
            for (var i = 0; i < mantissa.Length; i++)
            {
                if (mantissa[i])
                {
                    value |= 1 << (23 - i);
                }
            }
            return value;
        }

        private string GetMantissaBinaryString(BitArray mantissa)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < mantissa.Length; i++)
            {
                sb.Append(mantissa[i] ? '1' : '0');
            }
            return sb.ToString();
        }
    }
}
