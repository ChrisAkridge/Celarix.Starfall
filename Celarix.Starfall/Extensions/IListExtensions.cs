using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions;

public static class IListExtensions
{
    public static bool HasDuplicates<T>(this IList<T> list)
    {
        var set = new HashSet<T>();
        foreach (var item in list)
        {
            if (!set.Add(item))
            {
                return true; // Duplicate found
            }
        }
        return false; // No duplicates
    }
}
