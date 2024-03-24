using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    internal static class DictionaryHelper
    {
        public static void TryAddDictionary<TK, TV>(this IDictionary<TK, TV> dic, IDictionary<TK, TV>? other)
        {
            if (other == null) return;
            foreach (var kv in other)
            {
                if (dic.ContainsKey(kv.Key))
                {
                    throw new LightOrmException($"查询参数：{kv.Key} 重复");
                }
                dic.Add(kv.Key, kv.Value);
            }
        }

        public static void ForEach<TK, TV>(this ConcurrentDictionary<TK, TV> pairs, Action<TV> work) where TK : notnull
        {
            foreach (var item in pairs)
            {
                work.Invoke(item.Value);
            }
        }
        public static async Task ForEachAsync<TK, TV>(this ConcurrentDictionary<TK, TV> pairs, Func<TV, Task> work) where TK : notnull
        {
            foreach (var item in pairs)
            {
                await work.Invoke(item.Value);
            }
        }
    }
}
