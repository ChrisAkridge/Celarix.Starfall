using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal enum TokenKind
    {
        None,
        Unresolved,
        TextLiteral,
        UnseparatedIdentifier,
        UnseparatedPropertyBlock,
        Substitution,
    }
}
