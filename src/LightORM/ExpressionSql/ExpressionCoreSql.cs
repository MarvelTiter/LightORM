using LightORM.Providers;
using System;
using System.Collections.Concurrent;
using System.Threading;
namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql : ExpressionCoreSqlBase, IExpressionContext, IDisposable
{
    public ExpressionSqlOptions Options { get; }
    internal SqlExecutorProvider executorProvider;
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(Options.DefaultDbKey);
    public ExpressionCoreSql(ExpressionSqlOptions option)
    {
#if DEBUG
        Console.WriteLine($"创建ExpressionCoreSql：{DateTime.Now}");
#endif
        executorProvider = new SqlExecutorProvider(option);
        Options = option;
    }

    public ITransientExpressionContext SwitchDatabase(string key)
    {
        var ado = executorProvider.GetSqlExecutor(key);
        return ExpressionCoreSqlWithKey.Create(key, ado);
    }

    public string? CreateTableSql<T>(Action<TableGenerateOption>? action = null)
    {
        var ado = Ado;
        try
        {
            return string.Join(Environment.NewLine, GenerateDbTable<T>(ado, Options, action));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CreateTableAsync<T>(Action<TableGenerateOption>? action = null, CancellationToken cancellationToken = default)
    {
        var ado = Ado;
        try
        {
            var sqls = GenerateDbTable<T>(ado, Options, action);
            if (!sqls.Any())
            {
                return false;
            }
            ado.BeginTransaction();
            foreach (var s in sqls)
            {
                await ado.ExecuteNonQueryAsync(s, cancellationToken: cancellationToken);
            }
            await ado.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            await ado.RollbackTransactionAsync(cancellationToken);
            return false;
        }
    }
    private static IEnumerable<string> GenerateDbTable<T>(ISqlExecutor ado, ExpressionSqlOptions option, Action<TableGenerateOption>? action = null)
    {
        if (ado.Database.TableHandler is not null)
        {
            var o = option.TableGenOption;
            if (action != null)
            {
                o = (TableGenerateOption)o.Clone();
                action(o);
            }
            var handler = ado.Database.TableHandler.Invoke(o);
            var tableSql = handler.GenerateDbTable<T>();
            return tableSql;
        }
        return [];
    }
    public IExpSelect Select(string tableName) => throw new NotImplementedException();//new SelectProvider0(tableName, Ado);


    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
#if DEBUG
                Console.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
#endif
                executorProvider.Dispose();
            }

            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


internal sealed class ExpressionCoreSqlWithKey(string key, ISqlExecutor ado) : ExpressionCoreSqlBase, ITransientExpressionContext
{
    private static readonly ConcurrentDictionary<string, WeakReference<ExpressionCoreSqlWithKey>> weakCache =
        new();
    private readonly ISqlExecutor ado = ado;
    public override ISqlExecutor Ado => ado;
    public string Key { get; } = key;
    public static ExpressionCoreSqlWithKey Create(string key, ISqlExecutor executor)
    {
        // 尝试从缓存获取
        if (weakCache.TryGetValue(key, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var cachedInstance))
            {
                return cachedInstance;
            }
            //else
            //{
            //    var newInstance = new ExpressionCoreSqlWithKey(executor);
            //    weakRef.SetTarget(newInstance);
            //    return newInstance;
            //}
        }

        // 创建新实例并缓存
        var newInstance = new ExpressionCoreSqlWithKey(key, executor);
        weakCache[key] = new WeakReference<ExpressionCoreSqlWithKey>(newInstance);
        return newInstance;
    }

}
