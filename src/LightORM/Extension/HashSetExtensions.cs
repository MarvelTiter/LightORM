using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

internal static class HashSetExtensions
{
    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> values)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(values);
#else
        if (set == null) throw new ArgumentNullException(nameof(set));
        if (values == null) throw new ArgumentNullException(nameof(values));
#endif
        set.UnionWith(values);
    }
}
