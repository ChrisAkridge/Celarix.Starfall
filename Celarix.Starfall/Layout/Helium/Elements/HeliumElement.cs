using Celarix.Starfall.Layout.Helium.Layout;
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

        public abstract PositionedHeliumElement Measure(SSizeF maxSize);
        public abstract SPointF Arrange(SPointF parentPosition, SPointF parentSize, SSizeF thisSize);

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
