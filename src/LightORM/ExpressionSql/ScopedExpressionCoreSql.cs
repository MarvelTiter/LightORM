using System.Collections.Concurrent;
using System.Threading;

namespace LightORM.ExpressionSql;

internal sealed class ScopedExpressionCoreSql(ExpressionSqlOptions options) : ExpressionCoreSqlBase(options), IScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
    private bool useTrans;
    private IsolationLevel isolationLevel = IsolationLevel.Unspecified;
    private readonly ConnectionFactory connectionFactory = new(options);
    private readonly ConcurrentDictionary<string, DatabaseConnection> connections = [];
    private TransientExpressionContext? current;
    public override SqlAdo Ado
    {
        get
        {
            if (current.HasValue)
            {
                return current.Value.Ado;
            }
            return DefaultAdo;
        }
    }

    public SqlAdo DefaultAdo
    {
        get
        {
            var connection = connections.GetOrAdd(Options.DefaultDbKey, connectionFactory.GetDatabaseConnection);
            if (useTrans)
            {
                connection.BeginTransaction();
            }
            return new(connection);
        }
    }

    private readonly Dictionary<string, TransientExpressionContext> contextCaches = [];
    TransientExpressionContext IScopedExpressionContext.SwitchDatabase(string key)
    {
        if (contextCaches.TryGetValue(key, out var ctx))
        {
            return ctx;
        }
        var connection = connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection);
        if (useTrans)
        {
            connection.BeginTransaction(isolationLevel);
        }
        ctx = new(this, connection, Options);
        contextCaches[key] = ctx;
        current = ctx;
        return ctx;
    }

    public void Dispose()
    {
        foreach (var item in connections.Values)
        {
            item.Dispose();
        }
    }

    public void BeginAllTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        useTrans = true;
        this.isolationLevel = isolationLevel;
        foreach (var item in connections.Values)
        {
            item.BeginTransaction(isolationLevel);
        }
    }
    public void CommitAllTransaction()
    {
        foreach (var item in connections.Values)
        {
            item.CommitTransaction();
        }
    }
    public void RollbackAllTransaction()
    {
        foreach (var item in connections.Values)
        {
            item.RollbackTransaction();
        }
    }
    public void BeginTransaction(string key = "MainDb", IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).BeginTransaction(isolationLevel);

    public void CommitTransaction(string key = "MainDb")
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).CommitTransaction();

    public void RollbackTransaction(string key = "MainDb")
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).RollbackTransaction();

#if NET8_0_OR_GREATER
    public async Task BeginAllTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        useTrans = true;
        this.isolationLevel = isolationLevel;
        foreach (var item in connections.Values)
        {
            await item.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
    }

    public async Task CommitAllTransactionAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in connections.Values)
        {
            await item.CommitTransactionAsync(cancellationToken);
        }
    }


    public async Task RollbackAllTransactionAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in connections.Values)
        {
            await item.RollbackTransactionAsync(cancellationToken);
        }
    }


    public Task BeginTransactionAsync(string key = "MainDb"
        , IsolationLevel isolationLevel = IsolationLevel.Unspecified
        , CancellationToken cancellationToken = default) 
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).BeginTransactionAsync(isolationLevel, cancellationToken);


    public Task CommitTransactionAsync(string key = "MainDb", CancellationToken cancellationToken = default) 
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).CommitTransactionAsync(cancellationToken);


    public Task RollbackTransactionAsync(string key = "MainDb", CancellationToken cancellationToken = default) 
        => connections.GetOrAdd(key, connectionFactory.GetDatabaseConnection).RollbackTransactionAsync(cancellationToken);
#endif

}
