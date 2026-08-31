using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
namespace LightORM.SqlExecutor;

internal readonly struct PrepareResult(DbCommand command, bool isBreak)
{
    public DbCommand Command { get; } = command;
    public bool Break { get; } = isBreak;
}
public readonly struct SqlAdo
{
    internal SqlAdo(DatabaseConnection connection)
    {
        Connection = connection;
    }
    internal DatabaseConnection Connection { get; }
    public IDatabaseProvider Provider => Connection.Provider;
    internal AdoInterceptor Interceptor => Connection.Interceptor;

    internal void UseExternalTransaction(DbTransaction transaction) => Connection.UseExternalTransaction(transaction);

    internal void DisposeConnection()
    {
        if (Connection.UnderTransaction)
        {
            return;
        }
        Connection.Dispose();
    }

    private void DisposeCommand(PrepareResult result)
    {
        DisposeConnection();
        result.Command.Parameters.Clear();
        result.Command.Dispose();
    }

#if NET8_0_OR_GREATER

    private ValueTask DisposeCommandAsync(PrepareResult result)
    {
        DisposeConnection();
        result.Command.Parameters.Clear();
        return result.Command.DisposeAsync();
    }
#endif

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
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
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
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
        }
    }

    public DbDataReader ExecuteReader<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
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
                reader = r.Command.ExecuteReader(b);
            }
            else
            {
                reader = r.Command.ExecuteReader(behavior ?? CommandBehavior.Default);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReaderLight(reader, r, Connection);
        }
        catch (Exception ex)
        {
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
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
        PrepareResult? commandResult = default;
        try
        {
            commandResult = PrepareCommand(commandType, ctx);
            var r = commandResult.Value;
            if (r.Break)
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
                reader = r.Command.ExecuteReader(b);

            }
            else
            {
                reader = r.Command.ExecuteReader(behavior ?? CommandBehavior.Default);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new(new InternalDataReaderLight(reader, r, Connection));
        }
        catch (Exception ex)
        {
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
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
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
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
            if (commandResult.HasValue)
            {
                DisposeCommand(commandResult.Value);
            }
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
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
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
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        PrepareResult? commandResult = default;
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
                reader = await r.Command.ExecuteReaderAsync(b, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                reader = await r.Command.ExecuteReaderAsync(behavior ?? CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new InternalDataReaderLight(reader, r, Connection);
        }
        catch (Exception ex)
        {
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
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
        PrepareResult? commandResult = default;
        try
        {
            commandResult = await PrepareCommandAsync(commandType, ctx, cancellationToken).ConfigureAwait(false);
            var r = commandResult.Value;
            if (r.Break)
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
                reader = await r.Command.ExecuteReaderAsync(b, cancellationToken).ConfigureAwait(false);

            }
            else
            {
                reader = await r.Command.ExecuteReaderAsync(behavior ?? CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
            }
            ctx.Elapsed = StopwatchHelper.GetElapsedTime(start);
            Interceptor.NotifyAfterExecute(ctx);
            return new(new InternalDataReaderLight(reader, r, Connection));
        }
        catch (Exception ex)
        {
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
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
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
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
            if (commandResult.HasValue)
            {
#if NET8_0_OR_GREATER
                await DisposeCommandAsync(commandResult.Value).ConfigureAwait(false);
#else
                DisposeCommand(commandResult.Value);
#endif
            }
        }
    }

    #endregion

    internal static T? ChangeType<T>(object? value)
    {
        if (value is null || value is DBNull)
        {
            return default;
        }
        if (value is T typedValue)
        {
            return typedValue;
        }
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        var result = targetType switch
        {
            _ when targetType == typeof(string) => value.ToString(),
            _ when targetType == typeof(int) => Convert.ToInt32(value),
            _ when targetType == typeof(long) => Convert.ToInt64(value),
            _ when targetType == typeof(short) => Convert.ToInt16(value),
            _ when targetType == typeof(byte) => Convert.ToByte(value),
            _ when targetType == typeof(decimal) => Convert.ToDecimal(value),
            _ when targetType == typeof(double) => Convert.ToDouble(value),
            _ when targetType == typeof(float) => Convert.ToSingle(value),
            _ when targetType == typeof(bool) => Convert.ToBoolean(value),
            _ when targetType == typeof(DateTime) => Convert.ToDateTime(value),
            _ when targetType == typeof(Guid) => Guid.Parse(value.ToString()!),
            _ when targetType == typeof(char) => Convert.ToChar(value),
            _ when targetType.IsEnum => Enum.Parse(targetType, value.ToString()!, ignoreCase: true),
            // 兜底——理论上不会走到这里
            _ => Convert.ChangeType(value, targetType)
        };
        if (result is T finalResult)
        {
            return finalResult;
        }
        return default;
    }
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