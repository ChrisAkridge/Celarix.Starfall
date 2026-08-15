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

        public static bool TryGetWhenSome(string operatorText,
            OperatorKind kind,
            [NotNullWhen(true)] out IWhenSomeRule? rule)
        {
            rule = null;

            if (!TryGetOperator(operatorText, kind, out var @operator)
                || @operator.WhenSomeRule == null)
            {
                return false;
            }

            rule = @operator.WhenSomeRule;
            return true;
        }

        public static bool TryGetWhenNone(string operatorOrReservedText,
            OperatorKind kind,
            [NotNullWhen(true)] out IWhenNoneRule? rule)
        {
            rule = null;

            if (!TryGetOperator(operatorOrReservedText, kind, out var @operator)
                || @operator.WhenNoneRule == null)
            {
                return false;
            }

            rule = @operator.WhenNoneRule;
            return true;
        }

        public static string GetRenderedSymbol(string operatorText,
            OperatorKind kind)
        {
            return TryGetOperator(operatorText, kind, out var @operator)
                ? @operator.RenderedSymbol
                : throw new InvalidOperationException($"No {kind} operator found for symbol '{operatorText}'");
        }

        private static bool TryGetOperator(string operatorText,
            OperatorKind kind,
            [NotNullWhen(true)] out OperatorInfo? @operator)
        {
            var matchingOperators = _operators
                .Where(op => op.Symbol == operatorText && op.Kind == kind)
                .ToList();

            if (matchingOperators.Count == 0)
            {
                @operator = null;
                return false;
            }

            if (matchingOperators.Count > 1)
            {
                throw new InvalidOperationException($"Multiple {kind} operators registered for symbol '{operatorText}'.");
            }

            @operator = matchingOperators[0];
            return true;
        }
    }
}
