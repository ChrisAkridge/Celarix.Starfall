using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Libra.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal static class LibraSyntaxValidator
    {
        private enum IdentifierKind
        {
            Class,
            ID
        }
        private sealed record Identifier(string IdentifierText, IdentifierKind Kind);

        public static void Validate(ExpressionSyntax expression)
        {
            if (expression is PrefixSyntax prefix)
            {
                Validate(prefix.Operand);
            }
            else if (expression is BinarySyntax binary)
            {
                Validate(binary.Left);
                Validate(binary.Right);
            }
            else if (expression is ScriptSyntax script)
            {
                Validate(script.Base);
                if (script.Superscript is not null)
                {
                    Validate(script.Superscript);
                }
                if (script.Subscript is not null)
                {
                    Validate(script.Subscript);
                }
            }
            else if (expression is ReservedNameSyntax reservedName)
            {
                if (!OperatorRegistry.IsKnownReservedName(reservedName.Name))
                {
                    throw new LibraParseException(new($"Unknown reserved name '{reservedName.Name}'", reservedName.Span));
                }
            }
            else if (expression is ReservedCallSyntax reservedCall)
            {
                foreach (var argument in reservedCall.Arguments)
                {
                    Validate(argument);
                }
            }
            else if (expression is IdentifierSyntax identifier)
            {
                ValidateIdentifierBlock(identifier);
            }
            else if (expression is PropertyBlockSyntax propertyBlock)
            {
                Validate(propertyBlock.Expression);
            }
            else if (expression is SubstitutionSyntax substitution)
            {
                ValidateSubstitution(substitution);
            }
            else if (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
            {
                Validate(parenthesizedExpression.Expression);
            }
            else if (expression is BracedExpressionSyntax bracedExpression)
            {
                Validate(bracedExpression.Expression);
            }
        }

        public static void ValidateIdentifierBlock(IdentifierSyntax identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier.IdentifierBlock))
            {
                throw new LibraParseException(new("Identifier cannot be empty or whitespace", identifier.Span));
            }

            var results = new List<Identifier>();
            var seen = new HashSet<Identifier>();
            var position = 0;
            var text = identifier.IdentifierBlock;

            while (position < text.Length)
            {
                var kind = text[position] switch
                {
                    '.' => IdentifierKind.Class,
                    '#' => IdentifierKind.ID,
                    _ => throw new LibraParseException(new($"Unexpected character '{text[position]}' in identifier block", identifier.Span))
                };

                position += 1;

                if (position >= text.Length || !IsIdentifierStart(text[position]))
                {
                    throw new LibraParseException(new($"Expected identifier after '{text[position - 1]}'", identifier.Span));
                }

                var start = position++;
                while (position < text.Length && IsIdentifierContinuation(text[position]))
                {
                    position += 1;
                }

                var name = text[start..position];

                if (!seen.Add(new(name, kind)))
                {
                    throw new LibraParseException(new($"Duplicate identifier '{name}' of kind '{kind}'", identifier.Span));
                }

                results.Add(new(name, kind));
            }
        }

        private static void ValidateSubstitution(SubstitutionSyntax substitution)
        {
            if (!IsValidIdentifier(substitution.Name))
            {
                throw new LibraParseException(new($"Substitution name '{substitution.Name}' is not a valid identifier", substitution.Span));
            }
        }

        private static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            if (!IsIdentifierStart(name[0]))
            {
                return false;
            }
            for (var i = 1; i < name.Length; i++)
            {
                if (!IsIdentifierContinuation(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValidPropertyValue(string value)
        {
            // Property values are valid if all characters are also valid identifier continuation characters
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            for (var i = 0; i < value.Length; i++)
            {
                if (!IsIdentifierContinuation(value[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsIdentifierStart(char c) => c is >= 'A' and <= 'Z'
                or >= 'a' and <= 'z'
                or '_';

        private static bool IsIdentifierContinuation(char c) => IsIdentifierStart(c) || (c >= '0' && c <= '9');
    }
}
