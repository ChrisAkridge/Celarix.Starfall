using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_07_6_ExponentAsUnsignedInteger : AtriaSlide
    {
        private readonly BinaryIntegerElement _binaryInteger;
        private readonly BinaryIntegerSummandStackElement _summandStack;
        private readonly BinaryIntegerValueHistoryElement _valueHistory;
        private int _state;

        public SlideFP_07_6_ExponentAsUnsignedInteger(AtriaRuntime runtime, SSizeF size)
            : base(runtime, size)
        {
            _binaryInteger = BinaryIntegerElement.CreateUnsigned8("#binaryInteger", MeasurementService);
            _binaryInteger.Size = size;
            _binaryInteger.BitPattern = 0;
            _binaryInteger.Opacity = 0d;
            _summandStack = new BinaryIntegerSummandStackElement(
                "#summandStack", _binaryInteger, MeasurementService)
            {
                Size = size,
                Opacity = 0d
            };
            _valueHistory = new BinaryIntegerValueHistoryElement("#valueHistory", _summandStack)
            {
                Size = size
            };
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
        }

        public override SlideAdvanceResult Advance()
        {
            switch (_state)
            {
                case 0: ShowBits(); break;
                case 1: ShowLabelsAndSummands(); break;
                case 2: SweepUnsignedRange(); break;
                case 3: PromoteStoredValues(); break;
                case 4: ShowZeroExponentProblem(); break;
                case 5: RestoreStoredValues(); break;
                case 6: ApplyExponentBias(); break;
                case 7: HighlightSpecialExponents(); break;
                default: return SlideAdvanceResult.CanAdvance;
            }

            _state += 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private void ShowBits()
        {
            Console.WriteLine("FP7.6: Fades in the eight exponent bits.");
            var anchor = new BasisPoint(TopLeft, "#binaryIntegerAnchor");
            _binaryInteger.AnchorTopLeftTo(anchor);
            _summandStack.AnchorTopLeftTo(anchor);
            _valueHistory.AnchorTopLeftTo(anchor);
            Add([_valueHistory, _binaryInteger, _summandStack, anchor]);
            Animate(0.5d, p => _binaryInteger.Opacity = p);
        }

        private void ShowLabelsAndSummands()
        {
            Console.WriteLine("FP7.6: Staggers in the place labels and fades in the summand stack.");
            const int labelAnimationFrames = 30;
            const int staggerFrames = 4;
            var totalFrames = labelAnimationFrames
                + staggerFrames * (_binaryInteger.BitPlaceExponents.Count - 1);

            Animations.ScheduleAnimation(Animations.StartNow(totalFrames, p =>
            {
                var currentFrame = p * totalFrames;
                for (var bitIndex = 0; bitIndex < _binaryInteger.BitPlaceExponents.Count; bitIndex++)
                {
                    var localProgress = Math.Clamp(
                        (currentFrame - bitIndex * staggerFrames) / labelAnimationFrames,
                        0d, 1d);
                    _binaryInteger.SetLabelRevealProgress(bitIndex, Easings.Land(localProgress));
                }

                _summandStack.Opacity = Easings.Land(p);
            }));
        }

        private void SweepUnsignedRange()
        {
            Console.WriteLine("FP7.6: Sweeps the stored unsigned exponent from 0 through 255.");
            var lastValue = 0;
            _valueHistory.Spawn(0);
            Animate(2.5d, p =>
            {
                var eased = Easings.Smoothstep(p);
                var currentValue = (int)Math.Round(eased * 255d);
                _binaryInteger.BitPattern = currentValue;

                for (var value = lastValue + 1; value <= currentValue; value++)
                {
                    _valueHistory.Spawn(value);
                }

                lastValue = currentValue;
            });
        }

        private void PromoteStoredValues()
        {
            Console.WriteLine("FP7.6: Removes the binary machinery and promotes all 256 stored values.");
            _valueHistory.PromoteAllValues(0.8d);
            Animate(0.6d, p =>
            {
                var opacity = 1d - Easings.Smoothstep(p);
                _binaryInteger.Opacity = opacity;
                _summandStack.Opacity = opacity;
            });
        }

        private void ShowZeroExponentProblem()
        {
            Console.WriteLine("FP7.6: Mutes the stored values and turns zero into the exponent in 2^0 = 1.");
            _valueHistory.ShowZeroAsExponent(1.1d);
        }

        private void RestoreStoredValues()
        {
            Console.WriteLine("FP7.6: Returns zero to the grid, hides the equation, and promotes the stored values.");
            _valueHistory.RestoreGrid(1.0d);
        }

        private void ApplyExponentBias()
        {
            Console.WriteLine("FP7.6: Subtracts the exponent bias of 127 from every stored value.");
            _valueHistory.ApplyBias(127, 2.5d);
        }

        private void HighlightSpecialExponents()
        {
            Console.WriteLine("FP7.6: Marks the lowest and highest decoded exponents as special cases.");
            _valueHistory.HighlightSpecialExponents(0.7d);
        }

        private void Animate(double seconds, Action<double> update)
        {
            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(seconds), update));
        }
    }
}
