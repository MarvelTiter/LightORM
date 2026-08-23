using LightORM.Performances;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
namespace LightORM.SqlExecutor;

public readonly struct SqlAdo
{
    internal SqlAdo(DatabaseConnection connection, IDatabaseProvider provider, AdoInterceptor interceptor)
    {
        Connection = connection;
        Provider = provider;
        Interceptor = interceptor;
    }
    internal DatabaseConnection Connection { get; }
    public IDatabaseProvider Provider { get; }
    internal AdoInterceptor Interceptor { get; }

    private readonly struct PrepareResult(DbCommand command, bool isBreak)
    {
        public DbCommand Command { get; } = command;
        public bool Break { get; } = isBreak;
    }

    internal void UseExternalTransaction(DbTransaction transaction)
    {
        throw new NotImplementedException();
    }

    #region prepare
    private PrepareResult PrepareCommand(CommandType commandType, SqlExecuteContext et)
    {
        if (Connection.IsOccurException == true)
        {
            return new(null!, true);
        }
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn = Connection.GetCurrentConnection();
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        var command = conn.CreateCommand();
        command.CommandType = commandType;
        Provider.DatabaseAdapter.DbCommandInit(command);
        if (Connection.Transaction is not null)
        {
            command.Transaction = Connection.Transaction;
        }
        et.HandleDbParameter(Provider.DatabaseAdapter.Prefix, command);
        return new(command, false);
    }

    private async Task<PrepareResult> PrepareCommandAsync(CommandType commandType, SqlExecuteContext et, CancellationToken cancellationToken = default)
    {
        if (Connection.IsOccurException == true)
        {
            return new(null!, true);
        }
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn = Connection.GetCurrentConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        var command = conn.CreateCommand();
        command.CommandType = commandType;
        Provider.DatabaseAdapter.DbCommandInit(command);

        if (Connection.Transaction is not null)
        {
            command.Transaction = Connection.Transaction;
        }
        et.HandleDbParameter(Provider.DatabaseAdapter.Prefix, command);
        return new(command, false);
    }
    #endregion

    #region execute

    public int ExecuteNonQuery<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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

        }
    }

    public ScalarValue ExecuteScalar<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return new ScalarValue(obj);
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

        }
    }

    public DbDataReader ExecuteReader<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult commandResult;
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
            if (!Connection.UnderTransaction)
            {
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = commandResult.Command.ExecuteReader(b);
            }
            else
            {
                reader = commandResult.Command.ExecuteReader(behavior ?? CommandBehavior.Default);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReaderLight(reader, Connection);
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

    public MultipleResult QueryMultiple<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult commandResult;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            if (commandResult.Break)
            {
                return new(new EmptyDataReader());
            }
            DbDataReader reader;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            if (behavior?.HasFlag(CommandBehavior.SingleResult) == true)
            {
                throw new LightOrmException("behavior 指定了 CommandBehavior.SingleResult, 不符合QueryMultiple的行为");
            }
            if (!Connection.UnderTransaction)
            {
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = commandResult.Command.ExecuteReader(b);

            }
            else
            {
                reader = commandResult.Command.ExecuteReader(behavior ?? CommandBehavior.Default);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new(new InternalDataReaderLight(reader, Connection));
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new(new EmptyDataReader());
            }
            throw;
        }
        finally
        {

        }
    }

    public DataSet ExecuteDataSet<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = Provider.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return ds;
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

        }
    }

    public DataTable ExecuteDataTable<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = Provider.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return ds;
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

        }
    }

    public async Task<int> ExecuteNonQueryAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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

        }
    }

    public async Task<ScalarValue> ExecuteScalarAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return new(obj);
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

        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult;
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
            if (!Connection.UnderTransaction)
            {
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                reader = await r.Command.ExecuteReaderAsync(behavior ?? CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReaderLight(reader, Connection);
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

    public async Task<MultipleResult> QueryMultipleAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult commandResult;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            if (commandResult.Break)
            {
                return new(new EmptyDataReader());
            }
            DbDataReader reader;
            Interceptor.NotifyBeforeExecute(ctx);
            var start = StopwatchHelper.GetTimestamp();
            if (behavior?.HasFlag(CommandBehavior.SingleResult) == true)
            {
                throw new LightOrmException("behavior 指定了 CommandBehavior.SingleResult, 不符合QueryMultiple的行为");
            }
            if (!Connection.UnderTransaction)
            {
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = await commandResult.Command.ExecuteReaderAsync(b, cancellationToken).ConfigureAwait(false);

            }
            else
            {
                reader = await commandResult.Command.ExecuteReaderAsync(behavior ?? CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new(new InternalDataReaderLight(reader, Connection));
        }
        catch (Exception ex)
        {
            var ectx = new SqlExecuteExceptionContext(ctx, ex);
            Interceptor.NotifyException(ectx);
            if (ectx.IsHandled)
            {
                return new(new EmptyDataReader());
            }
            throw;
        }
        finally
        {

        }
    }

    public async Task<DataSet> ExecuteDataSetAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataSet();
        using var adapter = Provider.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return ds;
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

        }
    }

    public async Task<DataTable> ExecuteDataTableAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataTable();
        using var adapter = Provider.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
            return ds;
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

        }
    }

    #endregion

}

internal class StopwatchHelper
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