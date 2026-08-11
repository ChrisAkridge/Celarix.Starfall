using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Metrics;
using Celarix.Starfall.Libra.Metrics.Symbols;
using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Libra.LibraExpressions;

namespace Celarix.Starfall.Playground.DelphinusTests
{
    internal sealed class DelphinusSlide : AtriaSlide
    {
        private static readonly SColor _backgroundColor = new SColor(8, 0, 130, 255);
        private static readonly SFont _baseFont = new SFontFamily("Cambria Math", 30f);
        private int _state = 0;

        public DelphinusSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = _backgroundColor;

            //var _0 = AddExpr(Text("x"), Text("1"));
            //var _1 = Concat(Text("x"), Paren(_0));
            //var _2 = Frac(_1, Text("2"));
            //var _3 = Concat(Text("sumtorial"), Paren(Text("x")));
            //var _4 = EqualByDef(_3, _2);

            var _0 = Text("mt");
            var _1 = Sub(Text("1"), Text("t"));
            var _1a = Equal(_0, _1);

            var _2 = Exp(Text("mt"), Text("2"));
            var _3 = Mul(_2, Subscript(Text("X"), Text("0")));
            var _4 = Paren(_3);

            var _5 = Text("2");
            var _6 = Text("mt");
            var _7 = Text("t");
            var _8 = Subscript(Text("X"), Text("1"));
            var _9 = Mul(_5, _6);
            var _10 = Mul(_7, _8);
            var _11 = Mul(_9, _10);
            var _12 = Paren(_11);

            var _13 = Exp(Text("t"), Text("2"));
            var _14 = Subscript(Text("X"), Text("2"));
            var _15 = Mul(_13, _14);
            var _16 = Paren(_15);

            var _17 = AddExpr(_4, _12);
            var _18 = AddExpr(_17, _16);

            var _19 = Concat(2d, [_1a, _18]);

            var libraElement = new LibraElement(
                _19,
                _baseFont, "#libra");
            var anchor = new BasisPoint(Center, "anchor");
            libraElement.AnchorCenterTo(anchor);
            Add([libraElement, anchor]);
        }

        //public override void Render(IRenderTarget target)
        //{
        //    target.Clear(_backgroundColor);

        //    var metrics = new LibraMetrics
        //    {
        //        BinaryExpressions = new BinaryExpressionMetrics(),
        //        Fences = new FenceMetrics
        //        {
        //            ParenthesesMetrics = new ParenthesesMetrics()
        //        },
        //        Fractions = new FractionMetrics(),
        //        Scripts = new ScriptsMetrics()
        //    };

        //    var context = new LibraRenderingContext(MeasurementService, _baseFont.WithSize(224f), metrics, FenceRenderingMode.Procedural);
        //    var pMetrics = metrics.Fences.ParenthesesMetrics;

        //    var w = pMetrics.ParenthesesWidthEm * context.Em;
        //    var h = pMetrics.ParenthesesHeightEm * context.Em;
        //    var t = pMetrics.EndThicknessEm * context.Em;
        //    var i = pMetrics.EndVerticalInsetEm * context.Em;

        //    var outerTop = new SPointF(w - t, 0);
        //    var innerTop = new SPointF(w, 0);
        //    var innerBottom = new SPointF(w, h);
        //    var outerBottom = new SPointF(w - t, h);

        //    var innerControl1 = new SPointF(
        //        w - (pMetrics.InnerBulge * w),
        //        h * pMetrics.UpperControlYFraction
        //    );
        //    var innerControl2 = new SPointF(
        //        w - (pMetrics.InnerBulge * w),
        //        h * pMetrics.LowerControlYFraction
        //    );

        //    var outerControl1 = new SPointF(
        //        w - (pMetrics.OuterBulge * w),
        //        h * pMetrics.LowerControlYFraction
        //    );

        //    var outerControl2 = new SPointF(
        //        w - (pMetrics.OuterBulge * w),
        //        h * pMetrics.UpperControlYFraction
        //    );

        //    var libraPath = new LibraPathBuilder()
        //        .MoveTo(outerTop.X, outerTop.Y)
        //        .LineTo(innerTop.X, innerTop.Y)
        //        .CubicTo(innerControl1.X, innerControl1.Y,
        //            innerControl2.X, innerControl2.Y,
        //            innerBottom.X, innerBottom.Y)
        //        .LineTo(outerBottom.X, outerBottom.Y)
        //        .CubicTo(outerControl1.X, outerControl1.Y,
        //            outerControl2.X, outerControl2.Y,
        //            outerTop.X, outerTop.Y)
        //        .ClosePath();
        //    var libraStyle = new PathStyle(SColor.White, null, 2d, SStrokeCap.Butt, SStrokeJoin.Miter);
        //    var renderable = new LibraPathRenderable(new LibraRenderableKey(Guid.NewGuid(), "test"), libraPath, libraStyle);

        //    SPointF offset = new(300, 300);
        //    renderable.RenderAt(target, offset, 1d);

        //    var debugCircleSize = new SSizeF(5d, 5d);
        //    target.DrawEllipse(outerTop + offset, debugCircleSize, SColor.Red, SPaintStyle.Fill);
        //    target.DrawEllipse(innerTop + offset, debugCircleSize, SColor.Red, SPaintStyle.Fill);
        //    target.DrawEllipse(innerBottom + offset, debugCircleSize, SColor.Red, SPaintStyle.Fill);
        //    target.DrawEllipse(outerBottom + offset, debugCircleSize, SColor.Red, SPaintStyle.Fill);
        //    target.DrawEllipse(innerControl1 + offset, debugCircleSize, SColor.Green, SPaintStyle.Fill);
        //    target.DrawEllipse(innerControl2 + offset, debugCircleSize, SColor.Green, SPaintStyle.Fill);
        //    target.DrawEllipse(outerControl1 + offset, debugCircleSize, SColor.Green, SPaintStyle.Fill);
        //    target.DrawEllipse(outerControl2 + offset, debugCircleSize, SColor.Green, SPaintStyle.Fill);
        //}

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
