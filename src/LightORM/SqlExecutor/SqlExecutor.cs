﻿using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading.Tasks;
using LightORM.Cache;
using System.Threading;
using System.Collections.Concurrent;
using LightORM.Models;
namespace LightORM.SqlExecutor;

internal partial class SqlExecutor : ISqlExecutor, IDisposable
{
    // 事务上下文管理
    internal static readonly ConcurrentDictionary<IDatabaseProvider, AsyncLocal<TransactionContext?>> AsyncLocalTransactionContexts = new();
    public string Id { get; set; }
    internal static readonly ConcurrentDictionary<IDatabaseProvider, ConnectionPool> Pools = [];
    private static readonly ConcurrentDictionary<IDatabaseProvider, int> PoolSizes = [];
    //public Action<string, object?>? DbLog { get; set; }
    public IDatabaseProvider Database { get; private set; }
    public AsyncLocal<TransactionContext?> CurrentTransactionContext { get; }
    /// <summary>
    /// 数据库事务
    /// </summary>
    public DbTransaction? DbTransaction
    {
        get => CurrentTransactionContext.Value?.Transaction;
    }

    //public DbConnection DbConnection { get; private set; }
    public AdoInterceptor Interceptor { get; }
    public ConnectionPool Pool { get; }

    public SqlExecutor(IDatabaseProvider database, int poolSize, AdoInterceptor interceptor, string? id = null)
    {
        Database = database;
        Interceptor = interceptor;
        _ = PoolSizes.GetOrAdd(database, poolSize);
        Pool = Pools.GetOrAdd(database, db =>
        {
            PoolSizes.TryGetValue(db, out var size);
            return new ConnectionPool(() =>
            {
                var conn = db.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = db.MasterConnectionString;
                return conn;
            }, size);
        });
        Id = id ?? Guid.NewGuid().ToString();
        CurrentTransactionContext = AsyncLocalTransactionContexts.GetOrAdd(Database, new AsyncLocal<TransactionContext?>());
    }

    public SqlExecutor(IDatabaseProvider database, AdoInterceptor interceptor, string? id = null)
    {
        Database = database;
        Interceptor = interceptor;
        Pool = Pools.GetOrAdd(database, db =>
        {
            PoolSizes.TryGetValue(db, out var size);
            return new ConnectionPool(() =>
            {
                var conn = db.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = db.MasterConnectionString;
                return conn;
            }, size);
        });
        Id = id ?? Guid.NewGuid().ToString();
        CurrentTransactionContext = AsyncLocalTransactionContexts.GetOrAdd(Database, new AsyncLocal<TransactionContext?>());
    }

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
        if (externalTransaction == null)
            throw new ArgumentNullException(nameof(externalTransaction));

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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null), ex);
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null), ex);
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null), ex);
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
        if (context.NestLevel > 0)
        {
            context.NestLevel--;
            Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTran： {context.Id} -> {context.NestLevel}");
#if NET6_0_OR_GREATER
            if (context.Transaction.SupportsSavepoints)
            {
                context.Transaction.Rollback($"savePoint{context.NestLevel}");
            }
#endif
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.BeginTransaction, null, null), ex);
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.CommitTransaction, null, null), ex);
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
            var ctx = new SqlExecuteExceptionContext(new SqlExecuteContext(ExecuteMethod.RollbackTransaction, null, null), ex);
            Interceptor.NotifyException(ctx);
            if (ctx.IsHandled)
            {
                return;
            }
            throw ex;
        }
        try
        {
            if (context.NestLevel > 0)
            {
                Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {context.Id} -> {context.NestLevel}");
                context.NestLevel--;
                if (context.Transaction.SupportsSavepoints)
                {
                    await context.Transaction.RollbackAsync($"savePoint{context.NestLevel}", cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await context.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                Debug.WriteLineIf(ShowSqlExecutorDebugInfo, $"RollbackTranAsync： {context.Id} -> finished");
            }
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

    private void DisposeCommand(CommandResult result)
    {
        result.Command.Parameters.Clear();
        result.Command.Dispose();
        if (result.NeedToReturn)
        {
            Pool.Return(result.Connection);
        }
    }

    private readonly struct CommandResult(DbCommand command, DbConnection connection, bool needToReturn, bool @break)
    {
        public DbCommand Command { get; } = command;
        public DbConnection Connection { get; } = connection;
        public bool NeedToReturn { get; } = needToReturn;
        public bool Break { get; } = @break;
    }

    private CommandResult PrepareCommand(CommandType commandType, SqlExecuteContext et)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.IsOccurException == true)
        {
            return new(null!, null!, false, true);
        }
        var commandText = et.Sql!;
        var dbParameters = et.Parameter;
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn;
        bool needToReturn;
        if (context?.Transaction is not null)
        {
            // 事务操作使用事务连接
            conn = context.Transaction.Connection!;
            needToReturn = false;
        }
        else
        {
            // 非事务操作从池中获取连接
            conn = Pool.Get();
            needToReturn = true;
        }

        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        var command = conn.CreateCommand();
        GetInit(command.GetType())?.Invoke(command);
        if (context != null)
        {
            command.Transaction = context.Transaction;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = DbParameterReader.GetDbParameterReader(conn.ConnectionString, commandText, dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }

        return new(command, conn, needToReturn, false);
    }

    private async Task<CommandResult> PrepareCommandAsync(CommandType commandType, SqlExecuteContext et, CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.IsOccurException == true)
        {
            return new(null!, null!, false, true);
        }
        var commandText = et.Sql!;
        var dbParameters = et.Parameter;
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn;
        bool needToReturn;
        if (context?.Transaction is not null)
        {
            // 事务操作使用事务连接
            conn = context.Transaction.Connection!;
            needToReturn = false;
        }
        else
        {
            // 非事务操作从池中获取连接
            conn = Pool.Get();
            needToReturn = true;
        }

        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        var command = conn.CreateCommand();
        GetInit(command.GetType())?.Invoke(command);

        if (context != null)
        {
            command.Transaction = context.Transaction;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = DbParameterReader.GetDbParameterReader(conn.ConnectionString, commandText, dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }
        return new(command, conn, needToReturn, false);
    }

    public int ExecuteNonQuery(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            var r = commandResult.Value;
            if (r.Break)
            {
                return 0;
            }
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            var result = r.Command.ExecuteNonQuery();
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return result;
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return 0;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
    }

    public T? ExecuteScalar<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            var r = commandResult.Value;
            if (r.Break)
            {
                return default;
            }
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            var obj = r.Command.ExecuteScalar();
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return default;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
    }

    public DbDataReader ExecuteReader(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters);
        CommandResult commandResult;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            if (commandResult.Break)
            {
                return new EmptyDataReader();
            }
            DbDataReader reader;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            if (commandResult.NeedToReturn)
            {
                reader = commandResult.Command.ExecuteReader(CommandBehavior.CloseConnection);

            }
            else
            {
                reader = commandResult.Command.ExecuteReader();
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReader(reader, commandResult, Pool);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new EmptyDataReader();
            }
            throw;
        }
        finally
        {

        }
    }

    public DataSet ExecuteDataSet(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            var r = commandResult.Value;
            if (r.Break)
            {
                return new();
            }
            adapter!.SelectCommand = r.Command;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            adapter.Fill(ds);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new DataSet();
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
        return ds;
    }

    public DataTable ExecuteDataTable(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            var r = commandResult.Value;
            if (r.Break)
            {
                return new();
            }
            adapter!.SelectCommand = r.Command;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            adapter.Fill(ds);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new DataTable();
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
        return ds;
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
            {
                return 0;
            }
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            var result = await r.Command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return result;
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return 0;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
            {
                return default;
            }
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            var obj = await r.Command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return default;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters);
        CommandResult? commandResult;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
            {
                return new EmptyDataReader();
            }
            DbDataReader reader;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            if (r.NeedToReturn)
            {
                reader = await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                reader = await r.Command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReader(reader, r, Pool);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new EmptyDataReader();
            }
            throw;
        }
        finally
        {

        }
    }

    public async Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataSet();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
            {
                return ds;
            }
            adapter!.SelectCommand = r.Command;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            adapter.Fill(ds);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return ds;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
        return ds;
    }

    public async Task<DataTable> ExecuteDataTableAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters);
        CommandResult? commandResult = default;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
            {
                return ds;
            }
            adapter!.SelectCommand = r.Command;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            adapter.Fill(ds);
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return ds;
            }
            throw;
        }
        finally
        {
            if (commandResult.HasValue)
                DisposeCommand(commandResult.Value);
        }
        return ds;
    }

    private static T? ChangeType<T>(object value)
    {
        var conversionType = typeof(T);
        if (value == null)
        {
            return default;
        }
        var type = value.GetType();
        if (type.Equals(typeof(Guid)) && conversionType.Equals(typeof(string)))
        {
            value = value.ToString()!;
        }
        if (conversionType.Equals(type))
        {
            return (T)value;
        }
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            var nullableConverter = new NullableConverter(conversionType);
            conversionType = nullableConverter.UnderlyingType;
        }
        return (T)Convert.ChangeType(value, conversionType);
    }

    private static Dictionary<Type, Action<DbCommand>> commandInitCache = new Dictionary<Type, Action<DbCommand>>();
    private static Action<DbCommand>? GetInit(Type commandType)
    {
        if (commandType == null)
        {
            return null;
        }

        if (commandInitCache.TryGetValue(commandType, out Action<DbCommand>? value))
        {
            return value;
        }

        MethodInfo? basicPropertySetter = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
        MethodInfo? basicPropertySetter2 = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));
        if (basicPropertySetter != null || basicPropertySetter2 != null)
        {
            /*
             * (DbCommand cmd) => {
             *     (OracleCommand)cmd.set_BindByName(true);
             *     (OracleCommand)cmd.set_InitialLONGFetchSize(-1);
             * }
             */
            ParameterExpression cmdExp = Expression.Parameter(typeof(DbCommand), "cmd");
            List<Expression> body = new List<Expression>();
            if (basicPropertySetter != null)
            {
                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
                MethodCallExpression setter1Exp = Expression.Call(convertedCmdExp, basicPropertySetter, Expression.Constant(true, typeof(bool)));
                body.Add(setter1Exp);
            }
            if (basicPropertySetter2 != null)
            {
                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
                MethodCallExpression setter2Exp = Expression.Call(convertedCmdExp, basicPropertySetter2, Expression.Constant(-1, typeof(int)));
                body.Add(setter2Exp);
            }
            var lambda = Expression.Lambda<Action<DbCommand>>(Expression.Block(body), cmdExp);
            value = lambda.Compile();

            commandInitCache.Add(commandType, value);
        }

        return value;
    }

    private static MethodInfo? GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
    {
        PropertyInfo? property = declaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (property != null && property.CanWrite && property.PropertyType == expectedType && property.GetIndexParameters().Length == 0)
        {
            return property.GetSetMethod();
        }
        return null;
    }

    public object Clone()
    {
        return new SqlExecutor(Database, Interceptor);
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Debug.WriteLineIf(ShowSqlExecutorDebugInfo, "SqlExecutor disposing.........");
                //DisposeTransactionContext(disposing);
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

file class StopwatchHelper
{
    public static long GetTimestamp() => Stopwatch.GetTimestamp();
    public static TimeSpan GetElapsedTime(long startingTimestamp)
    {
#if NET8_0_OR_GREATER
        return Stopwatch.GetElapsedTime(startingTimestamp);
#else   
        var end = Stopwatch.GetTimestamp();
        var tickFrequency = (double)(10000 * 1000 / Stopwatch.Frequency);
        var tick = (end - startingTimestamp) * tickFrequency;
        return new TimeSpan((long)tick);
#endif
    }
}