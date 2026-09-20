using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    public sealed record LibraDiagnostic(string Message,
        TextSpan? textSpan);
}
