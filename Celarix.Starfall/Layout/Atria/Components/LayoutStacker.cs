using Celarix.Starfall.Extensions;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Components
{
    public sealed class LayoutStacker
    {
        private double _majorAxisPosition = 0d;

        public Direction Direction { get; }
        public double BaseMargin { get; }
        public double MarginStepMultiplier { get; }

        public LayoutStacker(Direction direction, double baseMargin, double marginStepMultiplier)
        {
            baseMargin.ThrowIfNotPositive(nameof(baseMargin));
            marginStepMultiplier.ThrowIfNotPositive(nameof(marginStepMultiplier));

            Direction = direction;
            BaseMargin = baseMargin;
            MarginStepMultiplier = marginStepMultiplier;
        }

        public SRectF Place(SSizeF objectSize, double minorAxisPosition, int followingMarginStep)
        {
            objectSize.ThrowIfNotPositive(nameof(objectSize));
            followingMarginStep.ThrowIfNotPositive(nameof(followingMarginStep));

            SPointF position = Direction switch
            {
                Direction.Vertical => new(minorAxisPosition, _majorAxisPosition),
                Direction.Horizontal => new(_majorAxisPosition, minorAxisPosition),
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            };

            var majorAxisSize = Direction switch
            {
                Direction.Vertical => objectSize.Height,
                Direction.Horizontal => objectSize.Width,
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            };

            var minorAxisSize = Direction switch
            {
                Direction.Vertical => objectSize.Width,
                Direction.Horizontal => objectSize.Height,
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            };

            var rectangleSize = Direction switch
            {
                Direction.Vertical => new SSizeF(minorAxisSize, majorAxisSize),
                Direction.Horizontal => new SSizeF(majorAxisSize, minorAxisSize),
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            };

            var marginSize = GetMarginSize(followingMarginStep);
            _majorAxisPosition += majorAxisSize + marginSize;
            return new SRectF(position, rectangleSize);
        }

        public SRectF Place(SSizeF objectSize, double minorAxisMin, double minorAxisMax, Alignment alignment,
            int followingMarginStep)
        {
            double minorAxisPosition = AlignOnBoundedMinorAxis(objectSize, minorAxisMin, minorAxisMax, alignment);
            return Place(objectSize, minorAxisMin + minorAxisPosition, followingMarginStep);
        }

        public IReadOnlyList<SRectF> PlaceAtSameMajorPosition(IReadOnlyList<(SSizeF Size, double MinorAxisPosition)> objectSizesAndMinorAxisPositions,
            int followingMarginStep)
        {
            if (objectSizesAndMinorAxisPositions.Count == 0)
            {
                throw new ArgumentException("The list of object sizes and minor axis positions cannot be empty.",
                    nameof(objectSizesAndMinorAxisPositions));
            }
            followingMarginStep.ThrowIfNotPositive(nameof(followingMarginStep));

            var largestOnMajorAxis = objectSizesAndMinorAxisPositions.Select(s => Direction switch
            {
                Direction.Vertical => s.Size.Height,
                Direction.Horizontal => s.Size.Width,
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            }).Max();

            var rectangles = new List<SRectF>(objectSizesAndMinorAxisPositions.Count);
            foreach (var (size, minorAxisPos) in objectSizesAndMinorAxisPositions)
            {
                var position = new SPointF(
                    Direction is Direction.Vertical ? minorAxisPos : _majorAxisPosition,
                    Direction is Direction.Vertical ? _majorAxisPosition : minorAxisPos);
                rectangles.Add(new(position, size));
            }

            var marginSize = GetMarginSize(followingMarginStep);
            _majorAxisPosition += largestOnMajorAxis + marginSize;
            return rectangles;
        }

        public double AlignOnBoundedMinorAxis(SSizeF objectSize, double minorAxisMin, double minorAxisMax, Alignment alignment)
        {
            if (minorAxisMax <= minorAxisMin)
            {
                throw new ArgumentOutOfRangeException(nameof(minorAxisMax), minorAxisMax,
                    "Minor axis max must be greater than minor axis min.");
            }
            objectSize.ThrowIfNotPositive(nameof(objectSize));

            alignment = alignment switch
            {
                Alignment.TopLeft => Alignment.LeftCenter,
                Alignment.TopCenter => Alignment.Center,
                Alignment.TopRight => Alignment.RightCenter,
                Alignment.LeftCenter => Alignment.LeftCenter,
                Alignment.Center => Alignment.Center,
                Alignment.RightCenter => Alignment.RightCenter,
                Alignment.BottomLeft => Alignment.LeftCenter,
                Alignment.BottomCenter => Alignment.Center,
                Alignment.BottomRight => Alignment.RightCenter,
                _ => throw new InvalidOperationException($"Invalid alignment {alignment}.")
            };
            var minorAxisSize = Direction switch
            {
                Direction.Vertical => objectSize.Width,
                Direction.Horizontal => objectSize.Height,
                _ => throw new InvalidOperationException($"Invalid direction {Direction}.")
            };

            var minorAxisPosition = AlignmentHelper.AlignAxis((minorAxisMax - minorAxisMin), minorAxisSize, alignment);
            return minorAxisPosition;
        }

        public void PlaceMargin(int marginStep)
        {
            marginStep.ThrowIfNotPositive(nameof(marginStep));
            var marginSize = GetMarginSize(marginStep);
            _majorAxisPosition += marginSize;
        }

        private double GetMarginSize(int marginStep)
        {
            return Math.Pow(MarginStepMultiplier, marginStep) * BaseMargin;
        }
    }
}
