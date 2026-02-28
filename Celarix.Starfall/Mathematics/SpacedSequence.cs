using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Mathematics
{
    public sealed class SpacedSequence<T>
        where T : IAdditionOperators<T, T, T>,
            ISubtractionOperators<T, T, T>,
            IComparisonOperators<T, T, bool>
    {
        private readonly T baseValue;
        private readonly T spacing;
        private T currentLeft;
        private T currentRight;

        public T CurrentLeft => currentLeft;
        public T CurrentRight => currentRight;

        public SpacedSequence(T baseValue, T spacing)
        {
            this.baseValue = baseValue;
            this.spacing = spacing;
            currentLeft = baseValue;
            currentRight = baseValue;
        }

        public void MoveLeft()
        {
            currentLeft -= spacing;
        }

        public void MoveRight()
        {
            currentRight += spacing;
        }

        public static IEnumerable<T> AllBetween(T baseValue, T spacing, T min, T max)
        {
            // 1. Move baseValue until it's within the range
            //    (if it's already within the range, this loop won't execute)
            while (baseValue < min)
            {
                baseValue += spacing;
            }
            while (baseValue > max)
            {
                baseValue -= spacing;
            }

            // 2. Generate the points left of baseValue, if any
            var left = baseValue - spacing;
            while (left >= min)
            {
                yield return left;
                left -= spacing;
            }

            // 3. Yield baseValue itself
            yield return baseValue;

            // 4. Generate the points right of baseValue, if any
            var right = baseValue + spacing;
            while (right <= max)
            {
                yield return right;
                right += spacing;
            }
        }
    }
}
