using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Extension {
    public static class DictionaryEx {
        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value) {
            if (ContainsKey(self, key, out TKey k)) {
                self[k] = value;
            } else
                self.Add(key, value);
        }

        public static bool TryGet<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, out TValue value) {
            if (ContainsKey(self, key, out TKey k)) {
                value = self[k];
                return true;
            }
            value = default;
            return false;
        }

        private static bool ContainsKey<TKey, TValue>(IDictionary<TKey, TValue> collection, TKey key, out TKey key1) {
            if (key is IEquatable<TKey>) {
                var k = (IEquatable<TKey>)key;
                foreach (var item in collection.Keys) {
                    if (k.Equals(item)) {
                        key1 = item;
                        return true;
                    }
                }
            } else {
                foreach (var item in collection.Keys) {
                    if (key.Equals(item)) {
                        key1 = item;
                        return true;
                    }
                }
            }


            key1 = default;
            return false;
        }
    }
}
