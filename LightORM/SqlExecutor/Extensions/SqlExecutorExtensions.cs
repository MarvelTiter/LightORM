using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor.Extensions;

public static class SqlExecutorExtensions
{
    public static IEnumerable<T> Query<T>(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.DbTransaction = trans;
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
    public static async Task<IList<T>> QueryAsync<T>(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.DbTransaction = trans;
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType);
            var des = BuildDeserializer<T>(reader);
            List<T> list = new List<T>();
            while (await reader.ReadAsync())
            {
                list.Add((T)des.Invoke(reader));
            }
            return list;
        }
        finally
        {
            if (reader != null)
#if NET6_0_OR_GREATER
                await reader.CloseAsync();
#else
                reader.Close();
#endif
        }
    }
    public static IEnumerable<dynamic> Query(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        return Query<MapperRow>(self, sql, param, trans, commandType);
    }

    public static async Task<IList<dynamic>> QueryAsync(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        var list = await QueryAsync<MapperRow>(self, sql, param, trans, commandType);
        return list.Cast<dynamic>().ToList();
    }

    public static T? QuerySingle<T>(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        return Query<T>(self, sql, param, trans, commandType).FirstOrDefault();
    }

    public static async Task<T?> QuerySingleAsync<T>(this ISqlExecutor self, string sql, object? param = null, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
    {
        DbDataReader? reader = null;
        try
        {
            if (trans != null)
            {
                self.DbTransaction = trans;
            }
            reader = await self.ExecuteReaderAsync(sql, param, commandType);
            var des = BuildDeserializer<T>(reader);
            T? result = default;
            while (await reader.ReadAsync())
            {
                result = (T)des.Invoke(reader);
                break;
            }
            return result;
        }
        finally
        {
            if (reader != null)
#if NET6_0_OR_GREATER
                await reader.CloseAsync();
#else
                reader.Close();
#endif
        }
    }

    private static Func<IDataReader, object> BuildDeserializer<T>(DbDataReader reader)
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
                        return null;
                    }
                }
                var begin = returnNullIfFirstMissing ? 1 : 0;
                for (var iter = begin; iter < fieldCount; ++iter)
                {
                    object obj = r.GetValue(iter);
                    values[iter] = obj is DBNull ? null : obj;
                }
                return new MapperRow(table, values);
            };
    }

    private static T? GetValue<T>(object? val)
    {
        if (val is T t)
        {
            return t;
        }
        Type effectiveType = typeof(T);
        if (val == null && (!effectiveType.IsValueType || Nullable.GetUnderlyingType(effectiveType) != null))
        {
            return default;
        }

        try
        {
            Type conversionType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
            return (T)Convert.ChangeType(val, conversionType, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
