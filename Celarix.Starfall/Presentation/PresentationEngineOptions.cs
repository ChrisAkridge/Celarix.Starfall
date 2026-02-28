using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentation
{
    public sealed class PresentationEngineOptions
    {
        public required ErrorLevel ErrorLevel { get; init; } = ErrorLevel.Display;
    }
}
