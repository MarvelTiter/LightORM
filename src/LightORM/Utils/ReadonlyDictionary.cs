using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils;

public class ReadonlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    private readonly Entry[] _entries;
    private readonly int _mask; // 用于将哈希码快速映射到索引
    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }
    }

    public IEnumerable<TKey> Keys => _entries.Where(e => e.IsOccupied).Select(e => e.Key);

    public IEnumerable<TValue> Values => _entries.Where(e => e.IsOccupied).Select(e => e.Value);

    public int Count { get; }
    public ReadonlyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var keyValuePairs = source.ToArray();
        Count = keyValuePairs.Length;

        // 1. 确定哈希桶的大小（取大于元素数量的最小2的n次幂）
        int bucketSize = CalculateSize(Count);
        _mask = bucketSize - 1;
        _entries = new Entry[bucketSize];

        // 2. 预计算每个键的哈希码并插入到数组中
        foreach (var kvp in keyValuePairs)
        {
            var key = kvp.Key ?? throw new ArgumentException("Key cannot be null.");
            int hashCode = key.GetHashCode() & 0x7FFFFFFF; // 确保为正数
            int index = hashCode & _mask;

            // 线性探测，寻找空位或已删除的位置
            while (_entries[index].IsOccupied)
            {
                index = (index + 1) & _mask; // 循环回到开头
            }

            _entries[index] = new Entry(key, kvp.Value, hashCode, occupied: true);
        }
    }
    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var entry in _entries)
        {
            if (entry.IsOccupied)
            {
                yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        int hashCode = key.GetHashCode() & 0x7FFFFFFF;
        int index = hashCode & _mask;

        // 线性探测查找
        while (_entries[index].IsOccupied)
        {
            ref Entry entry = ref _entries[index];
            // 首先比较哈希码，如果不同肯定不是目标键，可以快速跳过
            // 如果哈希码相同，再使用 Equals 比较，确保正确性
            if (entry.HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
            index = (index + 1) & _mask;
        }

        value = default!;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 计算大于 minSize 的最小 2 的 n 次幂
    /// 例如：输入 10，返回 16；输入 16，返回 16；输入 17，返回 32。
    /// </summary>
    private static int CalculateSize(int minSize)
    {
        // 确保至少有一定的空间减少哈希冲突
        minSize = Math.Max(minSize, 7);
        int size = 1;
        while (size < minSize)
        {
            size <<= 1; // 等同于 size *= 2
            if (size <= 0) // 溢出检查
            {
                throw new InvalidOperationException("Dictionary too large.");
            }
        }
        return size;
    }

    // 内部结构，存储键值对信息
    private readonly struct Entry(TKey key, TValue value, int hashCode, bool occupied)
    {
        public readonly TKey Key = key;
        public readonly TValue Value = value;
        public readonly int HashCode = hashCode;
        public readonly bool IsOccupied = occupied;
    }
}
