using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Cache
{
    internal static class StaticCache<T>
    {
        private readonly static ConcurrentDictionary<string, T> caches;
        static StaticCache()
        {
            caches = new();
        }

        public static T GetOrAdd(string key, Func<string, T> func)
        {
            return caches.GetOrAdd(key, func);
        }

        public static T GetOrAdd(string key, Func<T> func)
        {
            return caches.GetOrAdd(key, k => func());
        }
        public static bool HasKey(string key)
        {
            return caches.ContainsKey(key);
        }
        public static T? Get(string key)
        {
            return caches.TryGetValue(key, out var val) ? val : default;
        }

        public static int Count => caches.Count;

        public static IEnumerable<T> Values => caches.Values;
    }
}
