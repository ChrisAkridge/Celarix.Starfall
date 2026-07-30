using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Libra.LibraExpressionFactory;

namespace Celarix.Starfall.Playground.DelphinusTests
{
    internal sealed class DelphinusSlide : AtriaSlide
    {
        private static readonly SColor _backgroundColor = new SColor(8, 0, 130, 255);
        private static readonly SFont _baseFont = new SFontFamily("Cambria Math", 60f);
        private int _state = 0;

        public DelphinusSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            //BackgroundColor = _backgroundColor;

            //var _0 = AddExpr(Text("x"), Text("1"));
            //var _1 = Concat(Text("x"), Paren(_0));
            //var _2 = Frac(_1, Text("2"));
            //var _3 = Concat(Text("sumtorial"), Paren(Text("x")));
            //var _4 = EqualByDef(_3, _2);

            //var libraElement = new LibraElement(
            //    _4,
            //    _baseFont, "#libra");
            //var anchor = new BasisPoint(Center, "anchor");
            //libraElement.AnchorCenterTo(anchor);
            //Add([libraElement, anchor]);
        }

        public override void Render(IRenderTarget target)
        {
            target.Clear(_backgroundColor);

            var libraPath = new LibraPathBuilder()
                .QuadraticTo(50, 0, 100, 50)
                .LineTo(100, 100)
                .LineTo(0, 100)
                .LineTo(0, 0)
                .ClosePath();
            var libraStyle = new PathStyle(SColor.Green, null, 2d, SStrokeCap.Butt, SStrokeJoin.Miter);
            var renderable = new LibraPathRenderable(new LibraRenderableKey(Guid.NewGuid(), "test"), libraPath, libraStyle);

            renderable.RenderAt(target, new SPointF(100, 100), 1d);
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
