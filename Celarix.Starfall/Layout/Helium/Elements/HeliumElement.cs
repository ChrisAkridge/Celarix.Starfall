using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
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

        public string? Id { get; set; }
        public IReadOnlyList<string> Classes => classes;
        public abstract IReadOnlyList<HeliumElement> Children { get; }
        public abstract double DesiredWidthFraction { get; }
        public abstract double DesiredHeightFraction { get; }

        // Properties used during the layout phase.
        protected internal SPointF? ActualPosition { get; set; }
        protected internal SSizeF? ActualSize { get; set; }
        protected internal SRectF? ActualBounds => ActualPosition.HasValue && ActualSize.HasValue
            ? new SRectF(ActualPosition.Value, ActualSize.Value)
            : null;

        // Animation properties.
        public bool IsAnimating { get; protected set; }

        public abstract void MeasureSelf(SSizeF availableSize, MeasurementService measurementService);
        public abstract void ArrangeChildren(SRectF thisBounds);
        public abstract IReadOnlyList<IRenderable> GetRenderables(MeasurementService measurementService);

        public abstract HeliumElement Clone();

        public virtual void Update(double deltaTime)
        {
            foreach (var child in Children)
            {
                child.Update(deltaTime);
            }
        }

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
