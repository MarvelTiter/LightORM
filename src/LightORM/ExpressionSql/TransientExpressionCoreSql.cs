using System.Collections.Concurrent;

namespace LightORM.ExpressionSql;

internal sealed class TransientExpressionCoreSql(string key, ISqlExecutor ado, ExpressionSqlOptions options) : ExpressionCoreSqlBase, ITransientExpressionContext
{
    private static readonly ConcurrentDictionary<string, WeakReference<TransientExpressionCoreSql>> weakCache = new();
    
    public override ISqlExecutor Ado { get; } = ado;

    public override ExpressionSqlOptions Options { get; } = options;
    public string Key { get; } = key;

    public static TransientExpressionCoreSql Create(string key, ISqlExecutor executor, ExpressionSqlOptions options)
    {
        // 尝试从缓存获取
        if (weakCache.TryGetValue(key, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var cachedInstance))
            {
                return cachedInstance;
            }
        }

        // 创建新实例并缓存
        var newInstance = new TransientExpressionCoreSql(key, executor, options);
        weakCache[key] = new WeakReference<TransientExpressionCoreSql>(newInstance);
        return newInstance;
    }
}