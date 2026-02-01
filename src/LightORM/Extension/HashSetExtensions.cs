using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

internal static class HashSetExtensions
{
    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T>? values)
    {
        if (values is null)
        {
            return;
        }
        set.UnionWith(values);
    }
}
