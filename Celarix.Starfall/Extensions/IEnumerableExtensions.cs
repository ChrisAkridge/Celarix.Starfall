using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Extensions;

public static class IEnumerableExtensions
{
    public static BigInteger Sum<T>(this IEnumerable<T> enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable, nameof(enumerable));
        var result = new BigInteger(0);
        foreach (var item in enumerable)
        {
            if (item is null)
            {
                continue;
            }
            if (item is BigInteger bi)
            {
                result += bi;
            }
            else if (item is IConvertible convertible)
            {
                result += new BigInteger(convertible.ToDouble(null));
            }
            else
            {
                throw new InvalidOperationException($"Cannot sum items of type {typeof(T).FullName}.");
            }
        }
        return result;
    }

    public static BigInteger SumBy<T>(this IEnumerable<T> enumerable, Func<T, BigInteger> selector)
    {
        ArgumentNullException.ThrowIfNull(enumerable, nameof(enumerable));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        var result = new BigInteger(0);
        foreach (var item in enumerable)
        {
            if (item is null)
            {
                continue;
            }
            result += selector(item);
        }
        return result;
    }
}
