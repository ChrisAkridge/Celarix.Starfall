using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements.Containers
{
    /// <summary>
    /// Represents a container that can contain either one child element, or be split into two
    /// <see cref="BinaryElementContainer"/> children either horizontally or vertically. This is
    /// the fundamental building block of the Helium layout system.
    /// </summary>
    public sealed class BinaryElementContainer : HeliumElement
    {
        public enum SplitMode
        {
            Empty,
            SingleChild,
            Split
        }

        private HeliumElement? singleChild;
        private BinaryElementContainer? firstChild;
        private BinaryElementContainer? secondChild;
        private int? firstChildRatio;
        private int? secondChildRatio;
        private Direction? splitDirection;

        public override IReadOnlyList<HeliumElement> Children => singleChild != null
            ? [singleChild]
            : [firstChild!, secondChild!];

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

        public SplitMode CurrentMode => singleChild != null
            ? SplitMode.SingleChild
            : firstChild != null ? SplitMode.Split : SplitMode.Empty;
        public HeliumElement? SingleChild => singleChild;
        public BinaryElementContainer? FirstSplit => firstChild;
        public BinaryElementContainer? SecondSplit => secondChild;
        public (int FirstChildRatio, int SecondChildRatio)? SplitRatios => firstChildRatio.HasValue && secondChildRatio.HasValue
            ? (firstChildRatio.Value, secondChildRatio.Value)
            : null;
        public Direction? CurrentSplitDirection => splitDirection;

        /// <summary>
        /// Gets or sets the padding. This applies both in single-child and split modes.
        /// </summary>
        public Padding Padding { get; set; }

        /// <summary>
        /// Gets or sets the alignment for the children. This applies both in single-child and split modes.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.Center;

        public void SetSingleChild(HeliumElement? child)
        {
            if (firstChild != null || secondChild != null)
            {
                throw new InvalidOperationException("Cannot set a single child on a container that is currently split. Clear the existing split before setting a single child.");
            }

            singleChild = child;
        }

        public void SplitHorizontal(int topRatio, int bottomRatio) => Split(topRatio, bottomRatio, Direction.Horizontal);
        public void SplitVertical(int leftRatio, int rightRatio) => Split(leftRatio, rightRatio, Direction.Vertical);

        private void Split(int firstChildRatio, int secondChildRatio, Direction splitDirection)
        {
            if (singleChild != null)
            {
                throw new InvalidOperationException("Cannot split a container that already has a single child. Clear the single child before splitting.");
            }
            if (firstChild != null || secondChild != null)
            {
                throw new InvalidOperationException("Cannot split a container that is already split. Clear the existing split before splitting again.");
            }
            this.firstChildRatio = firstChildRatio;
            this.secondChildRatio = secondChildRatio;
            firstChild = new BinaryElementContainer();
            secondChild = new BinaryElementContainer();
            this.splitDirection = splitDirection;
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            ActualSize = availableSize;

            if (CurrentMode == SplitMode.Empty)
            {
                return;
            }

            if (CurrentMode == SplitMode.SingleChild)
            {
                singleChild!.MeasureSelf(Padding.GetInnerSize(ActualSize.Value), measurementService);
            }
            else
            {
                var totalRatio = firstChildRatio!.Value + secondChildRatio!.Value;
                if (splitDirection == Direction.Horizontal)
                {
                    var firstHeight = availableSize.Height * firstChildRatio.Value / totalRatio;
                    var secondHeight = availableSize.Height * secondChildRatio.Value / totalRatio;
                    firstChild!.MeasureSelf(Padding.GetInnerSize(new SSizeF(availableSize.Width, firstHeight)), measurementService);
                    secondChild!.MeasureSelf(Padding.GetInnerSize(new SSizeF(availableSize.Width, secondHeight)), measurementService);
                }
                else
                {
                    var firstWidth = availableSize.Width * firstChildRatio.Value / totalRatio;
                    var secondWidth = availableSize.Width * secondChildRatio.Value / totalRatio;
                    firstChild!.MeasureSelf(Padding.GetInnerSize(new SSizeF(firstWidth, availableSize.Height)), measurementService);
                    secondChild!.MeasureSelf(Padding.GetInnerSize(new SSizeF(secondWidth, availableSize.Height)), measurementService);
                }
            }
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            if (CurrentMode == SplitMode.Empty)
            {
                return;
            }

            if (CurrentMode == SplitMode.SingleChild)
            {
                var innerBounds = Padding.GetInnerRectForOuterRect(thisBounds);
                var childSize = singleChild!.ActualSize!.Value;
                singleChild.ActualPosition = AlignmentHelper.Align(Alignment, innerBounds, childSize);
                singleChild.ArrangeChildren(singleChild.ActualBounds!.Value);
            }
            else
            {
                var totalRatio = firstChildRatio!.Value + secondChildRatio!.Value;
                if (splitDirection == Direction.Horizontal)
                {
                    var firstHeight = thisBounds.Height * firstChildRatio.Value / totalRatio;
                    var secondHeight = thisBounds.Height * secondChildRatio.Value / totalRatio;
                    var firstBounds = Padding.GetInnerRectForOuterRect(new SRectF(thisBounds.X, thisBounds.Y, thisBounds.Width, firstHeight));
                    var secondBounds = Padding.GetInnerRectForOuterRect(new SRectF(thisBounds.X, thisBounds.Y + firstHeight, thisBounds.Width, secondHeight));
                    firstChild!.ArrangeChildren(firstBounds);
                    secondChild!.ArrangeChildren(secondBounds);
                }
                else
                {
                    var firstWidth = thisBounds.Width * firstChildRatio.Value / totalRatio;
                    var secondWidth = thisBounds.Width * secondChildRatio.Value / totalRatio;
                    var firstBounds = Padding.GetInnerRectForOuterRect(new SRectF(thisBounds.X, thisBounds.Y, firstWidth, thisBounds.Height));
                    var secondBounds = Padding.GetInnerRectForOuterRect(new SRectF(thisBounds.X + firstWidth, thisBounds.Y, secondWidth, thisBounds.Height));
                    firstChild!.ArrangeChildren(firstBounds);
                    secondChild!.ArrangeChildren(secondBounds);
                }
            }
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            if (CurrentMode == SplitMode.Empty)
            {
                return Array.Empty<IRenderable>();
            }

            if (CurrentMode == SplitMode.SingleChild)
            {
                return singleChild!.GetRenderables();
            }
            else
            {
                var renderables = new List<IRenderable>();
                renderables.AddRange(firstChild!.GetRenderables());
                renderables.AddRange(secondChild!.GetRenderables());
                return renderables;
            }
        }

        public override HeliumElement Clone()
        {
            return new BinaryElementContainer
            {
                Id = Id,
                Padding = Padding,
                Alignment = Alignment,
                singleChild = singleChild?.Clone(),
                firstChild = firstChild?.Clone() as BinaryElementContainer,
                secondChild = secondChild?.Clone() as BinaryElementContainer,
                firstChildRatio = firstChildRatio,
                secondChildRatio = secondChildRatio,
                splitDirection = splitDirection
            };
        }
    }
}
