using System.Threading;
using LightORM.DbStruct;

namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql(ExpressionSqlOptions option) : ExpressionCoreSqlBase, IExpressionContext
{
    public override ExpressionSqlOptions Options { get; } = option;
    internal readonly SqlExecutorProvider executorProvider = new(option);
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(Options.DefaultDbKey);

    public ITransientExpressionContext SwitchDatabase(string key)
    {
        var ado = executorProvider.GetSqlExecutor(key);
        return TransientExpressionCoreSql.Create(key, ado, Options);
    }

    public ISqlExecutor GetAdo(string key) => executorProvider.GetSqlExecutor(key);

    public IExpSelect Select(string tableName) => throw new NotImplementedException(); //new SelectProvider0(tableName, Ado);


    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                System.Diagnostics.Debug.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
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

    public string? CreateTableSql<T>(IDatabaseProvider provider, Action<TableGenerateOption>? action = null)
    {
        using var ado = new SqlExecutor.SqlExecutor(provider, Options.PoolSize, new(Options.Interceptors));
        return InternalCreateTableSql<T>(ado, Options, action);
    }

    public async Task<bool> CreateTableAsync<T>(IDatabaseProvider provider, Action<TableGenerateOption>? action = null, CancellationToken cancellationToken = default)
    {
        using var ado = new SqlExecutor.SqlExecutor(provider, Options.PoolSize, new(Options.Interceptors));
        return await InternalCreateTableAsync<T>(ado, Options, action, cancellationToken);
    }

    public async Task<IList<DbStruct.ReadedTable>> GetTablesAsync(IDatabaseProvider provider)
    {
        using var ado = new SqlExecutor.SqlExecutor(provider, Options.PoolSize, new(Options.Interceptors));
        return await InternalGetTablesAsync(ado, Options);
    }

    public async Task<ReadedTable> GetTableStructAsync(IDatabaseProvider provider, DbStruct.ReadedTable table)
    {
        using var ado = new SqlExecutor.SqlExecutor(provider, Options.PoolSize, new(Options.Interceptors));
        return await InternalTableStructAsync(table, ado, Options);
    }
}