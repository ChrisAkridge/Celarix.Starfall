using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentation.Graph
{
    public sealed class SceneNode<TScene>
    {
        public TScene Scene { get; }

        public SceneNode(TScene scene)
        {
            Scene = scene;
        }
    }
}
