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
        private int _state = 0;

        public DelphinusSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = new SColor(8, 0, 130, 255);

            var baseExpr = Paren(AddExpr(Text("x", "#x"), Text("y", "#y")));
            var exp = Frac(Text("1"), Text("2"));
            var sub = Frac(AddExpr(Text("2"), Text("1")), Text("4"));
            //var exp = Text("2");
            //var sub = Text("3");
            var libraElement = new LibraElement(
                ExpSub(baseExpr, exp, sub),
                _baseFont, "#libra");
            var anchor = new BasisPoint(Center, "anchor");
            libraElement.AnchorCenterTo(anchor);
            Add([libraElement, anchor]);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_state == 0)
            {
                var libraElement = (LibraElement)Query("#libra").Single();
                //libraElement.TransformAnimate(root =>
                //    root.ReplaceFromRoot("#x",
                //        _ => Frac(Text("4"), Text("2"), "#x")), 0.75d);
                _state = 1;
            }
            return SlideAdvanceResult.InternalStateChanged;
        }
    }
}
