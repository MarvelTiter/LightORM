using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor
{
    public static partial class SqlExecutor
    {
        public static Task<int> ExecuteAsync(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalExecuteAsync(self, command);
        }

        public static Task<object> ExecuteScaleAsync(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalScaleAsync(self, command);
        }

        public static Task<DataTable> ExecuteTableAsync(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalExecuteTableAsync(self, command);
        }

        public static Task<IDataReader> ExecuteReaderAsync(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalReaderAsync(self, command, (reader, cacheinfo) =>
            {
                return Task.FromResult(reader);
            });
        }

        public static Task<List<T>> QueryAsync<T>(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            var result = InternalQueryAsync<T>(self, command);
            return result;
        }

        public static async Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            var result = await InternalQueryAsync<MapperRow>(self, command);
            return result;
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection self, string sql, object param = null, IDbTransaction trans = null, CommandType? commandType = CommandType.Text)
        {
            CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
            return InternalSingleAsync<T>(self, command);
        }

        public static async Task<bool> ExecuteTransAsync(this IDbConnection self, IList<string> sqls, IList<object> ps)
        {
            self.Open();
            var tran = self.BeginTransaction();
            try
            {
                if (sqls.Count != ps.Count)
                    throw new ArgumentException("SQL与参数不匹配");
                int success = 0;
                for (int i = 0; i < sqls.Count; i++)
                {
                    var sql = sqls[i];
                    var p = ps[i];
                    var affect = await self.ExecuteAsync(sql, p, tran);
                    success += affect > 0 ? 1 : 0;
                }
                if (success == sqls.Count)
                {
                    tran.Commit();
                    return true;
                }
                else
                {
                    tran.Rollback();
                    return false;
                }
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

        static DbCommand TryParse(this IDbCommand dbCommand)
        {
            if (dbCommand is DbCommand cmd)
            {
                return cmd;
            }
            throw new NotSupportedException("不支持异步");
        }
        private static async Task<List<T>> InternalQueryAsync<T>(IDbConnection conn, CommandDefinition command)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            DbCommand cmd = null;
            IDataReader reader = null;
            var wasClosed = conn.State == ConnectionState.Closed;
            try
            {
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
                if (wasClosed)
                    conn.Open();
                reader = await ExecuteReaderWithFlagsFallback(cmd, wasClosed, CommandBehavior.SingleResult);
                if (cacheInfo.Deserializer == null)
                {
                    cacheInfo.Deserializer = BuildDeserializer<T>(reader);
                }
                List<T> ret = new List<T>();
                while (reader.Read())
                {
                    var val = cacheInfo.Deserializer(reader);
                    ret.Add(GetValue<T>(val));
                }
                return ret;
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

            async Task<IDataReader> ExecuteReaderWithFlagsFallback(DbCommand c, bool close, CommandBehavior behavior)
            {
                try
                {
                    return await c.ExecuteReaderAsync(GetBehavior(close, behavior));
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

        private static Task<T> InternalSingleAsync<T>(IDbConnection conn, CommandDefinition command)
        {
            return InternalReaderAsync(conn, command, (reader, cacheInfo) =>
            {
                while (reader.Read())
                {
                    var val = cacheInfo.Deserializer(reader);
                    return Task.FromResult(GetValue<T>(val));
                }
                return Task.FromResult<T>(default);
            }, true);
        }

        private static Task<DataTable> InternalExecuteTableAsync(IDbConnection conn, CommandDefinition command)
        {
            return InternalReaderAsync(conn, command, (reader, cacheInfo) =>
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                return Task.FromResult(dt);
            });
        }

        private static Task<object> InternalScaleAsync(IDbConnection conn, CommandDefinition command)
        {
            return InternalExectorAsync(conn, command, cmd =>
            {
                var obj = cmd.ExecuteScalarAsync();
                return (obj);
            });
        }

        private static Task<int> InternalExecuteAsync(IDbConnection conn, CommandDefinition command)
        {
            return InternalExectorAsync(conn, command, cmd =>
            {
                var count = cmd.ExecuteNonQueryAsync();
                return count;
            });
        }
        private static async Task<T> InternalReaderAsync<T>(IDbConnection conn, CommandDefinition command, Func<IDataReader, CacheInfo, Task<T>> func, bool singleRow = false)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            var wasClosed = conn.State == ConnectionState.Closed;
            DbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                if (wasClosed)
                    conn.Open();
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
                // singleRow 加上 CommandBehavior.SingleRow
                reader = await cmd.ExecuteReaderAsync(singleRow ? CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.SingleRow : CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
                if (cacheInfo.Deserializer == null)
                {
                    cacheInfo.Deserializer = BuildDeserializer<T>(reader);
                }
                return await func(reader, cacheInfo);
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

        private static Task<T> InternalExectorAsync<T>(IDbConnection conn, CommandDefinition command, Func<DbCommand, Task<T>> func)
        {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(object), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            var wasClosed = conn.State == ConnectionState.Closed;
            DbCommand cmd = null;
            try
            {
                if (wasClosed)
                    conn.Open();
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
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
    }
}
