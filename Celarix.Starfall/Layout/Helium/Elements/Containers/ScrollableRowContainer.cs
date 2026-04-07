using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements.Containers
{
    /// <summary>
    /// A container holding multiple children laid out in a row with an optional margin between them.
    /// A particular index can be picked as scrolled into position, and that position can be on the left,
    /// right, or center of the container. The container will scroll as necessary to keep the scrolled element
    /// in the correct position, even if space before or after the element is visible.
    /// </summary>
    // TODO: untested
    public sealed class ScrollableRowContainer : HeliumElement
    {
        private readonly List<HeliumElement> children = [];
        private int scrolledElementIndex = 0;
        private HAlignment scrolledElementAlignment = HAlignment.Center;

        public override IReadOnlyList<HeliumElement> Children => children;

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

        /// <summary>
        /// Gets or sets the margin between each child, measured in units of multiples of this container's height.
        /// For example, if this container has a height of 1, and the ChildrenMargin is set to 0.5, then there will
        /// be 0.5 container-heights between each child.
        /// </summary>
        public double ChildrenMargin { get; set; } = 0;

        public void AddChild(HeliumElement child)
        {
            children.Add(child);
        }

        public void AddChildren(IEnumerable<HeliumElement> children)
        {
            this.children.AddRange(children);
        }

        public bool RemoveChild(HeliumElement child)
        {
            return children.Remove(child);
        }

        public bool RemoveChildAt(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                return false;
            }
            children.RemoveAt(index);
            return true;
        }

        public void ClearChildren()
        {
            children.Clear();
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            // Containers are always as large as they can be.
            ActualSize = availableSize;

            // We don't actually know the available width for each child, though. We do know the height, and
            // the question we want to ask each child is "if you were given the full height, how wide would
            // you want to be?" But all we have is MeasureSelf(SSizeF), which means we have to give them some
            // width. In fact, it gets even worse - many children will always report that they fill their entire
            // available width... which we have as much of as we want. So here's what we'll do - give the child
            // a 1000x1000 area to measure itself in, and see what aspect ratio it wants to be. Oddly, this means
            // children that measure themselves as wide but not very tall will end up being bigger in the actual
            // container.
            var falseSize = new SSizeF(1000, 1000);
            foreach (var child in children)
            {
                child.MeasureSelf(falseSize, measurementService);
                var aspectRatio = child.ActualSize!.Value.Width / child.ActualSize!.Value.Height;
                var desiredWidth = aspectRatio * availableSize.Height;
                child.MeasureSelf(new SSizeF(desiredWidth, availableSize.Height), measurementService);
            }
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            // First, figure out where each child is within the container if we were to lay them out in a row with the specified margin.
            var singleMarginSize = new SSizeF(ChildrenMargin * ActualSize!.Value.Height, ActualSize!.Value.Height);
            var childrenInternalXPositions = new List<double>();
            var currentInternalX = 0d;
            foreach (var child in Children)
            {
                var childSize = child.ActualSize!.Value;
                childrenInternalXPositions.Add(currentInternalX);
                currentInternalX += childSize.Width + singleMarginSize.Width;
            }
            
            // Remove the last margin.
            // Next, figure out the scrolled element's alignment point within the container. This is
            // the point that we want to be at the correct position within the container, and we'll
            // scroll the container as necessary to make sure it is.
            var scrolledElementInternalX = childrenInternalXPositions[scrolledElementIndex];
            var scrolledElementInternalWidth = Children[scrolledElementIndex].ActualSize!.Value.Width;
            var scrolledElementAlignmentPoint = scrolledElementInternalX + scrolledElementAlignment switch
            {
                HAlignment.Left => 0,
                HAlignment.Center => scrolledElementInternalWidth / 2,
                HAlignment.Right => scrolledElementInternalWidth,
                _ => throw new InvalidOperationException($"Invalid alignment: {scrolledElementAlignment}")
            };

            // Next, figure out where that alignment point will be placed in the container.
            var containerAlignmentPoint = scrolledElementAlignment switch
            {
                HAlignment.Left => thisBounds.Left,
                HAlignment.Center => thisBounds.Left + thisBounds.Width / 2,
                HAlignment.Right => thisBounds.Right,
                _ => throw new InvalidOperationException($"Invalid alignment: {scrolledElementAlignment}")
            };

            // Finally, figure out the offset for each child such that the scrolled element's alignment
            // point is at the container's alignment point, and arrange each child accordingly.
            var alignmentOffset = containerAlignmentPoint - scrolledElementAlignmentPoint;
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var childInternalX = childrenInternalXPositions[i];
                var childSize = child.ActualSize!.Value;
                var childBounds = new SRectF(
                    thisBounds.Left + childInternalX + alignmentOffset,
                    thisBounds.Top,
                    childSize.Width,
                    childSize.Height);
                child.ArrangeChildren(childBounds);
            }
        }

        public override HeliumElement Clone()
        {
            var clone = new ScrollableRowContainer
            {
                scrolledElementIndex = scrolledElementIndex,
                scrolledElementAlignment = scrolledElementAlignment,
                ChildrenMargin = ChildrenMargin,
                Id = Id
            };

            foreach (var child in Children)
            {
                clone.AddChild(child.Clone());
            }

            return clone;
        }

        public override IReadOnlyList<IRenderable> GetRenderables(MeasurementService measurementService)
        {
            // We don't need to show anything fully outside of the container's actual bounds.
            var renderables = new List<IRenderable>();
            foreach (var child in Children)
            {
                var bounds = child.ActualBounds;
                if (SRectF.Intersects(bounds!.Value, ActualBounds!.Value))
                {
                    renderables.AddRange(child.GetRenderables(measurementService));
                }
            }

            return renderables;
        }
    }
}
