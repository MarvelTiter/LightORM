using System.Data.Common;
using System.Diagnostics;
using System.Threading;
namespace LightORM.SqlExecutor;

public class DatabaseConnection(DbConnection connection, DbTransaction? transaction = null)
{
    public bool IsOccurException { get; set; }
    public DbConnection Connection { get; set; } = transaction?.Connection ?? connection;
    public DbTransaction? Transaction { get; set; } = transaction;
    public DbConnection GetCurrentConnection() => Transaction?.Connection ?? Connection;
    public bool UnderTransaction => Transaction is not null;
    public int TransactionNestLevel { get; set; }
    public int Id => Connection.GetHashCode();
    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        try
        {
            if (Transaction is null)
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
                Transaction = isolationLevel == IsolationLevel.Unspecified
                        ? Connection.BeginTransaction()
                        : Connection.BeginTransaction(isolationLevel);
            }
            else
            {
                TransactionNestLevel++;
#if NET6_0_OR_GREATER
                if (Transaction.SupportsSavepoints)
                {
                    Transaction.Save($"savePoint{TransactionNestLevel}");
                }
#endif
            }
        }
        catch (Exception)
        {
            //var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            //Interceptor.NotifyException(ctx);
            //CurrentTransactionContext.Value?.SetException(ex);
            //if (ctx.IsHandled)
            //{
            //    return;
            //}
            //throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"BeginTran： {Id} -> {TransactionNestLevel}");
    }

    public void CommitTransaction()
    {
        if (Transaction is null)
            return;
        if (TransactionNestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTran： {Id} -> {TransactionNestLevel}");
            TransactionNestLevel--;
            return;
        }
        // 最外层事务提交
        try
        {
            Transaction.Commit();
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTran： {Id} -> finished");
        }
        finally
        {

        }
    }

    public void RollbackTransaction()
    {
        if (Transaction is null)
            return;
        if (TransactionNestLevel > 0)
        {
#if NET6_0_OR_GREATER
            if (Transaction.SupportsSavepoints)
            {
                Transaction.Rollback($"savePoint{TransactionNestLevel}");
            }
#endif
            TransactionNestLevel--;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {Id} -> {TransactionNestLevel}");
            return;
        }
        try
        {
            Transaction.Rollback();
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {Id} -> finished");
        }
        finally
        {

        }
    }
    #region 异步API
#if NET6_0_OR_GREATER
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Transaction is null)
            {
                if (Connection.State != ConnectionState.Open)
                {
                    await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                Transaction = isolationLevel == IsolationLevel.Unspecified
                    ? await Connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
                    : await Connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // 嵌套事务
                TransactionNestLevel++;
                if (Transaction.SupportsSavepoints)
                {
                    Transaction.Save($"savePoint{TransactionNestLevel}");
                }
            }
        }
        catch (Exception ex)
        {
            //var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            //Interceptor.NotifyException(ctx);
            //CurrentTransactionContext.Value?.SetException(ex);
            //if (ctx.IsHandled)
            //{
            //    return;
            //}
            throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"BeginTranAsync： {Id} -> {TransactionNestLevel}");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction is null)
        {
            if (IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to commit"); ;
            //var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null, typeof(object)), ex);
            //Interceptor.NotifyException(ctx);
            //if (ctx.IsHandled)
            //{
            //    return;
            //}
            //throw ex;
            return;
        }
        if (TransactionNestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTranAsync： {Id} -> {TransactionNestLevel}");
            TransactionNestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            await Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTranAsync： {Id} -> finished");
        }
        finally
        {

        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction is null)
        {
            if (IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            //var ex = new InvalidOperationException("No active transaction to commit"); ;
            //var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            //Interceptor.NotifyException(ctx);
            //if (ctx.IsHandled)
            //{
            //    return;
            //}
            //throw ex;
            return;
        }
        if (TransactionNestLevel > 0)
        {
            if (Transaction.SupportsSavepoints)
            {
                await Transaction.RollbackAsync($"savePoint{TransactionNestLevel}", cancellationToken).ConfigureAwait(false);
            }
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {Id} -> {TransactionNestLevel}");
            TransactionNestLevel--;
            return;
        }
        try
        {
            await Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {Id} -> finished");
        }
        finally
        {

        }
    }
#endif
    #endregion

    public void Dispose()
    {

    }
}
