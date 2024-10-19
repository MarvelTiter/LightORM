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

internal class ConnectionPool : IDisposable
{
    private readonly Func<DbConnection> _createFunc;
    private readonly int _maxCapacity;
    private int _numItems;
    private protected readonly ConcurrentQueue<DbConnection> _items = new();
    private protected DbConnection? _fastItem;
    private bool disposedValue;

    public ConnectionPool(Func<DbConnection> func, int maxCapacity)
    {
        _createFunc = func;
        _maxCapacity = maxCapacity;
    }
    public DbConnection Get()
    {
        DbConnection? item = _fastItem;
        if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_items.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _numItems);
                return item;
            }
            return _createFunc();
        }
        return item;
    }

    public void Return(DbConnection connection)
    {
        if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, connection, null) != null)
        {
            if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
            {
                _items.Enqueue(connection);
                return;
            }
            Interlocked.Decrement(ref _numItems);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Dispose();
            }
            else
            {
                connection.StateChange += Connection_StateChange;
            }
        }
    }

    private static void Connection_StateChange(object sender, StateChangeEventArgs e)
    {
        if (e.CurrentState == ConnectionState.Closed)
        {
            var conn = (DbConnection)sender;
            conn.StateChange -= Connection_StateChange;
            conn.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                while (_items.TryDequeue(out var i))
                {
                    i.Dispose();
                }
                _fastItem?.Dispose();
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

internal class SqlExecutor : ISqlExecutor, IDisposable
{
    public Action<string, object?>? DbLog { get; set; }
    public IDatabaseProvider Database { get; private set; }
    /// <summary>
    /// 数据库事务
    /// </summary>
    public DbTransaction? DbTransaction { get; set; }
    private DbConnection connection;
    private bool useTran;
    //public DbConnection DbConnection { get; private set; }
    private readonly ConnectionPool pool;
    private readonly int poolSize;

    public SqlExecutor(IDatabaseProvider database, int poolSize)
    {
        Database = database;
        this.poolSize = poolSize;
        pool = new ConnectionPool(GetConnection, poolSize);
        connection = pool.Get();
    }

    private DbConnection GetConnection()
    {
        var conn = Database.DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = Database.MasterConnectionString;
        return conn;
    }

    public void BeginTran()
    {
        DbTransaction = connection.BeginTransaction();
    }
    public void CommitTran()
    {
        DbTransaction?.Commit();
        DbTransaction = null;
    }
    public void RollbackTran()
    {
        DbTransaction?.Rollback();
        DbTransaction = null;
    }

#if NET6_0_OR_GREATER
    public async Task BeginTranAsync()
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        DbTransaction ??= await connection.BeginTransactionAsync();
    }

    public async Task CommitTranAsync()
    {
        if (DbTransaction != null)
        {
            await DbTransaction.CommitAsync();
            DbTransaction = null;
        }
    }

    public async Task RollbackTranAsync()
    {
        if (DbTransaction != null)
        {
            await DbTransaction.RollbackAsync();
            DbTransaction = null;
        }
    }

#else
    public Task BeginTranAsync()
    {
        BeginTran();
        return Task.FromResult(true);
    }

    public Task CommitTranAsync()
    {
        CommitTran();
        return Task.FromResult(true);
    }

    public Task RollbackTranAsync()
    {
        RollbackTran();
        return Task.FromResult(true);
    }

#endif

    private async Task TryCloseAsync(CommandResult result)
    {
        result.Command.Parameters.Clear();
        result.Command.Dispose();
#if NET6_0_OR_GREATER
        if (result.NeedToClose)
        {
            await result.Connection.CloseAsync();
        }
        if (result.NeedToReturn)
        {
            pool.Return(result.Connection);
        }
#else
        if (result.NeedToClose)
        {
            result.Connection.Close();
        }
        if (result.NeedToReturn)
        {
            pool.Return(result.Connection);
        }
        await Task.CompletedTask;
#endif
    }

    private void TryClose(CommandResult result)
    {
        result.Command.Parameters.Clear();
        result.Command.Dispose();
        if (result.NeedToClose)
        {
            result.Connection.Close();
        }
        if (result.NeedToReturn)
        {
            pool.Return(result.Connection);
        }
    }

    private struct CommandResult(DbCommand command, DbConnection connection, bool needToClose, bool needToReturn)
    {
        public DbCommand Command { get; } = command;
        public DbConnection Connection { get; } = connection;
        public bool NeedToClose { get; } = needToClose;
        public bool NeedToReturn { get; } = needToReturn;
    }

    private CommandResult PrepareCommand(CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        bool isSelect = commandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || commandText.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
        var conn = connection;
        var needToClose = false;
        var needToReturn = false;
        if (isSelect)
        {
            conn = pool.Get();
            needToReturn = true;
        }
        var command = conn.CreateCommand();
        GetInit(command.GetType())?.Invoke(command);
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
            needToClose = true;
        }
        if (!isSelect && DbTransaction != null)
        {
            command.Transaction = DbTransaction;
            needToClose = false;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = DbParameterReader.GetDbParameterReader(connection.ConnectionString, commandText, dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }

        return new(command, conn, needToClose, needToReturn);
    }

    private async Task<CommandResult> PrepareCommandAsync(CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        bool isSelect = commandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || commandText.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
        var conn = connection;
        var needToClose = false;
        var needToReturn = false;
        if (isSelect)
        {
            conn = pool.Get();
            needToReturn = true;
        }
        var command = conn.CreateCommand();
        GetInit(command.GetType())?.Invoke(command);
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
            needToClose = true;
        }
        if (!isSelect && DbTransaction != null)
        {
            command.Transaction = DbTransaction;
            needToClose = false;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = DbParameterReader.GetDbParameterReader(connection.ConnectionString, commandText, dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }
        return new(command, conn, needToClose, needToReturn);
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
            TryClose(r);
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
            TryClose(r);
        }
    }

    public DbDataReader ExecuteReader(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = PrepareCommand(commandType, commandText, dbParameters);
        try
        {
            if (r.NeedToClose)
            {
                return r.Command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            else
            {
                return r.Command.ExecuteReader();
            }
        }
        finally
        {
            r.Command.Parameters.Clear();
            r.Command.Dispose();
            if (r.NeedToReturn)
                pool.Return(r.Connection);
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
            TryClose(r);
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
            TryClose(r);
        }
        return ds;
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters);
        try
        {
            return await r.Command.ExecuteNonQueryAsync();
        }
        finally
        {
            await TryCloseAsync(r);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters);
        try
        {
            var obj = await r.Command.ExecuteScalarAsync();
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        finally
        {
            await TryCloseAsync(r);
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters);
        try
        {
            if (r.NeedToClose)
            {
                return await r.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            else
            {
                return await r.Command.ExecuteReaderAsync();
            }
        }
        finally
        {
            r.Command.Parameters.Clear();
            r.Command.Dispose();
            if (r.NeedToReturn)
            {
                pool.Return(r.Connection);
            }
        }
    }

    public async Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            await TryCloseAsync(r);
        }
        return ds;
    }

    public async Task<DataTable> ExecuteDataTableAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = Database.DbProviderFactory.CreateDataAdapter();
        var r = await PrepareCommandAsync(commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = r.Command;
            adapter.Fill(ds);
        }
        finally
        {
            await TryCloseAsync(r);
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
        return new SqlExecutor(Database, poolSize);
    }


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
#if DEBUG
                Console.WriteLine("SqlExecutor disposing.........");
#endif
                DbTransaction?.Rollback();
                DbTransaction = null;
                pool.Dispose();
                connection?.Dispose();
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