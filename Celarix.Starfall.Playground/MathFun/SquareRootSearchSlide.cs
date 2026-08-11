using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Libra.LibraExpressions;

namespace Celarix.Starfall.Playground.MathFun
{
    internal enum SquareRootSearchSlideLayers
    {
        MainLayer
    }

    internal sealed class SquareRootSearchSlide : LayeredAtriaSlide<SquareRootSearchSlideLayers>
    {
        private enum SearchPhase
        {
            Expansion,
            BinarySearch
        }

        private const double TargetSquare = 5d;

        private static readonly SFont _font = new SFontFamily("Cambria Math", 36f);

        private readonly LayeredProblemStack _problemStack;
        private SearchPhase _searchPhase;
        private int? _expansionPowerOf2;
        private double? _binarySearchGuess;

        public SquareRootSearchSlide(int width, int height) : base(width, height)
        {
            _problemStack = new LayeredProblemStack(width, height);
            _problemStack.AddProblem(new SColor(8, 0, 130, 255));

            _searchPhase = SearchPhase.Expansion;
            _expansionPowerOf2 = 0;
        }

        public override void Initialize()
        {
            RenderOwnBackground = false;
            var points = MathHelpers.EquallySpaceCenteredPoints(0, Size.Height, 4)
                .Select(p => new SPointF(Center.X, p))
                .ToArray();

            var targetExpression = Concat(1d,
                Text("Target:"),
                SquareRoot(TargetSquare));
            var target = new LibraElement(targetExpression, _font, "#target");
            var targetAnchor = new BasisPoint(points[0], "#targetAnchor");
            target.AnchorCenterTo(targetAnchor);
            

            var guessExpression = Concat(1d,
                Text("Guess:"),
                Equal(Exp(Text("2"), Text(_expansionPowerOf2!.Value.ToString())), Text("1")));
            var guess = new LibraElement(guessExpression, _font, "#guess");
            var guessAnchor = new BasisPoint(points[1], "#guessAnchor");
            guess.AnchorCenterTo(guessAnchor);

            var implied_0 = Equal(SquareRoot(TargetSquare), Text("1"));
            var implied_1 = new BinaryExpression("∴", implied_0, Equal(Exp(Text("1"), Text("2")), Text("5")),
                SColor.White, SColor.Transparent);
            var implied = new LibraElement(implied_1, _font, "#implied");
            var impliedAnchor = new BasisPoint(points[2], "#impliedAnchor");
            implied.AnchorCenterTo(impliedAnchor);

            var result_0 = Equal(Exp(Text("1"), Text("2")), Text("1"));
            var result_1 = new BinaryExpression("<", Text("1"), Text("5"), SColor.White, SColor.Transparent);
            var result_2 = new BinaryExpression(">", SquareRoot(TargetSquare), Text("1"), SColor.White, SColor.Transparent);
            var resultExpression = Concat(1.5d, result_0, result_1, result_2);
            var result = new LibraElement(resultExpression, _font, "#result");
            var resultAnchor = new BasisPoint(points[3], "#resultAnchor");
            result.AnchorCenterTo(resultAnchor);

            Layers[SquareRootSearchSlideLayers.MainLayer].Add([target, targetAnchor,
                guess, guessAnchor, implied, impliedAnchor, result, resultAnchor]);
        }

        public override void Render(IRenderTarget target)
        {
            _problemStack.RenderBackgrounds(target);
            base.Render(target);
        }

        // Man, Libra expressions are a bit hard to build right now. We'll use these for now.
        private LibraExpression SquareRoot(double value) => Concat(Text("sqrt"), Paren(Text(value.ToString())));
    }
}
