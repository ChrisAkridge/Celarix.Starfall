using Celarix.Starfall.Audio;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_14_15_SpecialExponents : AtriaSlide
    {
        private const double CursedNaNStaticOpacity = 0.03d;
        private static readonly SoundPlayer _soundPlayer = new();

        // States:
        // 0. Initial state, showing the floating point window and binary view showing the value 1.0 (exponent = 0, mantissa = 0).
        // 1. Clear the 2^0 bit in the row but not the mantissa. Slide all the way to 2^-126, leave all row bits clear,
        //    but leave the mantissa bits alone. The value is 2^-126.
        // 2. "so, because of the implied leading bit, we write a 1 to this bit..." (2^-126 is set to 1)
        // 3. "now let's try to clear it so we can get to zero..." (bit bounces but does not change)
        // 4. "um..." (bit bounces a second time)
        // 5. (and a third)
        // 6. (and, finally, a fourth) "so... it turns out we can't write zero at all! Whoops."
        // 7. Explain how there's a special case called subnormals for the smallest possible exponent.
        //    Clear the 2^-126 bit in the row and the mantissa. The value is now 0.0.
        // 8. Start a continuing animation that adds 1 to everything. This is the last state for discussing
        //    subnormals.
        // 9. Slide the window all the way to the left and hide the current value text.
        private int _state;
        private FloatingPointWindowElement _element;
        private SingleBinaryViewElement _binaryView;
        private readonly AnimationContext _animationContext = new();
        private readonly SImage _cursedNaNStatic;

        private bool _isNegative;
        private int _exponent = 0;
        private BitArray _currentMantissa = new(24);
        private BitArray _targetMantissa = new(23);
        private bool _isAddingToMantissa = true;
        private int _addingMantissa = 0;
        private List<string> _currentValueSplitBuffer = new List<string>();
        private bool _drawCursedNaNStatic = false;

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
                if ((mantissaBits & 0x0080_0000) == 0 && biasedExponent != 0 && biasedExponent != 255)
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

        private string VisibleValues
        {
            get
            {
                if (float.IsInfinity(CurrentValue))
                {
                    return "• Current value: Infinity\n• Results from overflows and operations like 1/0";
                }
                else if (float.IsNaN(CurrentValue))
                {
                    return "• Current value: NaN\n• Results from invalid operations like 0/0\n• Not equal to any value, including itself, even with the same bits\n• Spreads when used in calculations";
                }

                _currentValueSplitBuffer.Clear();
                var currentValueLine = $"• Current value: {CurrentValueString}";
                if (currentValueLine.Length < 100)
                {
                    _currentValueSplitBuffer.Add(currentValueLine);
                }
                else
                {
                    for (var i = 0; i < currentValueLine.Length; i += 100)
                    {
                        var length = Math.Min(100, currentValueLine.Length - i);
                        _currentValueSplitBuffer.Add(currentValueLine.Substring(i, length));
                    }
                }

                var sb = new StringBuilder();
                
                foreach (var line in _currentValueSplitBuffer)
                {
                    sb.AppendLine(line);
                }

                sb.AppendLine($"• Scientific notation: {ScientificNotationString}");
                return sb.ToString();
            }
        }

        static SlideFP_14_15_SpecialExponents()
        {
            _soundPlayer.LoadSound("glassBreaking", "Assets/Sounds/159197__justbrando__glass-breaking.wav");
        }

        public SlideFP_14_15_SpecialExponents(int width, int height) : base(width, height)
        {
            _cursedNaNStatic = SImage.FromSKImage(SKImage.FromEncodedData("Assets/Images/3317968666_f46dbaac72_o_cropped.jpg"));
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
            _element = new FloatingPointWindowElement("#floatingPointWindow", MeasurementService)
            {
                Size = new SSizeF(Size.Width, Size.Height / 4d),
                WindowOpacity = 1d,
                ArrowOpacity = 1d,
                BaseFontSize = 40
            };
            var elementAnchor = new BasisPoint(TopLeft.Down(Size.Height / 6d), "#floatingPointWindowAnchor");
            _element.AnchorTopLeftTo(elementAnchor);
            _element.FallingWindowRectExited += FloatingPointWindowElement_FallingWindowRectExited;

            _binaryView = new SingleBinaryViewElement("#binaryView", "Consolas")
            {
                Size = new SSizeF(Size.Width, Size.Height / 4d),
                BaseFontSize = 48
            };
            var viewAnchor = new BasisPoint(_element.Bounds.BottomLeft, "#viewAnchor");
            _binaryView.AnchorTopLeftTo(viewAnchor);
            _binaryView.Size = new SSizeF(Size.Width, Size.Height / 4d);
            _binaryView.Value = 1.0f;

            // Start with the implied leading bit set to 1, since we are not at the minimum exponent yet.
            _element.SetBit(0, true, bounce: false);
            _currentMantissa.Set(0, true);

            var valuesBlock = new MultilineTextBlock("#values")
            {
                Text = VisibleValues,
                Font = new SFontFamily("Consolas", 36f),
                Color = SColor.White
            };
            var valuesAnchor = new BasisPoint(BottomLeft.Up(50d), "#valuesAnchor");
            valuesBlock.AnchorBottomLeftTo(valuesAnchor);

            Add([_element, elementAnchor, _binaryView, viewAnchor, valuesBlock, valuesAnchor])
                .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);

            _element.SetArrowBit(0);
            _element.MoveWindowToExponent(0);
            _element.ScrollBitToCenter(0);
        }

        private void FloatingPointWindowElement_FallingWindowRectExited(object? sender, EventArgs e)
        {
            UpdateVisibleValues();
            var valuesBlock = (MultilineTextBlock)Query("#values").Single();
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartIn(AnimationContext.SecondsToFrames(2d),
                AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    valuesBlock.Opacity = p;
                }));
            _soundPlayer.PlaySound("glassBreaking");
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        public override void Render(IRenderTarget target)
        {
            if (_drawCursedNaNStatic)
            {
                // Draw the background ourselves so we can show the static atop it.
                target.Clear(Constants.FloatingPointBackground);

                var imagePath = "Assets/Images/3317968666_f46dbaac72_o_cropped.jpg";
                var fullPath = System.IO.Path.GetFullPath(imagePath);

                target.DrawImageFromFile(imagePath,
                    new SRectF(0, 0, Size.Width, Size.Height),
                    CursedNaNStaticOpacity,
                    SAngle.Zero);

                foreach (var element in Elements)
                {
                    element.Render(target);
                }
            }
            else
            {
                base.Render(target);
            }
        }

        public override SlideAdvanceResult Advance()
        {
            switch (_state)
            {
                case 0: return GoToMinimumExponent();
                case 1: return WriteOneBitToTwoToThe126th();
                case 2: return TryToClearThatBit();
                case 3: return TryAgain();
                case 4: return TryAThirdTime();
                case 5: return TryAFourthTime();
                case 6: return TryAndSucceed();
                case 7: return BeginAdding();
                case 8: return GoToTheMaximumExponent();
                case 9: return ComedicallyDropWindow();
                case 10: return ShowCorruptedNaNWindow();
                case 11: return SlideAdvanceResult.CanAdvance;
                default: throw new InvalidOperationException("Unreachable.");
            }
        }

        private SlideAdvanceResult GoToMinimumExponent()
        {
            _exponent = -126;
            _element.SetBit(0, false, bounce: true);
            _element.MoveWindowToExponent(_exponent);
            _element.SetArrowBit(_exponent);
            _element.ScrollBitToCenter(_exponent);
            UpdateVisibleValues();
            _state = 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult WriteOneBitToTwoToThe126th()
        {
            _element.SetBit(-126, true, bounce: true);
            UpdateVisibleValues();
            _state = 2;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult TryToClearThatBit()
        {
            _element.SetBit(-126, true, bounce: true);
            _state = 3;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult TryAgain()
        {
            _element.SetBit(-126, true, bounce: true);
            _state = 4;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult TryAThirdTime()
        {
            _element.SetBit(-126, true, bounce: true);
            _state = 5;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult TryAFourthTime()
        {
            _element.SetBit(-126, true, bounce: true);
            _state = 6;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult TryAndSucceed()
        {
            _element.SetBit(-126, false, bounce: true);
            _exponent = -127;
            _currentMantissa.Set(0, false);
            UpdateVisibleValues();
            _state = 7;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult BeginAdding()
        {
            var valuesBlock = (MultilineTextBlock)Query("#values").Single();
            valuesBlock.Font = new SFontFamily("Consolas", 20f);

            _animationContext.ScheduleContinuingAnimation(ContinuingAnimation.StartNow(() =>
            {
                _addingMantissa += 1;
                SetMantissaValue(_currentMantissa, _addingMantissa);

                for (var i = 0; i < _currentMantissa.Length; i++)
                {
                    _element.SetBit(-126 - i, _currentMantissa[i], bounce: false);
                }

                UpdateVisibleValues();
                return _isAddingToMantissa;
            }));
            _state = 8;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult GoToTheMaximumExponent()
        {
            _isAddingToMantissa = false;

            var valuesBlock = (MultilineTextBlock)Query("#values").Single();
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d),
                p =>
                {
                    valuesBlock.Opacity = 1d - p;
                },
                () =>
                {
                    _exponent = 128;
                    _element.MoveWindowToExponent(127);
                    _element.SetArrowBit(127);
                    _element.ScrollBitToCenter(127);
                    SetMantissaValue(_currentMantissa, 0);
                    _element.ClearAllSetBits(bounce: false);
                    _binaryView.Value = float.PositiveInfinity;
                }));

            _state = 9;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ComedicallyDropWindow()
        {
            _element.ComedicallyDropWindow();
            _state = 10;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowCorruptedNaNWindow()
        {
            var random = new Random();
            var valuesBlock = (MultilineTextBlock)Query("#values").Single();
            valuesBlock.Opacity = 0d;

            // Start by setting a few bits in the row after I said "you can't set bits up here" when
            // the window fell off and shattered.
            var bitsLeftToSet = 9;
            _animationContext.ScheduleContinuingAnimation(ContinuingAnimation.StartNow(() =>
            {
                if (random.NextDouble() < (0.025d * (10 - bitsLeftToSet))) // Make it more likely to set a bit as time goes on
                {
                    var exponent = random.Next(103, 127);
                    _element.SetBit(exponent, true, bounce: false);
                    _currentMantissa.Set(127 - exponent, true);
                    _binaryView.Value = CurrentValue;
                    UpdateVisibleValues();
                    bitsLeftToSet -= 1;
                }

                bool shouldContinue = bitsLeftToSet > 0;
                if (!shouldContinue)
                {
                    ShowFaintStaticAndCorruptedNaNWindow(random);
                }
                return shouldContinue;
            }));

            return SlideAdvanceResult.InternalStateChanged;
        }

        private void ShowFaintStaticAndCorruptedNaNWindow(Random random)
        {
            _drawCursedNaNStatic = true;
            BackgroundColor = SColor.Transparent;
            var flickeringEasing = new Easing(p => random.NextDouble() < 0.3d ? 1d : 0d);

            _element.WindowColor = new SColor(255, 0, 0, 127);
            _element.WindowWidthInBits = 23;
            _element.MoveWindowToExponent(127);

            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d),
                p =>
                {
                    _element.WindowOpacity = flickeringEasing(p);
                }, () =>
                {
                    _element.WindowOpacity = 1d;
                    _element.DoWindowJitter = true;
                    var valuesBlock = (MultilineTextBlock)Query("#values").Single();
                    valuesBlock.Opacity = 1d;
                    _state = 11;
                }));
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

        private void SetMantissaValue(BitArray mantissa, int value)
        {
            var highestShift = mantissa.Length - 1;
            for (var i = 0; i < mantissa.Length; i++)
            {
                var bitValue = (value >> (highestShift - i)) & 1;
                mantissa.Set(i, bitValue == 1);
            }
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
    }
}
