using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class StringBuilderExtensions
    {
        public static void AddIfNotEmpty(this StringBuilder builder, IList<string> list, bool clearAfterAdd = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(list);

            if (builder.Length > 0)
            {
                list.Add(builder.ToString());
                if (clearAfterAdd)
                {
                    builder.Clear();
                }
            }
        }
    }
}
