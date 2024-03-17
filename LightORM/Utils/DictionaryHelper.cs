using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils
{
    internal static class DictionaryHelper
    {
        public static void TryAddDictionary<TK,TV>(this IDictionary<TK, TV> dic, IDictionary<TK, TV>? other)
        {
            if (other == null) return;
            foreach (var kv in other)
            {
                if (dic.ContainsKey(kv.Key))
                {
                    throw new ArgumentException($"查询参数：{kv.Key} 重复");
                }
                dic.Add(kv.Key, kv.Value);
            }
        }
    }
}
