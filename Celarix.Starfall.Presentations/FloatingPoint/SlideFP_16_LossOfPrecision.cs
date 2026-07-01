using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_16_LossOfPrecision : AtriaSlide
    {
        private PrecisionGridElement? _precisionGridElement;

        public SlideFP_16_LossOfPrecision(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;
        }

        public override SlideAdvanceResult Advance()
        {
            if (_precisionGridElement == null)
            {
                _precisionGridElement = new PrecisionGridElement("#precisionGrid")
                {
                    DotsOnRow = 50
                };
                var anchor = new BasisPoint(TopLeft, "#precisionGridAnchor");
                _precisionGridElement.AnchorTopLeftTo(anchor);
                Add([_precisionGridElement, anchor])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (!_precisionGridElement.AnimationRunning)
            {
                _precisionGridElement.AnimationRunning = true;
                return SlideAdvanceResult.InternalStateChanged;

            }
            else
            {
                return SlideAdvanceResult.CanAdvance;
            }
        }
    }
}
