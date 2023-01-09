using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Ado
{
    public interface IAdo
    {
        IAdo SwitchDatabase(string key);
        int Execute(string sql, object? param = null);
        DataTable ExecuteDataTable(string sql, object? param = null);
        IEnumerable<T> Query<T>(string sql, object? param = null);
        IEnumerable<dynamic> Query(string sql, object? param = null);
        T? Single<T>(string sql, object? param = null);
        void Query(string sql, object? param, Action<IDataReader> callback);
        #region Async
        Task<int> ExecuteAsync(string sql, object? param = null);
        Task<DataTable> ExecuteDataTableAsync(string sql, object? param = null);
        Task<IList<T>> QueryAsync<T>(string sql, object? param = null, bool threadPool = false);
        Task<IList<dynamic>> QueryAsync(string sql, object? param = null);
        Task<T?> SingleAsync<T>(string sql, object? param = null);
        Task QueryAsync(string sql, object? param, Func<IDataReader, Task> callback);

        #endregion
    }
}
