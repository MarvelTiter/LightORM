using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Cache
{
    internal static class StaticCache<T>
    {
        private readonly static ConcurrentDictionary<string, Lazy<T>> caches;
        static StaticCache()
        {
            caches = new();
        }

        public static T GetOrAdd(string key, Func<T> func)
        {
            return caches.GetOrAdd(key, new Lazy<T>(func)).Value;
        }

        public static T? Get(string key)
        {
            return caches.TryGetValue(key, out var lazy) ? lazy.Value : default;
        }

        public static int Count => caches.Count;
    }
}
