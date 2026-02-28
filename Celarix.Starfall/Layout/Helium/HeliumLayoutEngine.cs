using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public class HeliumLayoutEngine : ILayoutEngine<HeliumScene, IHeliumTransition>
    {
        private IRenderTarget? renderTarget;
        private int viewportWidth;
        private int viewportHeight;

        public bool IsAnimating => throw new NotImplementedException();

        public HeliumLayoutEngine(int viewportWidth, int viewportHeight)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
        }

        public void Render(HeliumScene scene)
        {
            ThrowIfNoRenderTarget();
            scene.Render(renderTarget!, new SSizeF(viewportWidth, viewportHeight));
        }

        public void Transition(HeliumScene from, HeliumScene to, IHeliumTransition transition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the render target for this layout engine. This is required before any rendering or transitioning can be done.
        /// This method is used over a property setter to allow for the engine to be made before the target, but also
        /// to prevent for the target from being changed after the engine is made.
        /// </summary>
        /// <param name="renderTarget"></param>
        public void SetRenderTarget(IRenderTarget renderTarget)
        {
            if (this.renderTarget != null)
            {
                throw new InvalidOperationException("Render target has already been set for this layout engine. It cannot be changed after being set.");
            }

            this.renderTarget = renderTarget;
        }

        private void ThrowIfNoRenderTarget()
        {
            // No, seriously, always throw. If we can't render anywhere, how are we supposed to have
            // an ErrorLevel of Display?
            if (renderTarget == null)
            {
                throw new InvalidOperationException("No render target has been set for this layout engine. Please call SetRenderTarget before attempting to render or transition.");
            }
        }
    }
}
