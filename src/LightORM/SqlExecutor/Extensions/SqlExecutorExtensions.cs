using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightORM;

public static partial class SqlExecutorExtensions
{
    //public static DbConnection GetConnection(this ISqlExecutor executor)
    //{
    //    var conn = executor.Database.DbProviderFactory.CreateConnection()!;
    //    conn.ConnectionString = executor.Database.MasterConnectionString;
    //    return conn;
    //}
    public static IEnumerable<T> Query<T>(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = self.ExecuteReader(sql, param, commandType);
            var des = BuildDeserializer<T>(reader);
            while (reader.Read())
            {
                yield return des.Invoke(reader);
            }
        }
        finally
        {
            reader?.Close();
        }
    }
    public static IEnumerable<dynamic> Query(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = self.ExecuteReader(sql, param, commandType);
            var des = DynamicDeserializer(reader);
            while (reader.Read())
            {
                yield return des.Invoke(reader);
            }
        }
        finally
        {
            reader?.Close();
        }
    }
    public static T? QuerySingle<T>(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = self.ExecuteReader(sql, param, commandType);
            var des = BuildDeserializer<T>(reader);
            T? result = default;
            if (reader.Read())
            {
                result = des.Invoke(reader);
            }
            return result;
        }
        finally
        {
            reader?.Close();
        }
    }

    public static async Task<IList<T>> QueryListAsync<T>(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text
        , CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType, cancellationToken);
            var des = BuildDeserializer<T>(reader);
            List<T> list = [];
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                list.Add(des.Invoke(reader));
            }
            return list;
        }
        finally
        {
#if NET6_0_OR_GREATER
            if (reader is not null)
            {
                await reader.CloseAsync();
            }
#else
            reader?.Close();
#endif
        }
    }

    public static async Task<IList<dynamic>> QueryListAsync(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text
        , CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType, cancellationToken);
            var des = DynamicDeserializer(reader);
            List<object> list = [];
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                list.Add(des.Invoke(reader));
            }
            return list;
        }
        finally
        {
#if NET6_0_OR_GREATER
            if (reader is not null)
            {
                await reader.CloseAsync();
            }
#else
            reader?.Close();
#endif
        }
    }

    public static async IAsyncEnumerable<T> QueryAsync<T>(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text
        , [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType, cancellationToken);
            var des = BuildDeserializer<T>(reader);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                yield return des.Invoke(reader);
            }
        }
        finally
        {
#if NET6_0_OR_GREATER
            if (reader is not null)
            {
                await reader.CloseAsync();
            }
#else
            reader?.Close();
#endif
        }
    }

    public static async IAsyncEnumerable<dynamic> QueryAsync(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text
        , [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType, cancellationToken);
            var des = DynamicDeserializer(reader);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                yield return des.Invoke(reader);
            }
        }
        finally
        {
#if NET6_0_OR_GREATER
            if (reader is not null)
            {
                await reader.CloseAsync();
            }
#else
            reader?.Close();
#endif
        }
    }

    public static async Task<T?> QuerySingleAsync<T>(this ISqlExecutor self
        , string sql
        , object? param = null
        , DbTransaction? trans = null
        , CommandType commandType = CommandType.Text
        , CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.UseExternalTransaction(trans);
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType, cancellationToken);
            var des = BuildDeserializer<T>(reader);
            T? result = default;
            if (await reader.ReadAsync(cancellationToken))
            {
                result = des.Invoke(reader);
            }
            return result;
        }
        finally
        {
#if NET6_0_OR_GREATER
            if (reader is not null)
            {
                await reader.CloseAsync();
            }
#else
            reader?.Close();
#endif
        }
    }
    internal static Func<IDataReader, T> BuildDeserializer<T>(this DbDataReader reader)
    {
        return ExpressionBuilder.BuildDeserializer<T>(reader);
    }

    internal static Func<IDataReader, object> DynamicDeserializer(this DbDataReader reader)
    {
        return GetMapperRowDeserializer(reader, false);
    }

    internal static Func<IDataReader, object> GetMapperRowDeserializer(IDataRecord reader, bool returnNullIfFirstMissing)
    {
        var fieldCount = reader.FieldCount;

        MapperTable? table = null;

        return
            r =>
            {
                if (table == null)
                {
                    string[] names = new string[fieldCount];
                    for (int i = 0; i < fieldCount; i++)
                    {
                        string rawName = r.GetName(i).ToUpper();
                        names[i] = rawName;//(nameMap.ContainsKey(rawName) ? nameMap[rawName] : rawName);
                    }
                    table = new MapperTable(names);
                }

                var values = new object[fieldCount];

                if (returnNullIfFirstMissing)
                {
                    values[0] = r.GetValue(0);
                    if (values[0] is DBNull)
                    {
                        return null!;
                    }
                }
                var begin = returnNullIfFirstMissing ? 1 : 0;
                for (var iter = begin; iter < fieldCount; ++iter)
                {
                    object obj = r.GetValue(iter);
                    values[iter] = obj is DBNull ? null! : obj;
                }
                return new MapperRow(table, values);
            };
    }
}
