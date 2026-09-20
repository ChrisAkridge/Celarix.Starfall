using Celarix.Starfall.Libra.Parsing.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal static class PropertyRegistry
    {
        private static readonly IReadOnlyDictionary<string, IPropertyDefinition> _definitions =
            new Dictionary<string, IPropertyDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["foreground"] = new ForegroundPropertyDefinition(),
                ["background"] = new BackgroundPropertyDefinition(),
                ["fencetype"] = new FenceTypePropertyDefinition()
            };

        public static PropertyBindingResult BindProperties(PropertyBlockSyntax propertyBlock,
            LibraBuildContext context,
            LibraBinder binder)
        {
            var entries = ParseEntries(propertyBlock, binder);
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newContext = context;
            TextSpan? fenceTypeSpan = null;

            foreach (var entry in entries)
            {
                if (!seenKeys.Add(entry.Key))
                {
                    throw binder.CreateValidationException(entry.KeySpan,
                        $"Duplicate property '{entry.Key}'.");
                }

                if (!TryGetDefinition(entry.Key, out var definition))
                {
                    throw binder.CreateValidationException(entry.KeySpan,
                        $"Unknown property '{entry.Key}'.");
                }

                if (definition.IsFenceTypeProperty)
                {
                    fenceTypeSpan = entry.KeySpan;
                }
                newContext = definition.Apply(newContext, entry, binder);
            }

            return new PropertyBindingResult(newContext, fenceTypeSpan);
        }

        private static bool TryGetDefinition(string key,
            [NotNullWhen(true)] out IPropertyDefinition? definition)
        {
            return _definitions.TryGetValue(key, out definition);
        }

        private static IReadOnlyList<PropertyEntry> ParseEntries(PropertyBlockSyntax propertyBlock,
            LibraBinder binder)
        {
            var text = propertyBlock.PropertyBlock;
            if (text.Length < 2
                || text[0] != '['
                || text[^1] != ']')
            {
                throw binder.CreateValidationException(CreatePropertyBlockTokenSpan(propertyBlock),
                    "Property block must include opening and closing brackets.");
            }

            var innerStart = 1;
            var innerLength = text.Length - 2;
            if (innerLength == 0)
            {
                throw binder.CreateValidationException(CreatePropertyBlockTokenSpan(propertyBlock),
                    "Property block cannot be empty.");
            }

            var entries = new List<PropertyEntry>();
            var propertyStart = innerStart;
            while (propertyStart <= innerLength)
            {
                var propertyEnd = text.IndexOf(',', propertyStart);
                if (propertyEnd < 0)
                {
                    propertyEnd = innerStart + innerLength;
                }

                if (propertyEnd == propertyStart)
                {
                    throw binder.CreateValidationException(CreateSpan(propertyBlock, propertyStart, 1),
                        "Property block contains an empty property.");
                }

                entries.Add(ParseEntry(propertyBlock, propertyStart, propertyEnd, binder));

                if (propertyEnd == innerStart + innerLength)
                {
                    break;
                }
                propertyStart = propertyEnd + 1;
            }

            return entries;
        }

        private static PropertyEntry ParseEntry(PropertyBlockSyntax propertyBlock,
            int propertyStart,
            int propertyEnd,
            LibraBinder binder)
        {
            var text = propertyBlock.PropertyBlock;
            var equalsIndex = text.IndexOf('=', propertyStart, propertyEnd - propertyStart);
            var propertySpan = CreateSpan(propertyBlock, propertyStart, propertyEnd - propertyStart);
            if (equalsIndex < 0)
            {
                throw binder.CreateValidationException(propertySpan,
                    $"Property '{text[propertyStart..propertyEnd]}' is not in the expected 'key=value' format.");
            }

            if (equalsIndex == propertyStart)
            {
                throw binder.CreateValidationException(propertySpan,
                    "Property key cannot be empty.");
            }

            if (equalsIndex == propertyEnd - 1)
            {
                throw binder.CreateValidationException(propertySpan,
                    $"Property '{text[propertyStart..propertyEnd]}' must have a value.");
            }

            if (text.IndexOf('=', equalsIndex + 1, propertyEnd - equalsIndex - 1) >= 0)
            {
                throw binder.CreateValidationException(propertySpan,
                    $"Property '{text[propertyStart..propertyEnd]}' is not in the expected 'key=value' format.");
            }

            var key = text[propertyStart..equalsIndex];
            var value = text[(equalsIndex + 1)..propertyEnd];
            var keySpan = CreateSpan(propertyBlock, propertyStart, key.Length);
            var valueSpan = CreateSpan(propertyBlock, equalsIndex + 1, value.Length);

            return new PropertyEntry(key,
                value,
                propertySpan,
                keySpan,
                valueSpan);
        }

        private static TextSpan CreateSpan(PropertyBlockSyntax propertyBlock,
            int relativeStart,
            int length)
        {
            return new TextSpan(propertyBlock.Span.Text,
                PropertyBlockStartIndex(propertyBlock) + relativeStart,
                length);
        }

        private static TextSpan CreatePropertyBlockTokenSpan(PropertyBlockSyntax propertyBlock)
        {
            return new TextSpan(propertyBlock.Span.Text,
                PropertyBlockStartIndex(propertyBlock),
                propertyBlock.PropertyBlock.Length);
        }

        private static int PropertyBlockStartIndex(PropertyBlockSyntax propertyBlock)
        {
            return propertyBlock.Span.EndIndex - propertyBlock.PropertyBlock.Length;
        }
    }
}
