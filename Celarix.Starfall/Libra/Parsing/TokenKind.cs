using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal enum TokenKind
    {
        EndOfInput,

        Text,
        String,
        ReservedName,
        Substitution,

        Operator,
        OpenParen,
        CloseParen,
        OpenBrace,
        CloseBrace,

        Comma,
        
        IdentifierBlock,
        PropertyBlock
    }
}
