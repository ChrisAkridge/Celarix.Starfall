using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Presentations.FloatingPoint.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_05_ButWellJustPickBinary : AtriaSlide
    {
        public SlideFP_05_ButWellJustPickBinary(int width, int height) : base(width, height)
        {
            
        }

        public override void Initialize()
        {
            // Man, reasoning about coordinates is making my head hurt. Future me: layers, man. Anyway,
            // The FloatingPointWindowElement has a row of bits. It's 277 bits wide, so it's pretty wide.
            // We pick a font size, check the size of a "0" or "1" in that font, then use that as the bit's
            // height, from which we derive the rest of the sizing of a single bit. Then the row is just 277
            // times that width.
            //
            // We render the font at full size, though, so the actual OpenTk window is only viewing a
            // rectangular portion of the whole thing. We pan across it to show different bits. We do
            // so by dragging around a CenteredX property within it. But in order to discuss that, we
            // need to figure out coordinate spaces.
            //
            // We only care about horizontal coordinates here. By definition, the very center of the
            // binary point is at a row X of 0. The bits to the left of it are at negative Xs, and the
            // bits to the right of it are at positive Xs. We'll call the narrower rectangle the element
            // rectangle, as wide as the OpenTk window is. It has a left of (-ElementWidth/2) and a right
            // of (ElementWidth/2). When CenteredX == 0, we have the binary point in the center of the
            // element rectangle. Thus row and element X matches up perfectly.
            //
            // When CenteredX moves negative, the bits pan to the right and we see more left bits. This
            // is the same as if the entire row moved to the right by CenteredX, so the row's X is
            // actually (RowX - CenteredX), since CenteredX is negative and (pos - neg) = pos + abs(neg).
            //
            // When CenteredX moves positive, the bits pan to the left and we see more right bits. This
            // is the same as if the entire row moved to the left by CenteredX, so the row's X is
            // actually (RowX - CenteredX), since CenteredX is positive and (pos - pos) = 0.

            BackgroundColor = Constants.FloatingPointBackground;

            var tempElement = new FloatingPointWindowElement("#tempElement", MeasurementService)
            {
                Size = Size
            };
            var anchor = new BasisPoint(TopLeft, "#tempAnchor");
            tempElement.AnchorTopLeftTo(anchor);
            Add([anchor, tempElement]);
        }

        private Random _debugRandom = new Random();
        private int _lastSetBit = 0;
        public override SlideAdvanceResult Advance()
        {
            var fpWindow = Query("#tempElement").OfType<FloatingPointWindowElement>().First();

            // 1. Pick a random bit to center on. The range of bits is -149 to 127, inclusive, since the binary point is between bit -1 and bit 0.
            //var newBit = _debugRandom.Next(-149, 128);
            //fpWindow.ScrollBitToCenter(newBit);

            // 2. Set the next bit down.
            //fpWindow.SetBit(_lastSetBit, true, bounce: true);
            //_lastSetBit += 1;

            // 3. Show exponents.
            // fpWindow.SetShowExponents(show: true);

            // 4. Toggle exponents
            //fpWindow.SetShowExponents(_lastSetBit % 2 == 0);
            //_lastSetBit += 1;

            // 5. Toggle place values
            fpWindow.SetShowPlaceValues(_lastSetBit % 2 == 0);
            _lastSetBit += 1;

            return SlideAdvanceResult.InternalStateChanged;
        }
    }
}
