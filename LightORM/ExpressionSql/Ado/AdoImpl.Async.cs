#if NET40
#else
using LightORM.ExpressionSql;
using LightORM.ExpressionSql.Ado;
using LightORM.Interfaces;
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
            throw new NotImplementedException();
        }
        public Task<DataTable> ExecuteDataTableAsync(string sql, object? param = null)
        {
            throw new NotImplementedException();

        }
        public Task<IList<T>> QueryAsync<T>(string sql, object? param = null, bool threadPool = false)
        {
            throw new NotImplementedException();

        }
        public Task<IList<dynamic>> QueryAsync(string sql, object? param = null)
        {
            throw new NotImplementedException();

        }
        public Task<T?> SingleAsync<T>(string sql, object? param = null)
        {
            throw new NotImplementedException();

        }
        public Task QueryAsync(string sql, object? param, Func<IDataReader, Task> callback)
        {
            throw new NotImplementedException();

        }
    }
}
#endif
