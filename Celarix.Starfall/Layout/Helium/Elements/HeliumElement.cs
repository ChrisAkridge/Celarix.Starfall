using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public abstract class HeliumElement : ISelectable<HeliumElement>
    {
        private readonly List<string> classes = [];

        public string? Id { get; protected set; }
        public IReadOnlyList<string> Classes => classes;
        public abstract IReadOnlyList<HeliumElement> Children { get; }
        public abstract double DesiredWidthFraction { get; }
        public abstract double DesiredHeightFraction { get; }

        // Properties used during the layout phase.
        internal SPointF? ActualPosition { get; set; }
        internal SSizeF? ActualSize { get; set; }
        internal SRectF? ActualBounds => ActualPosition.HasValue && ActualSize.HasValue
            ? new SRectF(ActualPosition.Value, ActualSize.Value)
            : null;

        public abstract void MeasureSelf(SSizeF availableSize);
        public abstract void ArrangeChildren(SRectF thisBounds);
        public abstract IReadOnlyList<IRenderable> GetRenderables();

        public abstract HeliumElement Clone();

        public bool HasClass(string className) => classes.Contains(className);

        public void AddClass(string className)
        {
            // TODO: raise error if class is already present
            if (!classes.Contains(className))
            {
                classes.Add(className);
            }
        }

        public void RemoveClass(string className)
        {
            // TODO: raise error if class is not present
            classes.Remove(className);
        }
    }
}
