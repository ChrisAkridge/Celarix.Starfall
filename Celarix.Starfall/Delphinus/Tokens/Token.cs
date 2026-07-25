using Celarix.Starfall.Delphinus;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Delphinus.Tokens
{
    internal abstract class Token
    {
        public StyleContext? StyleContext { get; protected set; }
        public SSizeF? Size { get; internal set; }
        public SPointF? Position { get; internal set; }
    }
}
