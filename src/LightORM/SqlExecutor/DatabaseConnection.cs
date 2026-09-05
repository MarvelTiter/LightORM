using LightORM.Performances;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
namespace LightORM.SqlExecutor;

internal enum AdoState
{
    Active,
    Committed,
    Rollback,
    OccurException
}
public class DatabaseConnection : IDisposable
{
    //private bool disposed;
    public bool IsOccurException => State == AdoState.OccurException;
    internal AdoState State { get; private set; }
    public IDatabaseProvider Provider { get; }
    internal AdoInterceptor Interceptor { get; }
    public DbConnection Connection { get; set; }
    public DbTransaction? Transaction { get; set; }
    public DbConnection GetCurrentConnection() => Transaction?.Connection ?? Connection;
    public bool UnderTransaction => Transaction is not null;
    public int TransactionNestLevel { get; set; }
    public int Id => Connection.GetHashCode();
    public bool IsExternal { get; set; }

    internal DatabaseConnection(DbConnection connection
    , IDatabaseProvider provider
    , AdoInterceptor adoInterceptor
    , DbTransaction? transaction = null)
    {
        Provider = provider;
        Interceptor = adoInterceptor;
        Connection = transaction?.Connection ?? connection;
        Transaction = transaction;
    }

    public void UseExternalTransaction(DbTransaction dbTransaction)
    {
        if (dbTransaction.Connection is null)
            throw new InvalidOperationException("External transaction must have a valid connection");
        IsExternal = true;
        Transaction = dbTransaction;
        Connection = dbTransaction.Connection;
    }

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        try
        {
            //ObjectDisposedException.ThrowIf(disposed, this);
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
        catch (Exception ex)
        {
            State = AdoState.OccurException;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"BeginTran： {Id} -> {TransactionNestLevel}");
    }

    public void CommitTransaction()
    {
        if (Transaction is null)
        {
            if (IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to commit"); ;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
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
            State = AdoState.Committed;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTran： {Id} -> finished");
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            RollbackTransaction();
            State = AdoState.OccurException;
        }
        finally
        {
            Dispose();
        }
    }

    public void RollbackTransaction()
    {
        if (Transaction is null)
        {
            if (IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to rollback"); ;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
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
            State = AdoState.Rollback;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {Id} -> finished");
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
        }
        finally
        {
            Dispose();
        }
    }

    #region 异步API

#if NET6_0_OR_GREATER
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        try
        {
            //ObjectDisposedException.ThrowIf(disposed, this);
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
            State = AdoState.OccurException;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
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
            State = AdoState.Committed;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTranAsync： {Id} -> finished");
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            await RollbackTransactionAsync(cancellationToken);
            State = AdoState.OccurException;
        }
        finally
        {
            Dispose();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction is null)
        {
            if (IsOccurException == true)
            {
                // 如果发生的异常没有处理，不会进入到这里，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to rollback"); ;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
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
            State = AdoState.Rollback;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {Id} -> finished");
        }
        catch(Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
        }
        finally
        {
            Dispose();
        }
    }
#endif

    #endregion

    public void Dispose()
    {
        //if (disposed)
        //    return;
        // 内部事务创建的事务上下文
        if (!IsExternal)
        {
            if (Connection is not null)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
                var pool = ConnectionPool.Pools[Provider];
                pool.Return(Connection);
            }
            Transaction?.Dispose();
            Transaction = null;
        }
        //disposed = true;
        GC.SuppressFinalize(this);
    }
}
