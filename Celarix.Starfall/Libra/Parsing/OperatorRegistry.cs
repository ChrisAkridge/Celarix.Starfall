using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal static class OperatorRegistry
    {
        public const int CallAndPostfixBlockBindingPower = 110;
        public const int ScriptBindingPower = 100;
        private const int UnaryPrefixBindingPower = 90;
        private const int MultiplicativeBindingPower = 80;
        private const int AdditiveBindingPower = 70;
        private const int RelationalBindingPower = 60;
        private const int EqualityBindingPower = 50;

        private static readonly IReadOnlyList<OperatorInfo> _operators;
        private static readonly IReadOnlyList<ReservedFunctionInfo> _reservedFunctions;
        private static readonly IReadOnlyList<char> _firstChars;
        private static readonly TrieNode? _trie;

        static OperatorRegistry()
        {
            var info = new List<OperatorInfo>
            {
                new("+", "+", OperatorKind.Prefix, new PrefixOperatorRule(UnaryPrefixBindingPower), null),
                new("-", "-", OperatorKind.Prefix, new PrefixOperatorRule(UnaryPrefixBindingPower), null),
                new("+", "+", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(AdditiveBindingPower)),
                new("-", "-", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(AdditiveBindingPower)),
                new("*", "·", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(MultiplicativeBindingPower)),
                new("/", "÷", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(MultiplicativeBindingPower)),
                new("^", "", OperatorKind.Infix, null, new ScriptOperatorRule()),
                new("_", "", OperatorKind.Infix, null, new ScriptOperatorRule()),
                new("=", "=", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(EqualityBindingPower)),
                new("!=", "≠", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(EqualityBindingPower)),
                new("<", "<", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(RelationalBindingPower)),
                new(">", ">", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(RelationalBindingPower)),
                new("<=", "≤", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(RelationalBindingPower)),
                new(">=", "≥", OperatorKind.Infix, null, BinaryOperatorRule.LeftAssociative(RelationalBindingPower))
            };
            _operators = [.. info];

            var funcInfo = new List<ReservedFunctionInfo>
            {
                new(";frac", 2, (context, id, args) => new FractionExpression(args[0], args[1], context.ForegroundColor, context.BackgroundColor, id))
            };
            _reservedFunctions = [.. funcInfo];

            // Get the list of first characters. Don't count anything starting with ; as we parse that
            // as a reserved symbol in the lexer and only need it in the registry for precedence purposes.
            _firstChars = [.. info
                .Select(i => i.Symbol[0])
                .Where(c => c != ';')
                .Distinct()];
            var alphabetSize = _firstChars.Count;

            _trie = new TrieNode(string.Empty, alphabetSize);
            foreach (var op in info.Where(i => !i.IsReservedName))
            {
                Trie.Insert(_trie, op.Symbol, _firstChars);
            }
        }

        public static bool IsOperatorStart(string s)
        {
            // Use the trie to check if the string starts with any operator symbol
            return Trie.Search(_trie!, s, _firstChars);
        }

        public static bool TryMatchLongestOperator(string source, int start,
            [NotNullWhen(true)] out string? symbol)
        {
            var spanLength = 1;
            string? longestSeenMatch = null;
            string? lastSeenMatch = null;

            do
            {
                var span = source.Substring(start, spanLength);
                var trieMatch = Trie.Search(_trie!, span, _firstChars);
                if (trieMatch)
                {
                    lastSeenMatch = span;
                    longestSeenMatch = lastSeenMatch;
                    spanLength += 1;
                }
                else
                {
                    lastSeenMatch = null;
                }
            } while (lastSeenMatch != null);

            symbol = longestSeenMatch;
            return symbol != null;
        }

        public static bool IsKnownReservedName(string reservedName)
        {
            return _operators.Any(op => op.IsReservedName && op.Symbol == reservedName);
        }

        public static bool TryGetKnownReservedFunction(string reservedFunctionName, [NotNullWhen(true)] out ReservedFunctionInfo? info)
        {
            info = _reservedFunctions.SingleOrDefault(f => f.Name == reservedFunctionName);
            return info != null;
        }

        public static bool TryGetWhenSome(string operatorText,
            [NotNullWhen(true)] out IWhenSomeRule? rule)
        {
            rule = null;

            // Find the set of matching operators by the text
            var matchingOperators = _operators.Where(op => op.Symbol == operatorText).ToList();
            if (matchingOperators.Count == 0) { return false; }

            var operatorWithWhenSome = matchingOperators.SingleOrDefault(o => o.WhenSomeRule != null);
            if (operatorWithWhenSome == null) { return false; }

            rule = operatorWithWhenSome.WhenSomeRule!;
            return true;
        }

        public static bool TryGetWhenNone(string operatorOrReservedText,
            [NotNullWhen(true)] out IWhenNoneRule? rule)
        {
            rule = null;

            if (operatorOrReservedText.StartsWith(';'))
            {
                // Reserved function
                var reservedFunction = _reservedFunctions.SingleOrDefault(f => f.Name == operatorOrReservedText);
                if (reservedFunction == null) { return false; }
                rule = reservedFunction.WhenNoneRule;
                return true;
            }
            else
            {
                // Operator
                var matchingOperators = _operators.Where(op => op.Symbol == operatorOrReservedText).ToList();
                if (matchingOperators.Count == 0) { return false; }
                var operatorWithWhenNone = matchingOperators.SingleOrDefault(o => o.WhenNoneRule != null);
                if (operatorWithWhenNone == null) { return false; }
                rule = operatorWithWhenNone.WhenNoneRule!;
                return true;
            }
        }

        public static string GetRenderedSymbol(string operatorText)
        {
            var matchingOperator = _operators.SingleOrDefault(op => op.Symbol == operatorText);
            return matchingOperator == null
                ? throw new InvalidOperationException($"No operator found for symbol '{operatorText}'")
                : matchingOperator.RenderedSymbol;
        }
    }
}
