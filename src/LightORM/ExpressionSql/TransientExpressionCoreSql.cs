using System.Collections.Concurrent;
namespace LightORM.ExpressionSql;

internal sealed class TransientExpressionCoreSql(string key, ISqlExecutor ado) : ExpressionCoreSqlBase, ITransientExpressionContext
{
    private static readonly ConcurrentDictionary<string, WeakReference<TransientExpressionCoreSql>> weakCache = new();
    private readonly ISqlExecutor ado = ado;
    public override ISqlExecutor Ado => ado;
    public string Key { get; } = key;
    public static TransientExpressionCoreSql Create(string key, ISqlExecutor executor)
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
        var newInstance = new TransientExpressionCoreSql(key, executor);
        weakCache[key] = new WeakReference<TransientExpressionCoreSql>(newInstance);
        return newInstance;
    }

}
