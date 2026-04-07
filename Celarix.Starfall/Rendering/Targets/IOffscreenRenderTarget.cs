using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public interface IOffscreenRenderTarget : IRenderTarget
    {
        SImage? CompletedImage { get; }
    }
}
