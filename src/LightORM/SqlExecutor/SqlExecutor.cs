using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading.Tasks;
using LightORM.Cache;
using System.Threading;
using System.Collections.Concurrent;
namespace LightORM.SqlExecutor;

internal partial class SqlExecutor : ISqlExecutor, IDisposable
{
    // 事务上下文管理
    internal static readonly ConcurrentDictionary<IDatabaseProvider, AsyncLocal<TransactionContext?>> AsyncLocalTransactionContexts = new();
    public string Id { get; set; }
    internal static readonly ConcurrentDictionary<IDatabaseProvider, ConnectionPool> Pools = [];
    private static readonly ConcurrentDictionary<IDatabaseProvider, int> PoolSizes = [];
    public Action<string, object?>? DbLog { get; set; }
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
    public ConnectionPool Pool { get; }

    public SqlExecutor(IDatabaseProvider database, int poolSize, string? id = null)
    {
        Database = database;
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

    public SqlExecutor(IDatabaseProvider database, string? id = null)
    {
        Database = database;
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
    public class TransactionContext(DbTransaction transaction)
    {
        public DbTransaction Transaction { get; set; } = transaction;
        public Stack<DbTransaction> TranscationStack { get; set; } = [];
        public int NestLevel { get; set; }
        public bool IsExternal { get; internal set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    // 使用外部事务
    public void UseExternalTransaction(DbTransaction externalTransaction)
    {
        if (externalTransaction == null)
            throw new ArgumentNullException(nameof(externalTransaction));

        if (CurrentTransactionContext.Value != null)
            throw new InvalidOperationException("Already in a transaction context");

        CurrentTransactionContext.Value = new TransactionContext(externalTransaction)
        {
            IsExternal = true
        };
    }
    public void InitTransactionContext()
    {
        CurrentTransactionContext.Value ??= new TransactionContext(null!);
    }

    public void InitAllTransactionContext(IEnumerable<IDatabaseProvider> databases)
    {
        foreach (var item in databases)
        {
            var ctx = AsyncLocalTransactionContexts.GetOrAdd(item, new AsyncLocal<TransactionContext?>());
            ctx.Value ??= new TransactionContext(null!);
        }
    }

    public void InitTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        var context = CurrentTransactionContext.Value;
        if (context == null)
        {
            // 新事务
            var conn = Pool.Get();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            var transaction = isolationLevel == IsolationLevel.Unspecified
                ? conn.BeginTransaction()
                : conn.BeginTransaction(isolationLevel);
            CurrentTransactionContext.Value = new TransactionContext(transaction);
        }
        Debug.WriteLine($"InitTransaction： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }
    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        if (CurrentTransactionContext.Value?.Transaction is null)
        {
            // 新事务
            CurrentTransactionContext.Value ??= new TransactionContext(null!);

            var conn = Pool.Get();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            var transaction = isolationLevel == IsolationLevel.Unspecified
                ? conn.BeginTransaction()
                : conn.BeginTransaction(isolationLevel);
            CurrentTransactionContext.Value.Transaction = transaction;
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
        Debug.WriteLine($"BeginTran： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }
    public void CommitTransaction()
    {
        var context = CurrentTransactionContext.Value ??
            throw new InvalidOperationException("No active transaction to commit");

        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLine($"CommitTran： {context.Id} -> {context.NestLevel}");
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            context.Transaction.Commit();
            Debug.WriteLine($"CommitTran： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }
    public void RollbackTransaction()
    {
        var context = CurrentTransactionContext.Value ??
             throw new InvalidOperationException("No active transaction to rollback");
        if (context.NestLevel > 0)
        {
            context.NestLevel--;
            Debug.WriteLine($"RollbackTran： {context.Id} -> {context.NestLevel}");
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
            Debug.WriteLine($"RollbackTran： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

    private void DisposeTransactionContext()
    {
        var context = CurrentTransactionContext.Value;
        if (context == null) return;
        //if (context.NestLevel > 0)
        //{
        //    if (forceDispose)
        //    {
        //        throw new InvalidOperationException("未完成提交就释放对象");
        //    }
        //    return;
        //}
        // 内部事务创建的事务上下文
        if (!context.IsExternal)
        {
            var conn = context.Transaction.Connection;
            context.Transaction.Dispose();
            if (conn is not null)
            {
                conn.Close();
                Pool.Return(conn);
            }
        }
        CurrentTransactionContext.Value = null;

    }

#if NET6_0_OR_GREATER
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        if (CurrentTransactionContext.Value?.Transaction is null)
        {
            // 新事务
            CurrentTransactionContext.Value ??= new(null!);
            var conn = Pool.Get();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            var transaction = isolationLevel == IsolationLevel.Unspecified
                ? await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
                : await conn.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

            CurrentTransactionContext.Value.Transaction = transaction;
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
        Debug.WriteLine($"BeginTranAsync： {CurrentTransactionContext.Value?.Id} -> {CurrentTransactionContext.Value?.NestLevel}");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value ??
           throw new InvalidOperationException("No active transaction to commit");

        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            Debug.WriteLine($"CommitTranAsync： {context.Id} -> {context.NestLevel}");
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            await context.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLine($"CommitTranAsync： {context.Id} -> finished");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value ??
             throw new InvalidOperationException("No active transaction to rollback");

        try
        {
            if (context.NestLevel > 0)
            {
                Debug.WriteLine($"RollbackTranAsync： {context.Id} -> {context.NestLevel}");
                context.NestLevel--;
                if (context.Transaction.SupportsSavepoints)
                {
                    await context.Transaction.RollbackAsync($"savePoint{context.NestLevel}", cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await context.Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                Debug.WriteLine($"RollbackTranAsync： {context.Id} -> finished");
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

    //    private async Task TryCloseAsync(CommandResult result)
    //    {
    //        result.Command.Parameters.Clear();
    //        result.Command.Dispose();
    //#if NET6_0_OR_GREATER
    //        if (result.NeedToReturn)
    //        {
    //            await result.Connection.CloseAsync();
    //            pool.Return(result.Connection);
    //        }
    //#else
    //        if (result.NeedToReturn)
    //        {
    //            result.Connection.Close();
    //            pool.Return(result.Connection);
    //        }
    //        await Task.CompletedTask;
    //#endif
    //    }

    private void DisposeCommand(CommandResult result)
    {
        result.Command.Parameters.Clear();
        result.Command.Dispose();
        if (result.NeedToReturn)
        {
            Pool.Return(result.Connection);
        }
    }

    private readonly struct CommandResult(DbCommand command, DbConnection connection, bool needToReturn)
    {
        public DbCommand Command { get; } = command;
        public DbConnection Connection { get; } = connection;
        public bool NeedToReturn { get; } = needToReturn;
    }

    private CommandResult PrepareCommand(CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        var context = CurrentTransactionContext.Value;
        DbConnection conn;
        bool needToReturn;
        if (context != null)
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

        return new(command, conn, needToReturn);
    }

    private async Task<CommandResult> PrepareCommandAsync(CommandType commandType, string commandText, object? dbParameters, CancellationToken cancellationToken = default)
    {
        DbLog?.Invoke(commandText, dbParameters);
        var context = CurrentTransactionContext.Value;
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
        return new(command, conn, needToReturn);
    }

    public int ExecuteNonQuery(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {

        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            return r.Command.ExecuteNonQuery();
        }
        finally
        {
            DisposeCommand(r);
        }
    }

    public T? ExecuteScalar<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            var obj = r.Command.ExecuteScalar();
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        finally
        {
            DisposeCommand(r);
        }
    }

    public DbDataReader ExecuteReader(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            if (r.NeedToReturn)
            {
                var reader = r.Command.ExecuteReader(CommandBehavior.CloseConnection);
                return new InternalDataReader(reader, r, Pool);
            }
            else
            {
                var reader = r.Command.ExecuteReader();
                return new InternalDataReader(reader, r, Pool);
            }
        }
        finally
        {

        }
    }

    public DataSet ExecuteDataSet(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            DisposeCommand(r);
        }
        return ds;
    }

    public DataTable ExecuteDataTable(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            r.Command.Parameters.Clear();
            r.Command.Dispose();
            DisposeCommand(r);
        }
        return ds;
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            return await r.Command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            DisposeCommand(r);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            var obj = await r.Command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        finally
        {
            DisposeCommand(r);
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            if (r.NeedToReturn)
            {
                var reader = await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
                return new InternalDataReader(reader, r, Pool);
            }
            else
            {
                var reader = await r.Command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                return new InternalDataReader(reader, r, Pool);
            }
        }
        finally
        {

        }
    }

    public async Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataSet();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            DisposeCommand(r);
        }
        return ds;
    }

    public async Task<DataTable> ExecuteDataTableAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            DisposeCommand(r);
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
        return new SqlExecutor(Database);
    }


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Debug.WriteLine("SqlExecutor disposing.........");
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