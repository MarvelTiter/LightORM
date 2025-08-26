
using System.Threading;

namespace LightORM.ExpressionSql;

internal sealed class SingleScopedExpressionCoreSql : ExpressionCoreSqlBase, ISingleScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public bool IsTransaction { get; set; }

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
            Ado.BeginTransaction();
        }
    }

    void ISingleScopedExpressionContext.TryCommitTransaction()
    {
        if (Interlocked.CompareExchange(ref transactionState, 2, 1) == 1)
        {
            try
            {
                Ado.CommitTransaction();
            }
            catch (Exception)
            {
                Ado.RollbackTransaction();
                throw;
            }
        }
    }

    void ISingleScopedExpressionContext.TryRollbackTransaction()
    {
        if (Interlocked.CompareExchange(ref transactionState, 2, 1) == 1)
        {
            Ado.RollbackTransaction();
        }
    }

    void ISingleScopedExpressionContext.ResetTransactionState()
    {
        Interlocked.Exchange(ref transactionState, 0);
    }
}