using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables.Interfaces
{
    public interface IBoundedRenderable
    {
        SRectF Bounds { get; set; }
    }
}
