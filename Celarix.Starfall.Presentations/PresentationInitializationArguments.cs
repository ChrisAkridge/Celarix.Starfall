using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations
{
    internal sealed class PresentationInitializationArguments
    {
        public required int ViewportWidth { get; init; }
        public required int ViewportHeight { get; init; }
        public required bool ReinitializeOnLastChanceException { get; init; }
    }
}
