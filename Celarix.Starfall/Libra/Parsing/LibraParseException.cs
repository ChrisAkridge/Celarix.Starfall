using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    public sealed class LibraParseException : Exception
    {
        public LibraDiagnostic Diagnostic { get; }

        public LibraParseException(LibraDiagnostic diagnostic) : base(diagnostic.Message)
        {
        }
    }
}
