using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal static class FPHelpers
    {
        public static char ToUnicodeSuperscript(char digit)
        {
            return digit switch
            {
                '0' => '⁰',
                '1' => '¹',
                '2' => '²',
                '3' => '³',
                '4' => '⁴',
                '5' => '⁵',
                '6' => '⁶',
                '7' => '⁷',
                '8' => '⁸',
                '9' => '⁹',
                '-' => '⁻',
                _ => throw new ArgumentException($"Invalid digit character: {digit}")
            };
        }
    }
}
