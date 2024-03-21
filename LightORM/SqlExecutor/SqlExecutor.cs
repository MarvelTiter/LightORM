using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor;

internal class SqlExecutor : ISqlExecutor, IDisposable
{
    public Action<string, object?>? DbLog { get; set; }

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

    public async Task BeginTranAsync()
    {
#if NET45_OR_GREATER
        await Task.Yield();
        throw new NotSupportedException();
#else
        if (DbConnection.State != ConnectionState.Open)
        {
            await DbConnection.OpenAsync();
        }
        DbTransaction ??= await DbConnection.BeginTransactionAsync();
#endif
    }

    public void CommitTran()
    {
        DbTransaction!.Commit();
        DbTransaction = null;
        DbConnection.Close();
    }

    public async Task CommitTranAsync()
    {
#if NET45_OR_GREATER
        await Task.Yield();
        throw new NotSupportedException();
#else
        await DbTransaction!.CommitAsync();
        DbTransaction = null;
        await DbConnection.CloseAsync();
#endif
    }

    public void RollbackTran()
    {
        DbTransaction?.Rollback();
        DbTransaction = null;
        DbConnection.Close();
    }

    public async Task RollbackTranAsync()
    {
#if NET45_OR_GREATER
        await Task.Yield();
        throw new NotSupportedException();
#else
        if (DbTransaction != null)
        {
            await DbTransaction.RollbackAsync();
            DbTransaction = null;
            await DbConnection.CloseAsync();
        }
#endif
    }

    private bool PrepareCommand(DbCommand command, DbConnection connection, DbTransaction? transaction, CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        Console.WriteLine(commandText);
        var needToClose = false;
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
            var action = Cache.DbParameterReader.GetDbParameterReader(commandText, dbParameters.GetType());
            action?.Invoke(command, dbParameters);
        }

        return needToClose;
    }

    public async Task<bool> PrepareCommandAsync(DbCommand command, DbConnection connection, DbTransaction? transaction, CommandType commandType, string commandText, object? dbParameters)
    {
        DbLog?.Invoke(commandText, dbParameters);
        Console.WriteLine(commandText);
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
            var action = Cache.DbParameterReader.GetDbParameterReader(commandText, dbParameters.GetType());
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
            if (needToClose)
            {
                DbConnection.Close();
            }
        }
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
            if (needToClose)
            {
                DbConnection.Close();
            }
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
            if (needToClose)
            {
                DbConnection.Close();
            }
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
            if (needToClose)
            {
                DbConnection.Close();
            }
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

    public DataSet ExecuteDataSet(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = PrepareCommand(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            if (needToClose)
                DbConnection.Close();
        }
        return ds;
    }

    public async Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text)
    {
        var ds = new DataSet();
        using var adapter = ConnectInfo.DbProviderFactory.CreateDataAdapter();
        var cmd = DbConnection.CreateCommand();
        var needToClose = await PrepareCommandAsync(cmd, DbConnection, DbTransaction, commandType, commandText, dbParameters);
        try
        {
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            if (needToClose)
                DbConnection.Close();
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
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            if (needToClose)
                DbConnection.Close();
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
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
        }
        finally
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
            if (needToClose)
                DbConnection.Close();
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

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                DbTransaction?.Rollback();
                DbTransaction = null;
                if (DbConnection != null && DbConnection.State == ConnectionState.Open)
                {
                    DbConnection.Close();
                }
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