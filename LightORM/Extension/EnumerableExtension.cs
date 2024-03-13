using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Extension;

internal static class EnumerableExtension
{
    internal static IEnumerable<T> ForEach<T>(this IEnumerable<T> self, Func<T, T> func)
    {
        foreach (var item in self)
        {
            yield return func.Invoke(item);
        }
    }
}
