using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout
{
    public interface ITransition
    {
        double Duration { get; }
        void Render(double progress, IRenderTarget renderTarget);
    }
}
