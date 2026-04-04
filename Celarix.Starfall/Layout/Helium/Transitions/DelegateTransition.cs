using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions
{
    public sealed class DelegateTransition : IHeliumTransition
    {
        private readonly Action<double, IRenderTarget> renderAction;
        public double Duration { get; init; }

        public DelegateTransition(double duration, Action<double, IRenderTarget> renderAction)
        {
            Duration = duration;
            this.renderAction = renderAction;
        }

        public void Render(double progress, IRenderTarget renderTarget) => renderAction(progress, renderTarget);
    }
}
