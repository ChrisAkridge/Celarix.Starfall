using Celarix.Starfall.Layout;
using Celarix.Starfall.Presentation.Graph;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
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
        private readonly ILayoutEngine<TScene, TTransition> layoutEngine;
        private double progress;

        public TScene FromScene { get; }
        public TScene ToScene { get; }
        public string ToSceneId { get; }

        public ActiveTransition(TransitionEdge<TScene, TTransition> transitionEdge,
            ILayoutEngine<TScene, TTransition> layoutEngine,
            TScene fromScene, TScene toScene,
            string toSceneId)
        {
            this.transitionEdge = transitionEdge;
            this.layoutEngine = layoutEngine;
            progress = 0;
            FromScene = fromScene;
            ToScene = toScene;
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
            layoutEngine.Render(transitionEdge.Transition, progress);
        }
    }
}
