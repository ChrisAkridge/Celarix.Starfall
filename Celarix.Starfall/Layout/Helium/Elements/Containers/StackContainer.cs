using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements.Containers
{
    public sealed class StackContainer : HeliumElement
    {
        // TODO: replace with a tuple (HeliumElement?, int) and remove the sizeRatios list, since they have to be kept in sync and this way we can guarantee that they are.
        private List<HeliumElement?> children;
        private List<int> sizeRatios;
        private Direction direction;

        public override IReadOnlyList<HeliumElement> Children => [.. children
            .Where(c => c != null)
            .Cast<HeliumElement>()];

        public StackContainer(Direction direction, params IEnumerable<int> sizeRatios)
        {
            this.direction = direction;

            var sizeRatioList = sizeRatios.ToList();
            children = [.. Enumerable.Repeat<HeliumElement?>(null, sizeRatioList.Count)];
            this.sizeRatios = sizeRatioList;
        }

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;
        public Direction Direction => direction;
        public int Divisions => children.Count;

        /// <summary>
        /// Gets or sets the padding for each child element in the stack. This applies to all children;
        /// if you want padding on a single child, wrap it in a <see cref="SingleElementContainer"/> and
        /// set the padding on that.
        /// </summary>
        public Padding Padding { get; set; }

        /// <summary>
        /// Gets or sets the alignment for each child element in the stack. This applies to all children;
        /// if you want different alignments for different children, wrap each child in a <see cref="SingleElementContainer"/>
        /// and set the alignment on those.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.Center;

        public HeliumElement? GetChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {children.Count - 1}.");
            }
            return children[index];
        }

        public void SetChild(int index, HeliumElement? child)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {children.Count - 1}.");
            }
            children[index] = child;
        }

        public void AddChild(HeliumElement? child, int sizeRatio = 1)
        {
            children.Add(child);
            sizeRatios.Add(sizeRatio);
        }

        public void AddEmpty(int sizeRatio = 1)
        {
            AddChild(null, sizeRatio);
        }

        public void InsertChild(int index, HeliumElement? child, int sizeRatio = 1)
        {
            if (index < 0 || index > children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {children.Count}.");
            }
            children.Insert(index, child);
            sizeRatios.Insert(index, sizeRatio);
        }

        public void RemoveChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {children.Count - 1}.");
            }
            children.RemoveAt(index);
            sizeRatios.RemoveAt(index);
        }

        public override void MeasureSelf(SSizeF availableSize)
        {
            ActualSize = availableSize;

            if (children.Count == 0)
            {
                return;
            }

            var splitAxisSize = direction == Direction.Horizontal ? availableSize.Width : availableSize.Height;
            var totalParts = sizeRatios.Sum();
            var sizePerPart = splitAxisSize / totalParts;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    var cellOuterSize = direction == Direction.Horizontal
                        ? new SSizeF(sizePerPart * sizeRatios[i], availableSize.Height)
                        : new SSizeF(availableSize.Width, sizePerPart * sizeRatios[i]);
                    var cellInnerSize = Padding.GetInnerSize(cellOuterSize);
                    child.MeasureSelf(cellInnerSize);
                }
            }
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            var thisSize = ActualSize!.Value;

            if (children.Count == 0)
            {
                return;
            }

            var splitAxisSize = direction == Direction.Horizontal ? thisSize.Width : thisSize.Height;
            var totalParts = sizeRatios.Sum();
            var sizePerPart = splitAxisSize / totalParts;
            var currentOffset = 0d;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    if (direction == Direction.Horizontal)
                    {
                        var cellOuterBounds = new SRectF(
                            thisBounds.X + currentOffset,
                            thisBounds.Y,
                            sizePerPart * sizeRatios[i],
                            thisBounds.Height);
                        var cellInnerBounds = Padding.GetInnerRectForOuterRect(cellOuterBounds);
                        child.ActualPosition = AlignmentHelper.Align(Alignment, cellInnerBounds, child.ActualSize!.Value);
                    }
                    else if (direction == Direction.Vertical)
                    {
                        var cellOuterBounds = new SRectF(
                            thisBounds.X,
                            thisBounds.Y + currentOffset,
                            thisBounds.Width,
                            sizePerPart * sizeRatios[i]);
                        var cellInnerBounds = Padding.GetInnerRectForOuterRect(cellOuterBounds);
                        child.ActualPosition = AlignmentHelper.Align(Alignment, cellInnerBounds, child.ActualSize!.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported direction: {direction}");
                    }

                    child.ArrangeChildren(child.ActualBounds!.Value);
                }
                currentOffset += sizePerPart * sizeRatios[i];
            }
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            var renderables = new List<IRenderable>();

            var splitAxisSize = direction == Direction.Horizontal ? ActualBounds!.Value.Width : ActualBounds!.Value.Height;
            var totalParts = sizeRatios.Sum();
            var sizePerPart = splitAxisSize / totalParts;
            var currentOffset = 0d;

            for (int i = 0; i < children.Count; i++)
            {
                var sizeRatio = sizeRatios[i];
                var cellOuterBounds = direction == Direction.Horizontal
                    ? new SRectF(
                        ActualBounds!.Value.X + currentOffset,
                        ActualBounds!.Value.Y,
                        sizePerPart * sizeRatio,
                        ActualBounds!.Value.Height)
                    : new SRectF(
                        ActualBounds!.Value.X,
                        ActualBounds!.Value.Y + currentOffset,
                        ActualBounds!.Value.Width,
                        sizePerPart * sizeRatio);
                currentOffset += sizePerPart * sizeRatio;

                // Debug mode
                renderables.Add(new RectangleRenderable
                {
                    Bounds = cellOuterBounds,
                    Color = new SColor(255, 0, 0, 128),
                    PaintStyle = SPaintStyle.Stroke
                });
                renderables.Add(new TextRenderable
                {
                    Bounds = cellOuterBounds,
                    Color = new SColor(0, 255, 0, 255),
                    Text = $"Size {sizeRatio}",
                    Font = new SFontFamily("Arial", 12f),
                    DrawDirectly = true,
                    Alignment = Alignment.TopLeft,
                    Rotation = SAngle.Zero
                });

                HeliumElement? child = children[i];
                if (child != null)
                {
                    renderables.AddRange(child.GetRenderables());
                }
            }
            return renderables;
        }

        public override HeliumElement Clone()
        {
            var clone = new StackContainer(direction, sizeRatios)
            {
                Id = Id,
                Padding = Padding,
                Alignment = Alignment
            };
            
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var sizeRatio = sizeRatios[i];
                clone.AddChild(child?.Clone(), sizeRatio);
            }

            return clone;
        }
    }
}
