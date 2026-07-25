using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Libra.LibraExpressionFactory;

namespace Celarix.Starfall.Playground.DelphinusTests
{
    internal sealed class DelphinusSlide : AtriaSlide
    {
        private static readonly SFont _baseFont = new SFontFamily("Calibri", 60f);

        public DelphinusSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = new SColor(8, 0, 130, 255);

            var libraElement = new LibraElement(
                AddExpr(Frac(Text("4"), Text("2")), Text("y", "y")),
                _baseFont);
            var anchor = new BasisPoint(Center, "anchor");
            libraElement.AnchorCenterTo(anchor);
            Add([libraElement, anchor]);
        }
    }
}
