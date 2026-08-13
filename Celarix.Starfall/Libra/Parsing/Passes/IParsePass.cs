using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing.Passes
{
    internal interface IParsePass
    {
        public bool TryPass(IReadOnlyList<LibraToken> tokens,
            [NotNullWhen(true)] out IReadOnlyList<LibraToken>? parsedTokens,
            [NotNullWhen(false)] out LibraDiagnostic? diagnostic);
    }
}
