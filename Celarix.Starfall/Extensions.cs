using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall
{
    internal static class Extensions
    {
        private static char[] spaceSeparators;

        static Extensions()
        {
            var charInfo = Enumerable.Range(0, 0x110000)
                .Where(c => c < 0x00d800 || c > 0x00dfff) // Exclude surrogate code points
                .Select(char.ConvertFromUtf32)
                .GroupBy(s => char.GetUnicodeCategory(s, 0))
                .ToDictionary(g => g.Key);

            spaceSeparators = [.. charInfo[System.Globalization.UnicodeCategory.SpaceSeparator].SelectMany(s => s)];
        }

        public static string[] TokenizePreservingSpaces(this string line)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool? lastWasSpace = null;

            foreach (var ch in line)
            {
                bool isSpace = spaceSeparators.Contains(ch);

                if (lastWasSpace is null)
                {
                    // first character
                    sb.Append(ch);
                    lastWasSpace = isSpace;
                    continue;
                }

                if (isSpace == lastWasSpace)
                {
                    // same category (space vs non-space) -> accumulate
                    sb.Append(ch);
                }
                else
                {
                    // category changed -> flush current token, start new
                    tokens.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(ch);
                    lastWasSpace = isSpace;
                }
            }

            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
            }

            return tokens.ToArray();
        }
    }
}
