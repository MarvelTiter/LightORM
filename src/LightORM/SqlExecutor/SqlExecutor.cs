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
    private static readonly AsyncLocal<TransactionContext?> currentTransactionContext = new AsyncLocal<TransactionContext?>();
    public string Id { get; set; }
    private static readonly ConcurrentDictionary<IDatabaseProvider, ConnectionPool> pools = [];
    private static readonly ConcurrentDictionary<IDatabaseProvider, int> poolSizes = [];
    public Action<string, object?>? DbLog { get; set; }
    public IDatabaseProvider Database { get; private set; }
    /// <summary>
    /// 数据库事务
    /// </summary>
    public DbTransaction? DbTransaction
    {
        get => currentTransactionContext.Value?.Transaction;
    }

    //public DbConnection DbConnection { get; private set; }
    private readonly ConnectionPool pool;

    public SqlExecutor(IDatabaseProvider database, int poolSize, string? id = null)
    {
        Database = database;
        _ = poolSizes.GetOrAdd(database, poolSize);
        pool = pools.GetOrAdd(database, db =>
        {
            poolSizes.TryGetValue(db, out var size);
            return new ConnectionPool(() =>
            {
                var conn = db.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = db.MasterConnectionString;
                return conn;
            }, size);
        });
        Id = id ?? Guid.NewGuid().ToString();
    }

    public SqlExecutor(IDatabaseProvider database, string? id = null)
    {
        Database = database;
        pool = pools.GetOrAdd(database, db =>
        {
            poolSizes.TryGetValue(db, out var size);
            return new ConnectionPool(() =>
            {
                var conn = db.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = db.MasterConnectionString;
                return conn;
            }, size);
        });
        Id = id ?? Guid.NewGuid().ToString();
    }

    private DbConnection GetConnection()
    {
        var conn = Database.DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = Database.MasterConnectionString;
        return conn;
    }
    // 事务上下文类
    public class TransactionContext(DbTransaction transaction)
    {
        public DbTransaction Transaction { get; } = transaction;
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

        if (currentTransactionContext.Value != null)
            throw new InvalidOperationException("Already in a transaction context");

        currentTransactionContext.Value = new TransactionContext(externalTransaction)
        {
            IsExternal = true
        };
    }
    public void BeginTran(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        var context = currentTransactionContext.Value;

        if (context == null)
        {
            // 新事务
            var conn = pool.Get();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            var transaction = isolationLevel == IsolationLevel.Unspecified
                ? conn.BeginTransaction()
                : conn.BeginTransaction(isolationLevel);
            currentTransactionContext.Value = new TransactionContext(transaction);
        }
        else
        {
            // 嵌套事务
            context.NestLevel++;
#if NET6_0_OR_GREATER
            if (context.Transaction.SupportsSavepoints)
            {
                context.Transaction.Save($"savePoint{context.NestLevel}");
            }
#endif
        }
        Debug.WriteLine($"BeginTran： {currentTransactionContext.Value?.Id}");
    }
    public void CommitTran()
    {
        var context = currentTransactionContext.Value ??
            throw new InvalidOperationException("No active transaction to commit");

        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            context.Transaction.Commit();
            Debug.WriteLine($"CommitTran： {context.Id}");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }
    public void RollbackTran()
    {
        var context = currentTransactionContext.Value ??
             throw new InvalidOperationException("No active transaction to rollback");
        if (context.NestLevel > 0)
        {
#if NET6_0_OR_GREATER
            if (context.Transaction.SupportsSavepoints)
            {
                context.Transaction.Rollback($"savePoint{context.NestLevel}");
            }
#endif
            context.NestLevel--;
            return;
        }
        try
        {
            context.Transaction.Rollback();
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

    private void DisposeTransactionContext(bool forceDispose = false)
    {
        var context = currentTransactionContext.Value;
        if (context == null) return;
        if (context.NestLevel > 0)
        {
            if (forceDispose)
            {
                throw new InvalidOperationException("未完成提交就释放对象");
            }
            return;
        }
        // 内部事务创建的事务上下文
        if (!context.IsExternal)
        {
            var conn = context.Transaction.Connection;
            context.Transaction.Dispose();
            if (conn is not null)
            {
                conn.Close();
                pool.Return(conn);
            }
        }
        currentTransactionContext.Value = null;

    }

#if NET6_0_OR_GREATER
    public async Task BeginTranAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        var context = currentTransactionContext.Value;

        if (context == null)
        {
            // 新事务
            var conn = pool.Get();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            var transaction = await (isolationLevel == IsolationLevel.Unspecified
                ? conn.BeginTransactionAsync(cancellationToken)
                : conn.BeginTransactionAsync(isolationLevel, cancellationToken));

            currentTransactionContext.Value = new TransactionContext(transaction);
        }
        else
        {
            // 嵌套事务
            context.NestLevel++;
            context.Transaction.Save($"savePoint{context.NestLevel}");
        }
        Debug.WriteLine($"BeginTranAsync： {currentTransactionContext.Value?.Id}");
    }

    public async Task CommitTranAsync(CancellationToken cancellationToken = default)
    {
        var context = currentTransactionContext.Value ??
           throw new InvalidOperationException("No active transaction to commit");

        if (context.NestLevel > 0)
        {
            // 嵌套事务只减少计数器
            context.NestLevel--;
            return;
        }

        // 最外层事务提交
        try
        {
            await context.Transaction.CommitAsync(cancellationToken);
            Debug.WriteLine($"CommitTranAsync： {context.Id}");
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

    public async Task RollbackTranAsync(CancellationToken cancellationToken = default)
    {
        var context = currentTransactionContext.Value ??
             throw new InvalidOperationException("No active transaction to rollback");

        try
        {
            if (context.NestLevel > 0)
            {
                await context.Transaction.RollbackAsync($"savePoint{context.NestLevel}", cancellationToken);
                context.NestLevel--;
            }
            else
            {
                await context.Transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            DisposeTransactionContext();
        }
    }

#else
    public Task BeginTranAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        BeginTran(isolationLevel);
        return Task.FromResult(true);
    }

    public Task CommitTranAsync(CancellationToken cancellationToken = default)
    {
        CommitTran();
        return Task.FromResult(true);
    }

    public Task RollbackTranAsync(CancellationToken cancellationToken = default)
    {
        RollbackTran();
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
            pool.Return(result.Connection);
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
        var context = currentTransactionContext.Value;
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
            conn = pool.Get();
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
        var context = currentTransactionContext.Value;
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
            conn = pool.Get();
            needToReturn = true;
        }

        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
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
                return new InternalDataReader(reader, r, pool);
            }
            else
            {
                var reader = r.Command.ExecuteReader();
                return new InternalDataReader(reader, r, pool);
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
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken);
        try
        {
            return await r.Command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            DisposeCommand(r);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken);
        try
        {
            var obj = await r.Command.ExecuteScalarAsync(cancellationToken);
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
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken);
        try
        {
            if (r.NeedToReturn)
            {
                var reader = await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
                return new InternalDataReader(reader, r, pool);
            }
            else
            {
                var reader = await r.Command.ExecuteReaderAsync(cancellationToken);
                return new InternalDataReader(reader, r, pool);
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
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken);
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
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters, cancellationToken);
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
                DisposeTransactionContext(disposing);
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