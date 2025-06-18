
namespace LightORM.ExpressionSql;

internal sealed class SingleScopedExpressionCoreSql : ExpressionCoreSqlBase, ISingleScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public SingleScopedExpressionCoreSql(ISqlExecutor sqlExecutor)
    {
        Ado = sqlExecutor;
        Ado.InitTransactionContext();
    }
    public override ISqlExecutor Ado { get; } 

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified) => Ado.BeginTransaction(isolationLevel);

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified) => await Ado.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);

    public void CommitTransaction() => Ado.CommitTransaction();

    public async Task CommitTransactionAsync() => await Ado.CommitTransactionAsync().ConfigureAwait(false);

    public void RollbackTransaction() => Ado.RollbackTransaction();

    public async Task RollbackTransactionAsync() => await Ado.RollbackTransactionAsync().ConfigureAwait(false);

    public void Dispose()
    {
        Ado.Dispose();
    }
}