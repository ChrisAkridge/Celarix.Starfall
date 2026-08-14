using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Libra.LibraExpressions;
using Replacement = (string QuerySelector, System.Func<Celarix.Starfall.Libra.Expressions.LibraExpression, Celarix.Starfall.Libra.Expressions.LibraExpression> ReplacementFactory);
using ReplacementList = System.Collections.Generic.IReadOnlyList<(string QuerySelector, System.Func<Celarix.Starfall.Libra.Expressions.LibraExpression, Celarix.Starfall.Libra.Expressions.LibraExpression> ReplacementFactory)>;

namespace Celarix.Starfall.Playground.MathFun
{
    internal sealed class InvertibleFunctionBinarySearcher
    {
        private const string BinarySearch_01Expansion = "BinarySearch_01Expansion";
        private const string BinarySearch_02BinarySearch = "BinarySearch_02BinarySearch";

        private readonly InvertibleFunctionSearchStrategy _strategy;
        private readonly Func<double, double> _function;
        private readonly Func<double, double> _inverse;
        private readonly LibraExpressionFactory<double> _functionFormatter;
        private readonly LibraExpressionFactory<double> _inverseFormatter;

        private double _functionTarget;
        private string _strategyPhase;

        // Binary search state
        private int _expansionPowerOf2 = 0;
        private double _binarySearchHigh = 0d;
        private double _binarySearchLow = 0d;

        public double CurrentGuess { get; private set; }

        public InvertibleFunctionBinarySearcher(InvertibleFunctionSearchStrategy strategy,
            Func<double, double> function,
            Func<double, double> inverse,
            LibraExpressionFactory<double> functionFormatter,
            LibraExpressionFactory<double> inverseFormatter,
            double functionTarget)
        {
            _strategy = strategy;
            _function = function;
            _inverse = inverse;
            _functionFormatter = functionFormatter;
            _inverseFormatter = inverseFormatter;
            _functionTarget = functionTarget;

            if (strategy == InvertibleFunctionSearchStrategy.BinarySearch)
            {
                _strategyPhase = BinarySearch_01Expansion;
                CurrentGuess = 1d; // Start with 2^0 = 1
            }
        }

        // Display and formatting
        public IReadOnlyList<LibraExpression> BuildInitialExpressions()
        {
            var targetString = _functionTarget.ToString();
            string currentGuessStr = CurrentGuess.ToString();

            // Target expression (i.e., "Target: sqrt(5)")
            var target = Concat(0.75d, Text("Target:"), _functionFormatter.ExpressionFactory(_functionTarget));

            // Guess expression
            var guess = BuildGuessExpression();

            // Implication expression
            var impliedFunction = Equal(_functionFormatter.ExpressionFactory(_functionTarget), Text(currentGuessStr, "#impliedGuess"));
            var impliedInverse = Equal(_inverseFormatter.ExpressionFactory(CurrentGuess), Text(targetString, "#impliedInverse"));
            var implied = new BinaryExpression("∴", impliedFunction, impliedInverse, SColor.White, SColor.Transparent);

            // Result expression
            var inverseResult = _inverse(CurrentGuess);
            var comparison = inverseResult.CompareTo(_functionTarget);
            var comparisonExpression = 
                BuildComparisonExpression(currentGuessStr, inverseResult, comparison);
            var actualInverse = Equal(_inverseFormatter.ExpressionFactory(CurrentGuess), Text(inverseResult.ToString(), "#actualInverse"));
            var result = Concat(0.75d,
                actualInverse,
                comparisonExpression);

            return [target, guess, implied, result];
        }

        private BinaryExpression BuildComparisonExpression(string currentGuessStr, double inverseResult, int comparison)
        {
            var comparisonOperator = comparison switch
            {
                < 0 => "<",
                0 => "=",
                > 0 => ">"
            };
           
            return new BinaryExpression(comparisonOperator,
                _functionFormatter.ExpressionFactory(_functionTarget),
                Text(currentGuessStr), SColor.White, SColor.Transparent, "#comparison");
        }

        private LibraExpression BuildGuessExpression()
        {
            if (_strategy == InvertibleFunctionSearchStrategy.BinarySearch)
            {
                if (_strategyPhase == BinarySearch_01Expansion)
                {
                    return BuildBinarySearchExpansionGuess();
                }
                else if (_strategyPhase == BinarySearch_02BinarySearch)
                {
                    return BuildBinarySearchGuess();
                }
            }

            throw new NotImplementedException();
        }

        private LibraExpression BuildBinarySearchExpansionGuess()
        {
            // "Guess: 2^0 = 1"
            var guessValue = Math.Pow(2, _expansionPowerOf2);
            var guessExpression = Concat(0.75d,
                Text("Guess:"),
                Equal(Exp(Text("2"), Text(_expansionPowerOf2.ToString(), "#bsExpPow2")), Text(guessValue.ToString(), "#bsExpGuess")));
            return guessExpression;
        }

        private LibraExpression BuildBinarySearchGuess()
        {
            // "Range: 4 - 2 = 2, Half: 2/2 = 1, Guess: 2 + 1 = 3"
            var range = _binarySearchHigh - _binarySearchLow;
            var half = range / 2;
            var guess = _binarySearchLow + half;

            string binarySearchLowStr = _binarySearchLow.ToString();
            string rangeStr = range.ToString();
            string halfStr = half.ToString();
            string guessStr = guess.ToString();
            var rangeExpression = Concat(0.5d,
                Text("Range:"),
                Equal(Sub(Text(_binarySearchHigh.ToString(), ".bsHigh"), Text(binarySearchLowStr, ".bsLow")), Text(rangeStr, ".bsRange")));
            var halfExpression = Concat(0.5d,
                Text("Half:"),
                Equal(Frac(Text(rangeStr, ".bsRange"), Text("2")), Text(halfStr, ".bsHalf")));
            var guessExpression = Concat(0.5d,
                Text("Guess:"),
                Equal(AddExpr(Text(binarySearchLowStr, ".bsLow"), Text(halfStr, ".bsHalf")), Text(guessStr, ".bsGuess")));
            
            return Concat(0.75d, rangeExpression, halfExpression, guessExpression);
        }

        // Evaluation
        public ReplacementList StepEvaluation()
        {
            if (_strategy == InvertibleFunctionSearchStrategy.BinarySearch)
            {
                if (_strategyPhase == BinarySearch_01Expansion)
                {
                    return StepBinarySearchExpansion();
                }
                else if (_strategyPhase == BinarySearch_02BinarySearch)
                {
                    return StepBinarySearch();
                }
            }

            throw new NotImplementedException();
        }

        private ReplacementList StepBinarySearchExpansion()
        {
            // Increment or decrement the expansion power of 2.
            var oldExpansionPowerOf2 = _expansionPowerOf2;
            _expansionPowerOf2 += (_functionTarget < 1d) ? -1 : 1;

            // Try the new guess to see if we've exceeded the target.
            var newGuess = Math.Pow(2, _expansionPowerOf2);
            var newInverse = _inverse(newGuess);
            var tooFar = (_functionTarget < 1d) ? (newInverse < _functionTarget) : (newInverse > _functionTarget);

            if (tooFar)
            {
                // We can advance to the binary search phase.
                _strategyPhase = BinarySearch_02BinarySearch;
            }

            // Set the binary search low and high bounds based on the last two guesses.
            _binarySearchLow = Math.Pow(2, oldExpansionPowerOf2);
            _binarySearchHigh = newGuess;

            // Set the other variables.
            CurrentGuess = newGuess;

            string inverseId = _inverseFormatter.IdFactory(CurrentGuess);
            var result = new List<Replacement>
            {
                ("#bsExpPow2", _ => Text(_expansionPowerOf2.ToString(), "#bsExpPow2")),
                ("#bsExpGuess", _ => Text(newGuess.ToString(), "#bsExpGuess")),
                ("#impliedGuess", _ => Text(newGuess.ToString(), "#impliedGuess")),
                (inverseId, _ => _inverseFormatter.ExpressionFactory(newInverse)),
                ("#comparison", _ => BuildComparisonExpression(CurrentGuess.ToString(), newInverse, newInverse.CompareTo(_functionTarget)))
            };
            return result;
        }

        private ReplacementList StepBinarySearch() => throw new NotImplementedException();

        // Internal Libra factories
        private LibraExpression SquareRoot(double value) => Concat(Text("sqrt"), Paren(Text(value.ToString())));
    }

    internal enum InvertibleFunctionSearchStrategy
    {
        BinarySearch,
        DecimalSearch,
        NewtonApproximation
    }
}
