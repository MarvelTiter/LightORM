using System.Threading;
using LightORM.DbStruct;

namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql(ExpressionSqlOptions option) : ExpressionCoreSqlBase(option), IExpressionContext
{
    //private readonly ConcurrentDictionary<string, DatabaseConnection> connections = [];
    private readonly ConnectionFactory connectionFactory = new(option);
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override SqlAdo Ado => GetAdo(Options.DefaultDbKey);
    public ITransientContext SwitchDatabase(string key)
    {
        var connection = connectionFactory.GetDatabaseConnection(key);
        return new TransientContext(connection, Options);
    }

    public SqlAdo GetAdo(string key)
    {
        var connection = connectionFactory.GetDatabaseConnection(key);
        return new(connection);
    }

    public IExpSelect Select(string tableName) => throw new NotImplementedException(); //new SelectProvider0(tableName, Ado);

    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                System.Diagnostics.Debug.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
                //foreach (var item in connections.Values)
                //{
                //    item.Dispose();
                //}
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public string? CreateTableSql<T>(IDatabaseProvider provider, Action<TableOptions>? action = null)
    {
        return ExpressionCoreSqlContextMethodImpl.InternalCreateTableSql<T>(provider, action);
    }

    public async Task<bool> CreateTableAsync<T>(IDatabaseProvider provider, Action<TableOptions>? action = null, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.GetDatabaseConnection(provider);
        var ado = new SqlAdo(connection);
        var flag = await ExpressionCoreSqlContextMethodImpl.InternalCreateTableAsync<T>(ado, Options, action, cancellationToken);
        return flag;
    }

    public async Task<IList<DbStruct.ReadedTable>> GetTablesAsync(IDatabaseProvider provider)
    {
        using var connection = connectionFactory.GetDatabaseConnection(provider);
        var ado = new SqlAdo(connection);
        return await ExpressionCoreSqlContextMethodImpl.InternalGetTablesAsync(ado, Options);
    }

    public async Task<ReadedTable> GetTableStructAsync(IDatabaseProvider provider, DbStruct.ReadedTable table)
    {
        using var connection = connectionFactory.GetDatabaseConnection(provider);
        var ado = new SqlAdo(connection);
        return await ExpressionCoreSqlContextMethodImpl.InternalTableStructAsync(table, ado, Options);
    }
}