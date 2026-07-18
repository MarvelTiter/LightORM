using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor;

internal partial class SqlExecutor
{
    // 事务上下文管理
    internal static readonly ConcurrentDictionary<IDatabaseProvider, AsyncLocal<TransactionContext?>> AsyncLocalTransactionContexts = new();
    public AsyncLocal<TransactionContext?> CurrentTransactionContext { get; }

    // 事务上下文类
    public class TransactionContext
    {
        public DbTransaction? Transaction { get; set; }
        public DbConnection? Connection { get; set; }
        public int NestLevel { get; set; }
        public bool IsExternal { get; internal set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Exception? Exception { get; set; }
        public bool IsOccurException { get; set; }

        internal void SetException(Exception ex)
        {
            IsOccurException = true;
            Exception = ex;
        }
    }

    // 使用外部事务
    public void UseExternalTransaction(DbTransaction externalTransaction)
    {
        ArgumentNullException.ThrowIfNull(externalTransaction);

        if (CurrentTransactionContext.Value != null)
            throw new InvalidOperationException("Already in a transaction context");
        if (externalTransaction.Connection is null)
            throw new InvalidOperationException("External transaction must have a valid connection");
        CurrentTransactionContext.Value = new TransactionContext()
        {
            IsExternal = true,
            Transaction = externalTransaction,
            Connection = externalTransaction.Connection,
        };
    }
    public void InitTransactionContext()
    {
        CurrentTransactionContext.Value ??= new TransactionContext();
    }

    public void InitTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        try
        {
            if (CurrentTransactionContext.Value?.Transaction is null)
            {
                CurrentTransactionContext.Value ??= new TransactionContext();
                // 新事务
                var conn = Pool.Get();
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                var transaction = isolationLevel == IsolationLevel.Unspecified
                    ? conn.BeginTransaction()
                    : conn.BeginTransaction(isolationLevel);
                CurrentTransactionContext.Value.Transaction = transaction;
                CurrentTransactionContext.Value.Connection = conn;
            }
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            CurrentTransactionContext.Value?.SetException(ex);
            if (ctx.IsHandled)
            {
                return;
            }
            throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"InitTransaction： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        try
        {
            if (CurrentTransactionContext.Value?.Transaction is null)
            {
                // 新事务
                CurrentTransactionContext.Value ??= new TransactionContext();

                var conn = Pool.Get();
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                var transaction = isolationLevel == IsolationLevel.Unspecified
                    ? conn.BeginTransaction()
                    : conn.BeginTransaction(isolationLevel);
                CurrentTransactionContext.Value.Transaction = transaction;
                CurrentTransactionContext.Value.Connection = conn;
            }
            else
            {
                var context = CurrentTransactionContext.Value;
                // 嵌套事务
                context.NestLevel++;
#if NET6_0_OR_GREATER
                if (context.Transaction.SupportsSavepoints)
                {
                    context.Transaction.Save($"savePoint{context.NestLevel}");
                }
#endif
            }
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            CurrentTransactionContext.Value?.SetException(ex);
            if (ctx.IsHandled)
            {
                return;
            }
            throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"BeginTran： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }
    public void CommitTransaction()
    {
        var context = CurrentTransactionContext.Value;
        if (context?.Transaction is null)
        {
            if (context?.IsOccurException == true)
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
        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTran： {context.Id} -> {context.NestLevel}");
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            context.Transaction.Commit();
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTran： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }
    public void RollbackTransaction()
    {
        var context = CurrentTransactionContext.Value;
        if (context?.Transaction is null)
        {
            if (context?.IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to commit"); ;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
        if (context.NestLevel > 0)
        {
#if NET6_0_OR_GREATER
            if (context.Transaction.SupportsSavepoints)
            {
                context.Transaction.Rollback($"savePoint{context.NestLevel}");
            }
#endif
            context.NestLevel--;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {context.Id} -> {context.NestLevel}");
            return;
        }
        try
        {
            context.Transaction.Rollback();
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

#if NET6_0_OR_GREATER
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        try
        {
            if (CurrentTransactionContext.Value?.Transaction is null)
            {
                // 新事务
                CurrentTransactionContext.Value ??= new();
                var conn = Pool.Get();
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                var transaction = isolationLevel == IsolationLevel.Unspecified
                    ? await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
                    : await conn.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

                CurrentTransactionContext.Value.Transaction = transaction;
                CurrentTransactionContext.Value.Connection = conn;
            }
            else
            {
                var context = CurrentTransactionContext.Value;
                // 嵌套事务
                context.NestLevel++;
                if (context.Transaction.SupportsSavepoints)
                {
                    context.Transaction.Save($"savePoint{context.NestLevel}");
                }
            }
        }
        catch (Exception ex)
        {
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            CurrentTransactionContext.Value?.SetException(ex);
            if (ctx.IsHandled)
            {
                return;
            }
            throw;
        }
        Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"BeginTranAsync： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.Transaction is null)
        {
            if (context?.IsOccurException == true)
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
        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTranAsync： {context.Id} -> {context.NestLevel}");
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            await context.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"CommitTranAsync： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.Transaction is null)
        {
            if (context?.IsOccurException == true)
            {
                // 如果BeginTransaction发生的异常没有处理，不会进入到CommitTransaction，如果运行到这里，说明异常已经处理了，直接return
                return;
            }
            var ex = new InvalidOperationException("No active transaction to commit"); ;
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null, typeof(object)), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
        if (context.NestLevel > 0)
        {
            if (context.Transaction.SupportsSavepoints)
            {
                await context.Transaction.RollbackAsync($"savePoint{context.NestLevel}", cancellationToken).ConfigureAwait(false);
            }
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {context.Id} -> {context.NestLevel}");
            context.NestLevel--;
            return;
        }
        try
        {
            await context.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

#else
    public Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        BeginTransaction(isolationLevel);
        return Task.FromResult(true);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        CommitTransaction();
        return Task.FromResult(true);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        RollbackTransaction();
        return Task.FromResult(true);
    }

#endif
    private void DisposeTransactionContext()
    {
        var context = CurrentTransactionContext.Value;
        if (context == null) return;

        // 内部事务创建的事务上下文
        if (!context.IsExternal && context.Transaction is not null)
        {
            var conn = context.Connection;
            if (conn is not null)
            {
                conn.Close();
                Pool.Return(conn);
            }
            context.Transaction.Dispose();
            context.Transaction = null;
            context.Connection = null;
        }
    }
}
