using Celarix.Starfall.Layout;
using Celarix.Starfall.Presentation.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentation
{
    public sealed class ActiveTransition<TScene, TTransition>
        where TScene : class
        where TTransition : ITransition
    {
        private readonly TransitionEdge<TScene, TTransition> transitionEdge;
        private double progress;

        public string FromSceneId { get; }
        public string ToSceneId { get; }

        public ActiveTransition(TransitionEdge<TScene, TTransition> transitionEdge, string fromSceneId, string toSceneId)
        {
            this.transitionEdge = transitionEdge;
            progress = 0;
            FromSceneId = fromSceneId;
            ToSceneId = toSceneId;
        }

        public bool Update(double deltaTime)
        {
            var deltaProgress = deltaTime / transitionEdge.Transition.Duration;
            progress += deltaProgress;

            if (progress >= 1d)
            {
                // Transition is complete
                return true;
            }

            return false;
        }

        public void Render()
        {
            transitionEdge.Transition.Render(progress);
        }
    }
}
