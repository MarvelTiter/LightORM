using System.Collections.Generic;

namespace MDbContext.NewExpSql.SqlFragment
{
    /// <summary>
    /// 泛型类缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class FragmentCache<T> where T : BaseFragment
    {
        internal static Dictionary<string, T> cache = new Dictionary<string, T>();
        internal static T GetPart(string key)
        {
            if (cache.TryGetValue(key, out var value))
            {
                return value;
            }
            return default;
        }
        internal static void AddCache(string key, T value)
        {
            cache.Add(key, value);
        }
    }
}
