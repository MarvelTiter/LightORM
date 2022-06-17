using MDbContext.SqlExecutor.Service;
using MDbEntity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor
{    
    public static partial class SqlExecutor
    {

        public static int Execute(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalExecute(self, command);
        }

        public static object ExecuteScale(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalScale(self, command);
        }

        public static DataTable ExecuteTable(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalExecuteTable(self, command);
        }

        public static IDataReader ExecuteReader(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalReader(self, command, (reader, cacheinfo) =>
             {
                 return reader;
             });
        }

        public static IEnumerable<T> Query<T>(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            var result = InternalQuery<T>(self, command);
            return result;
        }

        public static IEnumerable<dynamic> Query(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            var result = InternalQuery<MapperRow>(self, command);
            return result;
        }

        public static T QuerySingle<T>(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalSingle<T>(self, command);
        }

        public static bool ExecuteTrans(this IDbConnection self, IList<string> sqls, IList<object> ps)
        {
            self.Open();
            var tran = self.BeginTransaction();
            try
            {
                if (sqls.Count != ps.Count)
                    throw new ArgumentException("SQL与参数不匹配");
                for (int i = 0; i < sqls.Count; i++)
                {
                    var sql = sqls[i];
                    var p = ps[i];
                    self.Execute(sql, p, tran);
                }
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                self.Close();
                throw ex;
            }
            finally
            {
                self.Close();
            }
        }

        private static IEnumerable<T> InternalQuery<T>(IDbConnection conn, CommandDefinition command)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            IDbCommand cmd = null;
            IDataReader reader = null;
            var wasClosed = conn.State == ConnectionState.Closed;
            try
            {
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader);
                if (wasClosed)
                    conn.Open();
                reader = ExecuteReaderWithFlagsFallback(cmd, wasClosed, CommandBehavior.SingleResult);
                if (cacheInfo.Deserializer == null)
                {
                    cacheInfo.Deserializer = BuildDeserializer<T>(reader);
                }
                while (reader.Read())
                {
                    var val = cacheInfo.Deserializer(reader);
                    yield return GetValue<T>(val);
                }
            }
            finally
            {
                // dispose
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            cmd.Cancel();
                        }
                        catch
                        {
                        }
                    }
                    reader.Dispose();
                }
                if (wasClosed)
                {
                    conn.Close();
                }
                cmd?.Dispose();
            }

            IDataReader ExecuteReaderWithFlagsFallback(IDbCommand c, bool close, CommandBehavior behavior)
            {
                try
                {
                    return c.ExecuteReader(GetBehavior(close, behavior));
                }
                catch (ArgumentException ex)
                {
                    throw;
                }
            }
            CommandBehavior GetBehavior(bool close, CommandBehavior @default)
            {
                return (close ? (@default | CommandBehavior.CloseConnection) : @default);
            }
        }

        private static T InternalSingle<T>(IDbConnection conn, CommandDefinition command)
        {
            return InternalReader(conn, command, (reader, cacheInfo) =>
            {
                while (reader.Read())
                {
                    var val = cacheInfo.Deserializer(reader);
                    return GetValue<T>(val);
                }
                return default;
            }, true);
        }

        private static DataTable InternalExecuteTable(IDbConnection conn, CommandDefinition command)
        {
            return InternalReader(conn, command, (reader, cacheInfo) =>
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                return dt;
            });
        }

        private static object InternalScale(IDbConnection conn, CommandDefinition command)
        {
            return InternalExector(conn, command, cmd =>
            {
                var obj = cmd.ExecuteScalar();
                return obj;
            });
        }

        private static int InternalExecute(IDbConnection conn, CommandDefinition command)
        {
            return InternalExector(conn, command, cmd =>
            {
                var count = cmd.ExecuteNonQuery();
                return count;
            });
        }

        private static T InternalReader<T>(IDbConnection conn, CommandDefinition command, Func<IDataReader, CacheInfo, T> func, bool singleRow = false)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            var wasClosed = conn.State == ConnectionState.Closed;
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                if (wasClosed)
                    conn.Open();
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader);
                // singleRow 加上 CommandBehavior.SingleRow
                reader = cmd.ExecuteReader(singleRow ? CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.SingleRow : CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
                if (cacheInfo.Deserializer == null)
                {
                    cacheInfo.Deserializer = BuildDeserializer<T>(reader);
                }
                return func(reader, cacheInfo);
            }
            finally
            {
                // dispose
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try
                        {
                            cmd.Cancel();
                        }
                        catch
                        {
                        }
                    }
                    reader.Dispose();
                }
                if (wasClosed)
                {
                    conn.Close();
                }
                cmd?.Dispose();
            }
        }

        private static T InternalExector<T>(IDbConnection conn, CommandDefinition command, Func<IDbCommand, T> func)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(object), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            var wasClosed = conn.State == ConnectionState.Closed;
            IDbCommand cmd = null;
            try
            {
                if (wasClosed)
                    conn.Open();
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader);
                return func(cmd);
            }
            finally
            {
                // dispose               
                if (wasClosed)
                {
                    conn.Close();
                }
                cmd?.Dispose();
            }
        }

        private static Func<IDataReader, object> BuildDeserializer<T>(IDataReader reader)
        {

            if (typeof(T) == typeof(object) || typeof(T) == typeof(MapperRow))
            {
                return GetMapperRowDeserializer<T>(reader, false);
            }
            IDeserializer des = new ExpressionBuilder();
            return des.BuildDeserializer<T>(reader);
        }

        internal static Func<IDataReader, object> GetMapperRowDeserializer<T>(IDataRecord reader, bool returnNullIfFirstMissing)
        {
            var fieldCount = reader.FieldCount;

            MapperTable table = null;

            return
                r =>
                {
                    if (table == null)
                    {
                        Type entityType = typeof(T);
                        PropertyInfo[] props = entityType.GetProperties();
                        Dictionary<string, string> nameMap = new Dictionary<string, string>();
                        foreach (PropertyInfo prop in props)
                        {
                            var attr = prop.GetCustomAttribute<ColumnNameAttribute>();
                            var field = attr?.Name ?? prop.Name;
                            nameMap.Add(field, prop.Name);
                        }
                        string[] names = new string[fieldCount];
                        for (int i = 0; i < fieldCount; i++)
                        {
                            string rawName = r.GetName(i);
                            names[i] = nameMap.ContainsKey(rawName) ? nameMap[rawName] : rawName;
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

        private static T GetValue<T>(object val)
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
}
