using LightORM.AssemblyControl;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace LightORM.Utils;

internal static class StringBuilderPool
{
    private static int POOL_CAPACITY => ExpressionSqlOptions.Instance.Value.InternalObjectPoolSize;
    private const int BUILDER_CAPACITY = 4096;
    private static readonly ConcurrentQueue<StringBuilder> pool = [];
    private static int itemCount = 0;
    [ThreadStatic]
    private static StringBuilder? fast;
    public static IDisposable Get(out StringBuilder builder, int capacity = 128)
    {
        if (!BenchmarkConfig.UseStringBuilderPool)
        {
            builder = new StringBuilder(capacity);
            return new BuilderWrap(null);
        }
        var item = fast;

        if (item is not null)
        {
            fast = null;
            builder = item;
            return new BuilderWrap(builder);
        }
        if (pool.TryDequeue(out item))
        {
            Interlocked.Decrement(ref itemCount);
            builder = item;
            return new BuilderWrap(builder);
        }
        item = new StringBuilder(capacity);
        builder = item;
        return new BuilderWrap(builder);


    }

    public static void Return(StringBuilder item)
    {
        if (item == null) return;

        item.Clear();
        if (item.Capacity > BUILDER_CAPACITY)  // 超过 4096 就不缓存
        {
            return;
        }
        // 快速路径尝试
        if (fast == null)
        {
            fast = item;
            return;
        }
        if (Interlocked.Increment(ref itemCount) <= POOL_CAPACITY)
        {
            pool.Enqueue(item);
            return;
        }
        Interlocked.Decrement(ref itemCount);
    }

    private class BuilderWrap(StringBuilder? builder) : IDisposable
    {
        public void Dispose()
        {
            if (builder == null) return;
            Return(builder);
        }
    }
}
