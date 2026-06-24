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
        private int _state;
        private FloatingPointWindowElement _element;
        private SingleBinaryViewElement _binaryView;
        private readonly AnimationContext _animationContext = new();

        private bool _isNegative;
        private int _exponent = 0;
        private BitArray _currentMantissa = new(24);
        private BitArray _targetMantissa = new(23);
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

                var intValue = signBit | exponentBits | (mantissaBits & 0x007F_FFFF);
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
                var superscriptExponent = _exponent.ToString().Select(FPHelpers.ToUnicodeSuperscript); // Exponent in superscript
                string currentMantissaString = GetMantissaBinaryString(_currentMantissa).Substring(1); // Remove the implied leading bit
                var impliedLeadingBit = ((_exponent != -127) ? "1" : "0");
                return $"{(_isNegative ? "-" : "")}{impliedLeadingBit}.{currentMantissaString} × 2{string.Concat(superscriptExponent)}";
            }
        }

        private string CurrentValueString => Helpers.ExactStringSingle(CurrentValue);
        private string TargetValueString => Helpers.ExactStringSingle(TargetValue);
        private string DifferenceString => Helpers.ExactStringSingle(TargetValue - CurrentValue);
        private string PlaceValueString
        {
            get
            {
                var currentExponent = _exponent - _nextSetBit;
                var exponentSuperscript = currentExponent.ToString().Select(FPHelpers.ToUnicodeSuperscript); // Exponent in superscript
                var exponentPart = $"2{string.Concat(exponentSuperscript)}";
                var placeValuePart = Helpers.ExactStringSingle((float)Math.Pow(2, currentExponent));
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
            _binaryView = new SingleBinaryViewElement("#binaryView", "Consolas")
            {
                Size = new SSizeF(Size.Width, Size.Height / 4d),
                BaseFontSize = 48
            };

            _element.SetArrowBit(0);
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
                case 0: return FadeInBitRow();
                case 1: return ZoomInAndShowExponentsAndPlaceValues();
                case 2: return ZoomOutAndHideExponentsAndPlaceValues();
                case 3: return ShowWindow();
                case 4: return ShowValues();
                case 5: return ShowArrow();
                case 6: return SetNextMantissaBit();
                case 7: return AskTheAudience();
                case 8: return SlideAdvanceResult.CanAdvance;
                default: throw new InvalidOperationException("Unreachable.");
            }
        }

        private SlideAdvanceResult FadeInBitRow()
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
            var windowAnimation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
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
            var valuesAnchor = new BasisPoint(BottomLeft.Down(25d), "#valuesAnchor");
            valuesBlock.AnchorBottomLeftTo(valuesAnchor);

            var viewAnchor = new BasisPoint(_element.Bounds.BottomLeft, "#viewAnchor");
            _binaryView.AnchorTopLeftTo(viewAnchor);
            _binaryView.Size = new SSizeF(Size.Width, Size.Height / 4d);

            Add([valuesBlock, valuesAnchor, _binaryView, viewAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
            _state = 5;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowArrow()
        {
            var arrowAnimation = FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
            {
                _element.ArrowOpacity = p;
            }, () => _state = 6);
            _animationContext.ScheduleAnimation(arrowAnimation);

            SetTarget((float)Math.PI);

            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult SetNextMantissaBit()
        {
            if (_nextSetBit >= 24)
            {
                _state = 7;
                _nextSetBit = 0;
                return SlideAdvanceResult.InternalStateChanged;
            }

            var bitIndex = _nextSetBit;
            bool nextTargetBit;

            if (bitIndex == 0)
            {
                // Implied leading bit.
                var isNormal = _exponent != -127 && _exponent != 128;
                nextTargetBit = isNormal;
            }
            else
            {
                nextTargetBit = _targetMantissa[bitIndex - 1];
            }

            _currentMantissa[bitIndex] = nextTargetBit;
            _nextSetBit += 1;

            _element.SetBitAndAdvanceArrowAndScroll(_exponent - bitIndex, nextTargetBit);
            UpdateVisibleValues();
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult AskTheAudience()
        {
            float? newTarget = null;
            do
            {
                Console.Write("Enter a new target value (or 'exit' to finish): ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.ToLowerInvariant().Trim() == "exit")
                {
                    _state = 8;
                    return SlideAdvanceResult.InternalStateChanged;
                }

                if (float.TryParse(input, out var parsedValue))
                {
                    newTarget = parsedValue;
                    _nextSetBit = 0;
                    _element.ClearAllSetBits(bounce: true);
                    _currentMantissa.SetAll(false);
                    SetTarget(newTarget.Value);
                    _state = 6;
                    
                    return SlideAdvanceResult.InternalStateChanged;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid floating-point number or 'exit'.");
                }
            } while (newTarget == null);

            throw new InvalidOperationException("Unreachable code reached in AskTheAudience.");
        }

        // Helpers
        private int GetMantissaValue(BitArray mantissa)
        {
            var highestShift = mantissa.Length - 1;
            var value = 0;
            for (var i = 0; i < mantissa.Length; i++)
            {
                if (mantissa[i])
                {
                    value |= 1 << (highestShift - i);
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

        private void UpdateVisibleValues()
        {
            var valuesBlock = (MultilineTextBlock)Query("#values").Single();
            valuesBlock.Text = VisibleValues;

            var sign = _isNegative ? unchecked((int)0x8000_0000) : 0;
            var biasedExponent = (_exponent + 127) & 0xFF;
            var exponentBits = biasedExponent << 23;
            var currentMantissa = GetMantissaValue(_currentMantissa);
            var intValue = sign | exponentBits | (currentMantissa & 0x007F_FFFF);
            _binaryView.Value = BitConverter.Int32BitsToSingle(intValue);
        }

        private void SetTarget(float target)
        {
            var intValue = BitConverter.SingleToInt32Bits(target);
            _isNegative = (intValue & 0x8000_0000) != 0;
            _element.SetShowNegativeFlag(_isNegative);

            var oldExponent = _exponent;
            var biasedExponent = (intValue >> 23) & 0xFF;
            _exponent = biasedExponent - 127;
            if (_exponent != oldExponent)
            {
                _element.MoveWindowToExponent(_exponent);
                _element.SetArrowBit(_exponent);
                _element.ScrollBitToCenter(_exponent);
            }

            var mantissaBits = intValue & 0x007F_FFFF;

            for (var i = 0; i < 23; i++)
            {
                _targetMantissa[i] = (mantissaBits & (1 << (22 - i))) != 0;
            }

            UpdateVisibleValues();
        }
    }
}
