using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public sealed class HeliumScene
    {
        public HeliumElement? Root { get; set; }
        public SColor BackgroundColor { get; set; }

        public void Render(IRenderTarget target, SSizeF maxSize)
        {
            target.Clear(BackgroundColor);

            if (Root == null) { return; }

            Root.MeasureSelf(maxSize, new Rendering.MeasurementService(target));
            Root.ArrangeChildren(new SRectF(SPointF.Zero, maxSize));

            // Children never set their own position, so we must set the root's position to (0,0) before rendering.
            Root.ActualPosition = SPointF.Zero;

            var renderables = Root.GetRenderables();

            foreach (var renderable in renderables)
            {
                renderable.Render(target);
            }
        }

        /// <summary>
        /// Creates a deep clone of the current <see cref="HeliumScene"/> instance, including all of its elements and their properties.
        /// </summary>
        /// <returns>A clone of this scene with all objects cloned to new object instances.</returns>
        public HeliumScene Clone()
        {
            return new HeliumScene
            {
                Root = Root?.Clone(),
                BackgroundColor = BackgroundColor
            };
        }

        public IEnumerable<IRenderable> GetRenderables(SSizeF maxSize, MeasurementService measurementService)
        {
            if (Root == null) { return []; }

            Root.MeasureSelf(maxSize, measurementService);
            Root.ArrangeChildren(new SRectF(SPointF.Zero, maxSize));
            var renderables = Root.GetRenderables();
            ClearLayoutData(Root);
            return renderables;
        }

        /// <summary>
        /// Clears the layout data for the specified element and all of its descendant elements.
        /// </summary>
        /// <remarks>This method resets the layout-related properties, such as position and size, for the
        /// given element and recursively for all of its children. Use this method to prepare elements for a fresh
        /// layout calculation.</remarks>
        /// <param name="element">The root <see cref="HeliumElement"/> whose layout data will be cleared. Cannot be null.</param>
        private static void ClearLayoutData(HeliumElement element)
        {
            element.ActualPosition = null;
            element.ActualSize = null;
            foreach (var child in element.Children)
            {
                ClearLayoutData(child);
            }
        }
    }
}
