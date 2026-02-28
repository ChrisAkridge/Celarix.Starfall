using Celarix.Starfall.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentation.Graph
{
    public sealed class TransitionEdge<TScene, TTransition>
         where TScene : class
         where TTransition : ITransition
    {
        public SceneNode<TScene> From { get; }
        public SceneNode<TScene> To { get; }
        public TTransition Transition { get; }

        public TransitionEdge(SceneNode<TScene> from, SceneNode<TScene> to, TTransition transition)
        {
            From = from;
            To = to;
            Transition = transition;
        }
    }
}
