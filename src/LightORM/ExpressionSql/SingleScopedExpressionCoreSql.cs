
using System.Threading;

namespace LightORM.ExpressionSql;

internal sealed class SingleScopedExpressionCoreSql(DatabaseConnection databaseConnection, ExpressionSqlOptions options) : ExpressionCoreSqlBase(options), ISingleScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override SqlAdo Ado => new(databaseConnection);

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        => databaseConnection.BeginTransaction(isolationLevel);
    public void CommitTransaction() => databaseConnection.CommitTransaction();
    public void RollbackTransaction() => databaseConnection.RollbackTransaction();

#if NET8_0_OR_GREATER

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified
        , CancellationToken cancellationToken = default)
        => await databaseConnection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => await databaseConnection.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => await databaseConnection.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);

#endif


    public void Dispose()
    {
        databaseConnection.KeepAlive = false;
        if (databaseConnection.State == AdoState.Active && databaseConnection.UnderTransaction)
        {
            try
            {
                CommitTransaction();
            }
            catch (Exception)
            {
                RollbackTransaction();
            }
        }
        else
        {
            databaseConnection.Dispose();
        }
    }
    /// <summary>
    /// <para>
    /// 0 - 未开启事务
    /// </para>
    /// <para>
    /// 1 - 已开启事务
    /// </para>
    /// <para>
    /// 2 - 已提交/已回滚事务
    /// </para>
    /// </summary>
    private int transactionState = 0;

    void ISingleScopedExpressionContext.TryBeginTransaction()
    {
        if (Interlocked.CompareExchange(ref transactionState, 1, 0) == 0)
        {
            databaseConnection.BeginTransaction();
        }
    }

    void ISingleScopedExpressionContext.TryCommitTransaction()
    {
        if (Interlocked.CompareExchange(ref transactionState, 2, 1) == 1)
        {
            try
            {
                databaseConnection.CommitTransaction();
            }
            catch (Exception)
            {
                databaseConnection.RollbackTransaction();
                throw;
            }
        }
    }

    void ISingleScopedExpressionContext.TryRollbackTransaction()
    {
        if (Interlocked.CompareExchange(ref transactionState, 2, 1) == 1)
        {
            databaseConnection.RollbackTransaction();
        }
    }

    void ISingleScopedExpressionContext.ResetTransactionState()
    {
        Interlocked.Exchange(ref transactionState, 0);
    }
}