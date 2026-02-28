using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering
{
    public interface INotifyFrameRequested
    {
        void OnFrameRequested(double deltaTime);
    }
}
