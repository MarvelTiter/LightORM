using LightORM.SqlExecutor;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static LightORM.SqlExecutor.SqlExecutor;

namespace LightORM;

//public static partial class SqlExecutorExtensions
//{
//    /// <summary>
//    /// 开启事务-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    /// <param name="isolationLevel"></param>
//    public static void BeginTran(this ISqlExecutor executor, string savePoint, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
//    {
//        var context = currentTransactionContext.Value ?? throw new InvalidOperationException("No active transaction to begin savepoint");
//        // 嵌套事务
//        context.NestLevel++;
//#if NET6_0_OR_GREATER
//        if (context.Transaction.SupportsSavepoints)
//        {
//            context.Transaction.Save($"savePoint{context.NestLevel}");
//        }
//#endif
//    }
//    /// <summary>
//    /// 提交事务-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    public static void CommitTran(this ISqlExecutor executor, string savePoint)
//    {

//    }

//    /// <summary>
//    /// 回滚事务-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    public static void RollbackTran(this ISqlExecutor executor, string savePoint)
//    {

//    }
//    /// <summary>
//    /// 开启事务异步-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    /// <param name="isolationLevel"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    public static Task BeginTranAsync(this ISqlExecutor executor, string savePoint, IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
//    {

//    }
//    /// <summary>
//    /// 提交事务异步-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    public static Task CommitTranAsync(this ISqlExecutor executor, string savePoint, CancellationToken cancellationToken = default)
//    {

//    }
//    /// <summary>
//    /// 回滚事务异步-savePoint
//    /// </summary>
//    /// <param name="executor"></param>
//    /// <param name="savePoint"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    public static Task RollbackTranAsync(this ISqlExecutor executor, string savePoint, CancellationToken cancellationToken = default)
//    {

//    }
//}

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
                yield return (T)des.Invoke(reader);
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
        return Query<MapperRow>(self, sql, param, trans, commandType);
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
                result = (T)des.Invoke(reader);
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
                list.Add((T)des.Invoke(reader));
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
        var list = await QueryListAsync<MapperRow>(self, sql, param, trans, commandType, cancellationToken);
        return [.. list.Cast<dynamic>()];
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
                yield return (T)des.Invoke(reader);
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
        var datas = QueryAsync<MapperRow>(self, sql, param, trans, commandType, cancellationToken);
        await foreach (var item in datas.WithCancellation(cancellationToken))
        {
            yield return item;
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
                result = (T)des.Invoke(reader);
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
    internal static Func<IDataReader, object> BuildDeserializer<T>(this DbDataReader reader)
    {

        if (typeof(T) == typeof(object) || typeof(T) == typeof(MapperRow))
        {
            return GetMapperRowDeserializer<T>(reader, false);
        }
        return ExpressionBuilder.BuildDeserializer<T>(reader);
    }

    internal static Func<IDataReader, object> GetMapperRowDeserializer<T>(IDataRecord reader, bool returnNullIfFirstMissing)
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
