using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

internal static class EnumerableEx
{
    public static object? GetValueByIndex(this IEnumerable e, int index)
    {
        return e.Cast<object>().ElementAt(index);
    }
}
