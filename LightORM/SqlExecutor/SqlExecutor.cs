using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading.Tasks;
using LightORM.Cache;
namespace LightORM.SqlExecutor;

internal class SqlExecutor : ISqlExecutor, IDisposable
{
    public Action<string, object?>? DbLog { get; set; }
    public bool DisposeImmediately { get; set; }
    public DbConnectInfo ConnectInfo { get; private set; }
    /// <summary>
    /// 数据库事务
    /// </summary>
    public DbTransaction? DbTransaction { get; set; }
    public DbConnection DbConnection { get; private set; }
    public SqlExecutor(DbConnectInfo connectInfo)
    {
        ConnectInfo = connectInfo;
        DbConnection = connectInfo.DbProviderFactory.CreateConnection()!;
        DbConnection.ConnectionString = connectInfo.ConnectString;
    }
    public void BeginTran()
    {
        if (DbConnection.State != ConnectionState.Open)
        {
            DbConnection.Open();
        }
        DbTransaction ??= DbConnection.BeginTransaction();
    }
    public void CommitTran()
    {
        DbTransaction?.Commit();
        DbTransaction = null;
        DbConnection.Close();
    }
    public void RollbackTran()
    {
        DbTransaction?.Rollback();
        DbTransaction = null;
        DbConnection.Close();
    }

#if !NET45
    public async Task BeginTranAsync()
    {
        if (DbConnection.State != ConnectionState.Open)
        {
            await DbConnection.OpenAsync();
        }
        DbTransaction ??= await DbConnection.BeginTransactionAsync();
    }

    public async Task CommitTranAsync()
    {
        if (DbTransaction != null)
        {
            await DbTransaction.CommitAsync();
            DbTransaction = null;
        }
        await DbConnection.CloseAsync();
    }

    public async Task RollbackTranAsync()
    {
        if (DbTransaction != null)
        {
            await DbTransaction.RollbackAsync();
            DbTransaction = null;
        }
        await DbConnection.CloseAsync();
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

    public Task TryCloseAsync(bool needToClose)
    {
#if NET45
        if (needToClose)
            DbConnection?.Close();
        return Task.FromResult(true);

#else
        if (DbConnection != null && needToClose)
        {
            return DbConnection.CloseAsync();
        }
        return Task.CompletedTask;
#endif
    }

    public void TryClose(bool needToClose)
    {
        if (needToClose)
            DbConnection.Close();
    }


    private bool PrepareCommand(DbCommand command, DbConnection connection, DbTransaction? transaction, CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        var needToClose = false;
        GetInit(command.GetType())?.Invoke(command);
        if (DbConnection.State != ConnectionState.Open)
        {
            connection.Open();
            needToClose = true;
        }
        if (transaction != null)
        {
            command.Transaction = transaction;
            needToClose = false;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = command.GetDbParameterReader(dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }

        return needToClose;
    }

    public async Task<bool> PrepareCommandAsync(DbCommand command, DbConnection connection, DbTransaction? transaction, CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        GetInit(command.GetType())?.Invoke(command);
        var needToClose = false;
        if (DbConnection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            needToClose = true;
        }
        if (transaction != null)
        {
            command.Transaction = transaction;
            needToClose = false;
        }
        command.CommandText = commandText;
        command.CommandType = commandType;
        if (dbParameters != null)
        {
            var action = command.GetDbParameterReader(dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }
        return needToClose;
    }

    public int ExecuteNonQuery(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            return cmd.ExecuteNonQuery();
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            TryClose(needToClose);
        }
    }

    public T? ExecuteScalar<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            var obj = cmd.ExecuteScalar();
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            TryClose(needToClose);
        }
    }

    public DbDataReader ExecuteReader(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            if (needToClose)
            {
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            else
            {
                return cmd.ExecuteReader();
            }
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
        }
    }

    public DataSet ExecuteDataSet(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            TryClose(needToClose);
        }
        return ds;
    }

    public DataTable ExecuteDataTable(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            TryClose(needToClose);
        }
        return ds;
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            await TryCloseAsync(needToClose);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            var obj = await cmd.ExecuteScalarAsync();
            if (obj is DBNull || obj is null)
            {
                return default;
            }
            return ChangeType<T>(obj);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            await TryCloseAsync(needToClose);
        }
    }
    public async Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            if (needToClose)
            {
                return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            else
            {
                return await cmd.ExecuteReaderAsync();
            }
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
        }
    }

    public async Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            await TryCloseAsync(needToClose);
        }
        return ds;
    }

    public async Task<DataTable> ExecuteDataTableAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataTable();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter!.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            await TryCloseAsync(needToClose);
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
                if (DbConnection?.State == ConnectionState.Open)
                {
                    DbConnection.Close();
                }
                DbConnection?.Dispose();
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