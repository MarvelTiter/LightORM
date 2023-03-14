#if NET40
#else
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor
{

	public static partial class SqlExecutor
	{
		public static Task<int> ExecuteAsync(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => ExecuteAsync(self, sql, CancellationToken.None, param, trans, commandType);
		public static Task<int> ExecuteAsync(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			return InternalExecuteNonQueryAsync(self, command, token);
		}

		public static Task<object> ExecuteScaleAsync(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => ExecuteScaleAsync(self, sql, CancellationToken.None, param, trans, commandType);
		public static Task<object> ExecuteScaleAsync(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			return InternalExecuteScalarAsync(self, command, token);
		}

		public static Task<DataTable> ExecuteTableAsync(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => ExecuteTableAsync(self, sql, CancellationToken.None, param, trans, commandType);
		public static Task<DataTable> ExecuteTableAsync(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			return InternalExecuteTableAsync(self, command, token);
		}

		public static Task ExecuteReaderAsync(this IDbConnection self, string sql, object? param = null, Func<IDataReader, Task>? func = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => ExecuteReaderAsync(self, sql, CancellationToken.None, param, func, trans, commandType);
		public static async Task ExecuteReaderAsync(this IDbConnection self, string sql, CancellationToken token, object? param = null, Func<IDataReader, Task>? func = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			await InternalReaderAsync(self, command, async (reader, cacheinfo) =>
			{
				if (func != null)
				{
					await func.Invoke(reader);
					return true;
				}
				return false;
			}, token);
		}
		public static async Task<IList<T>> ThreadPoolQueryAsync<T>(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			var result = await ThreadPoolQueryAsync<T>(self, command);
			return result;
		}
		public static Task<IList<T>> QueryAsync<T>(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => QueryAsync<T>(self, sql, CancellationToken.None, param, trans, commandType);

		public static async Task<IList<T>> QueryAsync<T>(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			var result = await InternalQueryAsync<T>(self, command, token);
			return result;
		}
		public static Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => QueryAsync(self, sql, CancellationToken.None, param, trans, commandType);
		public static async Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			var result = await InternalQueryAsync<MapperRow>(self, command, token);
			return result;
		}

		public static Task<T?> QuerySingleAsync<T>(this IDbConnection self, string sql, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text) => QuerySingleAsync<T>(self, sql, CancellationToken.None, param, trans, commandType);
		public static Task<T?> QuerySingleAsync<T>(this IDbConnection self, string sql, CancellationToken token, object? param = null, IDbTransaction? trans = null, CommandType? commandType = CommandType.Text)
		{
			CommandDefinition command = new CommandDefinition(sql, param, trans, commandType);
			return InternalSingleAsync<T>(self, command, token);
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

		private static Task<IList<T>> ThreadPoolQueryAsync<T>(IDbConnection conn, CommandDefinition command)
		{
			TaskCompletionSource<IList<T>> tcs = new TaskCompletionSource<IList<T>>();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					var list = InternalQuery<T>(conn, command).ToList();
					tcs.SetResult(list);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			return tcs.Task;
		}

		private static async Task<IList<T>> InternalQueryAsync<T>(IDbConnection conn, CommandDefinition command, CancellationToken token)
		{
			// 缓存
			var parameter = command.Parameters;
			Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
			CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
			// 读取
			DbCommand? cmd = null;
			DbDataReader? reader = null;
			var wasClosed = conn.State == ConnectionState.Closed;
			try
			{
				cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
				if (wasClosed)
					conn.Open();
				reader = await cmd.ExecuteReaderAsync(GetBehavior(wasClosed, CommandBehavior.SingleResult), token);
				if (cacheInfo.Deserializer == null)
				{
					cacheInfo.Deserializer = BuildDeserializer<T>(reader);
				}
				List<T> ret = new List<T>();
				while (await reader.ReadAsync())
				{
					var val = cacheInfo.Deserializer(reader);
					if (val != null)
						ret.Add(GetValue<T>(val)!);
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
							cmd?.Cancel();
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

			CommandBehavior GetBehavior(bool close, CommandBehavior @default)
			{
				return (close ? (@default | CommandBehavior.CloseConnection) : @default);
			}
		}

		private static Task<T?> InternalSingleAsync<T>(IDbConnection conn, CommandDefinition command, CancellationToken token)
		{
			return InternalReaderAsync(conn, command, (reader, cacheInfo) =>
			{
				while (reader.Read())
				{
					var val = cacheInfo.Deserializer?.Invoke(reader);
					return Task.FromResult(GetValue<T>(val));
				}
				return Task.FromResult<T?>(default);
			}, token, true);
		}

		private static Task<DataTable> InternalExecuteTableAsync(IDbConnection conn, CommandDefinition command, CancellationToken token)
		{
			return InternalReaderAsync(conn, command, (reader, cacheInfo) =>
			{
				DataTable dt = new DataTable();
				dt.Load(reader);
				return Task.FromResult(dt);
			}, token);
		}

		private static async Task<T> InternalReaderAsync<T>(IDbConnection conn, CommandDefinition command, Func<IDataReader, CacheInfo, Task<T>> func, CancellationToken token, bool singleRow = false)
		{
			// 缓存
			var parameter = command.Parameters;
			Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
			CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
			// 读取
			var wasClosed = conn.State == ConnectionState.Closed;
			DbCommand? cmd = null;
			IDataReader? reader = null;
			try
			{
				if (wasClosed)
					conn.Open();
				cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
				// singleRow 加上 CommandBehavior.SingleRow
				reader = await cmd.ExecuteReaderAsync(singleRow ? CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.SingleRow : CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, token);
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
							cmd?.Cancel();
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

		private static Task<object> InternalExecuteScalarAsync(IDbConnection conn, CommandDefinition command, CancellationToken token)
		{
			// 缓存
			var parameter = command.Parameters;
			Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(object), parameter?.GetType());
			CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
			// 读取
			var wasClosed = conn.State == ConnectionState.Closed;
			DbCommand? cmd = null;
			try
			{
				if (wasClosed)
					conn.Open();
				cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
				return cmd.ExecuteScalarAsync(token);
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

		private static Task<int> InternalExecuteNonQueryAsync(IDbConnection conn, CommandDefinition command, CancellationToken token)
		{
			// 缓存
			var parameter = command.Parameters;
			Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(object), parameter?.GetType());
			CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
			// 读取
			var wasClosed = conn.State == ConnectionState.Closed;
			DbCommand? cmd = null;
			try
			{
				if (wasClosed)
					conn.Open();
				cmd = command.SetupCommand(conn, cacheInfo.ParameterReader).TryParse();
				return cmd.ExecuteNonQueryAsync(token);
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
#endif
