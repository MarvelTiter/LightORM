using System.Threading;
using static LightORM.SqlExecutor.SqlExecutor;

namespace LightORM.ExpressionSql;

internal sealed class ScopedExpressionCoreSql : ExpressionCoreSqlBase, IScopedExpressionContext
{
    private readonly SqlExecutorProvider executorProvider;

    public string Id { get; } = $"{Guid.NewGuid():N}";
    private bool useTrans;
    private IsolationLevel isolationLevel = IsolationLevel.Unspecified;
    public override ExpressionSqlOptions Options { get; }
    private TransientExpressionCoreSql? current;
    public override ISqlExecutor Ado
    {
        get
        {
            if (current is not null)
            {
                return current.Ado;
            }
            var ado = executorProvider.GetSqlExecutor(ConstString.Main);
            if (useTrans)
            {
                ado.InitTransaction(isolationLevel);
            }
            return ado;
        }
    }


    public ScopedExpressionCoreSql(ExpressionSqlOptions options)
    {
        this.executorProvider = new SqlExecutorProvider(options);
        Options = options;
        foreach (var item in options.DatabaseProviders.Values)
        {
            var ctx = AsyncLocalTransactionContexts.GetOrAdd(item, new AsyncLocal<TransactionContext?>());
            ctx.Value ??= new TransactionContext();
        }
    }

    private readonly Dictionary<string, TransientExpressionCoreSql> contextCaches = [];
    ITransientExpressionContext IScopedExpressionContext.SwitchDatabase(string key)
    {
        if (contextCaches.TryGetValue(key, out var ctx))
        {
            return ctx;
        }
        var ado = executorProvider.GetSqlExecutor(key);
        if (useTrans)
        {
            ado.InitTransaction(isolationLevel);
        }
        ctx = new(key, ado, Options);
        contextCaches[key] = ctx;
        current = ctx;
        return ctx;
    }

    public void Dispose()
    {
        executorProvider.Dispose();
    }

    public void BeginAllTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        useTrans = true;
        this.isolationLevel = isolationLevel;
        executorProvider.Executors.ForEach(e => e.BeginTransaction(isolationLevel));
    }

    public async Task BeginAllTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        useTrans = true;
        this.isolationLevel = isolationLevel;
        await executorProvider.Executors.ForEachAsync(e => e.BeginTransactionAsync(isolationLevel));
    }
    public void CommitAllTransaction() => executorProvider.Executors.ForEach(e => e.CommitTransaction());

    public async Task CommitAllTransactionAsync() => await executorProvider.Executors.ForEachAsync(e => e.CommitTransactionAsync());

    public void RollbackAllTransaction() => executorProvider.Executors.ForEach(e => e.RollbackTransaction());

    public async Task RollbackAllTransactionAsync() => await executorProvider.Executors.ForEachAsync(e => e.RollbackTransactionAsync());

    public void BeginTransaction(string key = "MainDb", IsolationLevel isolationLevel = IsolationLevel.Unspecified) => executorProvider.GetSqlExecutor(key).BeginTransaction();

    public Task BeginTransactionAsync(string key = "MainDb", IsolationLevel isolationLevel = IsolationLevel.Unspecified) => executorProvider.GetSqlExecutor(key).BeginTransactionAsync();

    public void CommitTransaction(string key = "MainDb") => executorProvider.GetSqlExecutor(key).CommitTransaction();

    public Task CommitTransactionAsync(string key = "MainDb") => executorProvider.GetSqlExecutor(key).CommitTransactionAsync();

    public void RollbackTransaction(string key = "MainDb") => executorProvider.GetSqlExecutor(key).RollbackTransaction();

    public Task RollbackTransactionAsync(string key = "MainDb") => executorProvider.GetSqlExecutor(key).RollbackTransactionAsync();
}
