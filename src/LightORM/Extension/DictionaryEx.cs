using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

internal static class DictionaryEx
{
#if NET462
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey name, TValue value)
    {
        if (!dic.ContainsKey(name))
        {
            dic.Add(name, value);
            return true;
        }
        return false;
    }
#endif
}
