using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class AtriaId
    {
        private enum SelectorAccumulatorState
        {
            Default,
            IdSelector,
            ClassSelector
        }

        private readonly List<string> _classes = new();

        public string? Id { get; }
        public IReadOnlyList<string> Classes => _classes;

        private AtriaId(string? id, IEnumerable<string> classes)
        {
            Id = id;
            _classes.AddRange(classes);
        }

        public bool Matches(string selector)
        {
            // Step 1. Remove any whitespace, interior and exterior, from the selector
            var selectorTrimmed = selector.RemoveWhitespace();

            // Step 2. Accumulate class and ID selectors
            var state = SelectorAccumulatorState.Default;
            var selectorBuilder = new StringBuilder();
            var selectors = new List<(SelectorAccumulatorState Type, string Selector)>();

            for (int i = 0; i < selectorTrimmed.Length; i++)
            {
                char c = selectorTrimmed[i];
                if (c == '#')
                {
                    // Flush previous selector if exists
                    if (selectorBuilder.Length > 0)
                    {
                        selectors.Add((state, selectorBuilder.ToString()));
                        selectorBuilder.Clear();
                    }
                    state = SelectorAccumulatorState.IdSelector;
                }
                else if (c == '.')
                {
                    // Flush previous selector if exists
                    if (selectorBuilder.Length > 0)
                    {
                        selectors.Add((state, selectorBuilder.ToString()));
                        selectorBuilder.Clear();
                    }
                    state = SelectorAccumulatorState.ClassSelector;
                }
                else
                {
                    selectorBuilder.Append(c);
                }
            }

            // Flush any remaining selector
            if (selectorBuilder.Length > 0)
            {
                selectors.Add((state, selectorBuilder.ToString()));
            }

            // Step 3. For now, we don't actually want any Default selectors, this might change
            if (selectors.Any(s => s.Type == SelectorAccumulatorState.Default))
            {
                throw new ArgumentException($"Selector '{selector}' must contain selectors that are only ID selectors (i.e. #id) or class selectors (i.e. .class).");
            }

            // Step 4. Check if any selector matches
            foreach (var (type, sel) in selectors)
            {
                if (type == SelectorAccumulatorState.IdSelector)
                {
                    if (Id == sel)
                    {
                        return true;
                    }
                }
                else if (type == SelectorAccumulatorState.ClassSelector)
                {
                    if (Classes.Contains(sel))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static AtriaId Parse(string idString)
        {
            var parts = idString.Split(['.', '#'], StringSplitOptions.RemoveEmptyEntries);
            string? id = null;
            var classes = new List<string>();
            foreach (var part in parts)
            {
                if (id == null && idString.Contains('#' + part))
                {
                    id = part;
                }
                else
                {
                    classes.Add(part);
                }
            }
            return new AtriaId(id, classes);
        }
    }
}
