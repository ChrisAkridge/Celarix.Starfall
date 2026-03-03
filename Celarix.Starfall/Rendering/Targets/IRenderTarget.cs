using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Targets
{
    public interface IRenderTarget
    {
        bool CanAnimate { get; }
        bool IsAnimating { get; set; }

        void Start();
        void Complete();

        void Clear(SColor color);
        void DrawRectangle(SRectF bounds, SColor color, SAngle rotation);
    }
}
