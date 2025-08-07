using LightORM.Providers;
using System;
using System.Threading;
namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql : ExpressionCoreSqlBase, IExpressionContext, IDisposable
{
    public ExpressionSqlOptions Options { get; }
    internal SqlExecutorProvider executorProvider;
    public string Id { get; } = $"{Guid.NewGuid():N}";
    private readonly SemaphoreSlim switchSign = new(1, 1);
    //public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(CurrentKey, UseTrans);
    public override ISqlExecutor Ado
    {
        get
        {
            dbKey ??= ConstString.Main;
            var ado = executorProvider.GetSqlExecutor(dbKey);
            dbKey = null;
            if (switchSign.CurrentCount == 0) switchSign.Release();
            return ado;
        }
    }
    public ExpressionCoreSql(ExpressionSqlOptions option)
    {
#if DEBUG
        Console.WriteLine($"创建ExpressionCoreSql：{DateTime.Now}");
#endif
        executorProvider = new SqlExecutorProvider(option);
        Options = option;
    }

    private string? dbKey = null;

    public IExpressionContext SwitchDatabase(string key)
    {
        // 确保切换key之后，Provider拿到的ISqlExecutor是对应的
        switchSign.Wait();
        dbKey = key;
        return this;
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
