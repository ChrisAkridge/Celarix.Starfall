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
        public double MajorAxisPosition => _majorAxisPosition;

        public LayoutStacker(Direction direction, double baseMargin, double marginStepMultiplier)
        {
            baseMargin.ThrowIfNotPositive(nameof(baseMargin));
            marginStepMultiplier.ThrowIfNotPositive(nameof(marginStepMultiplier));

            Direction = direction;
            BaseMargin = baseMargin;
            MarginStepMultiplier = marginStepMultiplier;
        }

        /// <summary>
        /// Places an object of the specified size at the current major axis position and the specified
        /// minor axis position, then advances the major axis position by the size of the object plus
        /// a margin determined by the following margin step.
        /// </summary>
        /// <param name="objectSize">The size of the object to place.</param>
        /// <param name="minorAxisPosition">The position along the minor axis at which to place the object.</param>
        /// <param name="followingMarginStep">The step used to calculate the margin after placing the object.</param>
        /// <returns>The rectangle representing the placed object's bounds.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public SRectF Place(SSizeF objectSize, double minorAxisPosition, int followingMarginStep)
        {
            objectSize.ThrowIfNotPositive(nameof(objectSize));

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

        /// <summary>
        /// Places an object of the specified size at a position along the minor axis that is aligned within the bounds defined by
        /// <paramref name="minorAxisMin"/> and <paramref name="minorAxisMax"/> according to the specified <paramref name="alignment"/>.
        /// </summary>
        /// <param name="objectSize">The size of the object to place.</param>
        /// <param name="minorAxisMin">The minimum bound along the minor axis.</param>
        /// <param name="minorAxisMax">The maximum bound along the minor axis.</param>
        /// <param name="alignment">The alignment within the bounds.</param>
        /// <param name="followingMarginStep">The step used to calculate the margin after placing the object.</param>
        /// <returns>The rectangle representing the placed object's bounds.</returns>
        public SRectF Place(SSizeF objectSize, double minorAxisMin, double minorAxisMax, Alignment alignment,
            int followingMarginStep)
        {
            double minorAxisPosition = AlignOnBoundedMinorAxis(objectSize, minorAxisMin, minorAxisMax, alignment);
            return Place(objectSize, minorAxisMin + minorAxisPosition, followingMarginStep);
        }

        public SRectF PlaceWithNoMargin(SSizeF objectSize, double minorAxisMin, double minorAxisMax, Alignment alignment)
        {
            double minorAxisPosition = AlignOnBoundedMinorAxis(objectSize, minorAxisMin, minorAxisMax, alignment);
            return Place(objectSize, minorAxisMin + minorAxisPosition, 0);
        }

        /// <summary>
        /// Places multiple objects at the same major axis position, each with its own size and minor
        /// axis position, then advances the major axis position by the largest object size plus a margin
        /// determined by the following margin step.
        /// </summary>
        /// <param name="objectSizesAndMinorAxisPositions">A list of tuples containing the size and minor axis position of each object to place.</param>
        /// <param name="followingMarginStep">The step used to calculate the margin after placing the objects.</param>
        /// <returns>A list of rectangles representing the placed objects' bounds.</returns>
        /// <exception cref="ArgumentException">Thrown if the list of object sizes and minor axis positions is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the direction is invalid.</exception>
        public IReadOnlyList<SRectF> PlaceAtSameMajorPosition(IReadOnlyList<(SSizeF Size, double MinorAxisPosition)> objectSizesAndMinorAxisPositions,
            int followingMarginStep)
        {
            if (objectSizesAndMinorAxisPositions.Count == 0)
            {
                throw new ArgumentException("The list of object sizes and minor axis positions cannot be empty.",
                    nameof(objectSizesAndMinorAxisPositions));
            }

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

        /// <summary>
        /// Aligns an object of the specified size within the bounds defined by <paramref name="minorAxisMin"/> and <paramref name="minorAxisMax"/>
        /// </summary>
        /// <param name="objectSize">The size of the object to align.</param>
        /// <param name="minorAxisMin">The minimum bound of the minor axis.</param>
        /// <param name="minorAxisMax">The maximum bound of the minor axis.</param>
        /// <param name="alignment">The alignment of the object within the bounds.</param>
        /// <returns>The position of the object along the minor axis.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minorAxisMax"/> is less than or equal to <paramref name="minorAxisMin"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the direction is invalid.</exception>
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

        /// <summary>
        /// Advances the major axis position by a margin determined by the specified margin step.
        /// </summary>
        /// <param name="marginStep">The step used to calculate the margin size.</param>
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
