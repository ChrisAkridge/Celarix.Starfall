using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_07_5_ChoosingBitAllocation : AtriaSlide
    {
        private readonly FloatingPointFormatDesignElement _formatDesign;
        private int _state;

        public SlideFP_07_5_ChoosingBitAllocation(AtriaRuntime runtime, SSizeF size) : base(runtime, size)
        {
            _formatDesign = new FloatingPointFormatDesignElement("#formatDesign")
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
                case 0: ShowScientificNotationParts(); break;
                case 1: RemoveBase(); break;
                case 2: ReduceSignToOneBit(); break;
                case 3: ShowSignConvention(); break;
                case 4: ShowMemoryWidth(8); break;
                case 5: ShowMemoryWidth(16); break;
                case 6: ShowMemoryWidth(32); break;
                case 7: ShowMemoryWidth(64); break;
                case 8: NameTheFormats(); break;
                case 9: ShowSinglePrecisionLayout(); break;
                default: return SlideAdvanceResult.CanAdvance;
            }

            _state += 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private void ShowScientificNotationParts()
        {
            Console.WriteLine("FP7.5: Shows the four parts of a scientific-notation number as stored pieces.");
            var anchor = new BasisPoint(TopLeft, "#formatDesignAnchor");
            _formatDesign.AnchorTopLeftTo(anchor);
            Add([_formatDesign, anchor]);
            Animate(0.6d, p => _formatDesign.ScientificPartsOpacity = p);
        }

        private void RemoveBase()
        {
            Console.WriteLine("FP7.5: Removes the implicit base 2 and recenters the remaining parts.");
            Animate(0.7d, p => _formatDesign.BaseRemovalProgress = Easings.Land(p));
        }

        private void ReduceSignToOneBit()
        {
            Console.WriteLine("FP7.5: Reduces the sign to one bit with two possible values.");
            Animate(0.6d, p => _formatDesign.SignBitProgress = Easings.Land(p));
        }

        private void ShowSignConvention()
        {
            Console.WriteLine("FP7.5: Shows 0 for positive and 1 for negative.");
            Animate(0.5d, p => _formatDesign.SignConventionOpacity = p);
        }

        private void ShowMemoryWidth(int bitCount)
        {
            Console.WriteLine($"FP7.5: Shows the {bitCount}-bit memory denomination.");
            if (bitCount == 8)
            {
                Animate(0.6d, p =>
                {
                    _formatDesign.ScientificPartsOpacity = 1d - p;
                    _formatDesign.MemoryOptionsOpacity = p;
                    _formatDesign.EightBitOpacity = p;
                });
                return;
            }

            Animate(0.5d, p =>
            {
                switch (bitCount)
                {
                    case 16: _formatDesign.SixteenBitOpacity = p; break;
                    case 32: _formatDesign.ThirtyTwoBitOpacity = p; break;
                    case 64:
                        _formatDesign.SixtyFourBitOpacity = p;
                        _formatDesign.OneHundredTwentyEightBitOpacity = p;
                        break;
                }
            });
        }

        private void NameTheFormats()
        {
            Console.WriteLine("FP7.5: Names the IEEE formats and emphasizes single precision.");
            Animate(0.7d, p =>
            {
                _formatDesign.EightBitOpacity = 1d;
                _formatDesign.SixteenBitOpacity = 1d;
                _formatDesign.ThirtyTwoBitOpacity = 1d;
                _formatDesign.SixtyFourBitOpacity = 1d;
                _formatDesign.OneHundredTwentyEightBitOpacity = 1d;
                _formatDesign.FormatNamesOpacity = Easings.Land(p);
            });
        }

        private void ShowSinglePrecisionLayout()
        {
            Console.WriteLine("FP7.5: Expands single precision into four bytes and claims the leftmost sign bit.");
            Animate(0.8d, p =>
            {
                var eased = Easings.Land(p);
                _formatDesign.MemoryOptionsOpacity = 1d - eased;
                _formatDesign.SingleLayoutOpacity = eased;
            });
        }

        private void Animate(double seconds, Action<double> update)
        {
            Animations.ScheduleAnimation(Animations.StartNow(
                AnimationContext.SecondsToFrames(seconds), update));
        }
    }
}
