using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
namespace LightORM.SqlExecutor;

internal partial class SqlExecutor : ISqlExecutor
{
    public string Id { get; set; }
    internal static readonly ConcurrentDictionary<IDatabaseProvider, ConnectionPool> Pools = [];
    private static readonly ConcurrentDictionary<IDatabaseProvider, int> PoolSizes = [];
    public IDatabaseProvider Database { get; private set; }
    /// <summary>
    /// 数据库事务
    /// </summary>
    public DbTransaction? DbTransaction
    {
        get => CurrentTransactionContext.Value?.Transaction;
    }

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

    public int ExecuteNonQuery<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters, typeof(TParameter), commandType);
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

    public ScalarValue ExecuteScalar<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters, typeof(TParameter), commandType);
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
                DisposeCommand(commandResult.Value);
        }
    }

    public DbDataReader ExecuteReader<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
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
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = commandResult.Command.ExecuteReader(b);

            }
            else
            {
                reader = commandResult.Command.ExecuteReader(behavior ?? CommandBehavior.Default);
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

    public MultipleResult QueryMultiple<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        CommandResult commandResult;
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
            if (commandResult.NeedToReturn)
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
            return new(new InternalDataReader(reader, commandResult, Pool));
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
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters, typeof(TParameter), commandType);
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

    public DataTable ExecuteDataTable<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters, typeof(TParameter), commandType);
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

    public async Task<int> ExecuteNonQueryAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.NonQuery, commandText, dbParameters, typeof(TParameter), commandType);
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

    public async Task<ScalarValue> ExecuteScalarAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Scalar, commandText, dbParameters, typeof(TParameter), commandType);
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
                DisposeCommand(commandResult.Value);
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
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
                var b = behavior.HasValue ? behavior.Value | CommandBehavior.CloseConnection : CommandBehavior.CloseConnection;
                reader = await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                reader = await r.Command.ExecuteReaderAsync(behavior ?? CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
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

    public async Task<MultipleResult> QueryMultipleAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
    {
        var ctx = new SqlExecuteContext(ExecuteMethod.Reader, commandText, dbParameters, typeof(TParameter), commandType);
        CommandResult commandResult;
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
            if (commandResult.NeedToReturn)
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
            return new(new InternalDataReader(reader, commandResult, Pool));
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
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataSet, commandText, dbParameters, typeof(TParameter), commandType);
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

    public async Task<DataTable> ExecuteDataTableAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string commandText, TParameter dbParameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var ctx = new SqlExecuteContext(ExecuteMethod.DataTable, commandText, dbParameters, typeof(TParameter), commandType);
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

    //    private readonly static ConcurrentDictionary<Type, Action<DbCommand>?> commandInitCache = [];
    //    internal static Action<DbCommand>? GetInit(DbCommand commandObject)
    //    {
    //        if (commandObject is null)
    //        {
    //            return null;
    //        }

    //        var commandType = commandObject.GetType();
    //        if (!commandInitCache.TryGetValue(commandType, out var action))
    //        {
    //            MethodInfo? setBindName = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
    //            MethodInfo? setInit = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));
    //            if (setBindName is null && setInit is null)
    //            {
    //                return null;
    //            }
    //            /*
    //             * (DbCommand cmd) => {
    //             *     (OracleCommand)cmd.set_BindByName(true);
    //             *     (OracleCommand)cmd.set_InitialLONGFetchSize(-1);
    //             * }
    //             */
    //            ParameterExpression cmdExp = Expression.Parameter(typeof(DbCommand), "cmd");
    //            List<Expression> body = [];
    //            if (setBindName != null)
    //            {
    //                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
    //                MethodCallExpression setter1Exp = Expression.Call(convertedCmdExp, setBindName, Expression.Constant(true, typeof(bool)));
    //                body.Add(setter1Exp);
    //            }
    //            if (setInit != null)
    //            {
    //                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
    //                MethodCallExpression setter2Exp = Expression.Call(convertedCmdExp, setInit, Expression.Constant(-1, typeof(int)));
    //                body.Add(setter2Exp);
    //            }
    //            var lambda = Expression.Lambda<Action<DbCommand>>(Expression.Block(body), cmdExp);
    //            action = lambda.Compile();
    //            commandInitCache.TryAdd(commandType, action);
    //        }
    //        return action;
    //    }

    //    internal static MethodInfo? GetBasicPropertySetter(
    //#if NET8_0_OR_GREATER
    //    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
    //#endif
    //        Type declaringType, string name, Type expectedType)
    //    {
    //        PropertyInfo? property = declaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
    //        if (property != null && property.CanWrite && property.PropertyType == expectedType && property.GetIndexParameters().Length == 0)
    //        {
    //            return property.GetSetMethod();
    //        }
    //        return null;
    //    }

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