#if NET40
#else
using LightORM.ExpressionSql;
using LightORM.ExpressionSql.Ado;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Ado
{
    public partial class AdoImpl
    {
        CancellationToken CancelToken = CancellationToken.None;
        public IAdo AttachCancellationToken(CancellationToken token)
        {
            CancelToken = token;
            return this;
        }
        async Task<T> InternalExecuteAsync<T>(string sql, object? param, Func<Task<T>> executor)
        {
            SqlArgs args = new SqlArgs() { Action = SqlAction.ExecuteSql, Sql = sql, SqlParameter = param };
            life.BeforeExecute?.Invoke(args);
            var result = await executor();
            life.AfterExecute?.Invoke(args);
            return result;
        }
        public Task<int> ExecuteAsync(string sql, object? param = null)
        {
            return InternalExecuteAsync(sql, param, () =>
            {
                return CurrentConnection.ExecuteAsync(sql, CancelToken, param);
            });
        }
        public Task<DataTable> ExecuteDataTableAsync(string sql, object? param = null)
        {
            return InternalExecuteAsync(sql, param, () =>
            {
                return CurrentConnection.ExecuteTableAsync(sql, CancelToken, param);
            });
        }
        public async Task<IList<T>> QueryAsync<T>(string sql, object? param = null, bool threadPool = false)
        {
            var ret = await InternalExecuteAsync(sql, param, () =>
               {
                   if (threadPool)
                   {
                       return CurrentConnection.ThreadPoolQueryAsync<T>(sql, param);
                   }
                   else
                   {
                       return CurrentConnection.QueryAsync<T>(sql, CancelToken, param);
                   }
               });
            return ret;
        }
        public async Task<IList<dynamic>> QueryAsync(string sql, object? param = null)
        {
            var ret = await InternalExecuteAsync(sql, param, async () =>
              {
                  var result = await CurrentConnection.QueryAsync(sql, CancelToken, param);
                  return result.ToList();
              });
            return ret;
        }
        public Task<T?> SingleAsync<T>(string sql, object? param = null)
        {
            return InternalExecuteAsync(sql, param, () =>
            {
                return CurrentConnection.QuerySingleAsync<T>(sql, CancelToken, param);
            });
        }
        public async Task QueryAsync(string sql, object? param, Func<IDataReader, Task> callback)
        {
            await InternalExecuteAsync<int>(sql, param, async () =>
            {
                await CurrentConnection.ExecuteReaderAsync(sql, CancelToken, param, callback);
                return 0;
            });
        }
    }
}
#endif
