#if NET40
#else
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Ado
{
    public partial class AdoImpl
    {
        public Task<int> ExecuteAsync(string sql, object? param = null)
        {
            return CurrentConnection.ExecuteAsync(sql, param);
        }
        public Task<DataTable> ExecuteDataTableAsync(string sql, object? param = null)
        {
            return CurrentConnection.ExecuteTableAsync(sql, param);
        }
        public Task<IList<T>> QueryAsync<T>(string sql, object? param = null, bool threadPool = false)
        {
            if (threadPool)
            {
                return CurrentConnection.ThreadPoolQueryAsync<T>(sql, param);
            }
            else
            {
                return CurrentConnection.QueryAsync<T>(sql, param);
            }
        }
        public async Task<IList<dynamic>> QueryAsync(string sql, object? param = null)
        {
            var result = await CurrentConnection.QueryAsync(sql, param);
            return result.ToList();
        }
        public Task<T?> SingleAsync<T>(string sql, object? param = null)
        {
            return CurrentConnection.QuerySingleAsync<T>(sql, param);
        }
        public Task QueryAsync(string sql, object? param, Func<IDataReader, Task> callback)
        {
            return CurrentConnection.ExecuteReaderAsync(sql, param, callback);
        }
    }
}
#endif
