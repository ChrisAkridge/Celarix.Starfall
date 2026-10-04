using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Atria
{
    public abstract class BasisElement
    {
        public abstract AtriaId Id { get; protected set; }
        public abstract void RenderDebug(IRenderTarget target);
    }
}
