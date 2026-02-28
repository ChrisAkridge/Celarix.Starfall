using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Layout
{
    public sealed class PositionedHeliumElement : ISelectable<PositionedHeliumElement>
    {
        private readonly List<PositionedHeliumElement> positionedChildren = [];
        private readonly List<string> classes = [];

        public HeliumElement Element { get; init; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }

        public string? Id { get; }

        public IReadOnlyList<string> Classes => classes;

        public IReadOnlyList<PositionedHeliumElement> Children => positionedChildren;

        public PositionedHeliumElement(HeliumElement element)
        {
            Element = element;
            Id = element.Id;
            classes = [.. element.Classes];
        }

        public PositionedHeliumElement(HeliumElement element, SSizeF size) : this(element)
        {
            Width = size.Width;
            Height = size.Height;
        }

        public void Arrange(SPointF position)
        {
            if (Width == null || Height == null)
            {
                throw new InvalidOperationException("Cannot arrange element without defined width and height, please call Measure first.");
            }

            var elementPosition = Element.Arrange(position, TODO, new SSizeF(Width ?? 0, Height ?? 0));
            X = elementPosition.X;
            Y = elementPosition.Y;

            foreach (var child in positionedChildren)
            {
                child.Arrange(elementPosition);
            }
        }

        public void AddChild(PositionedHeliumElement child)
        {
            positionedChildren.Add(child);
        }

        public void AddClass(string className)
        {
            if (!classes.Contains(className))
            {
                classes.Add(className);
            }
        }

        public bool HasClass(string className)
        {
            return classes.Contains(className);
        }

        public void RemoveClass(string className)
        {
            classes.Remove(className);
        }
    }
}
