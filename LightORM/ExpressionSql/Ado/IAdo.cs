using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
#if NET40
#else
using System.Threading.Tasks;
#endif
namespace LightORM.ExpressionSql.Ado;

public interface IAdo
{
    //IAdo SwitchDatabase(string key);
    int Execute(string sql, object? param = null);
    DataTable ExecuteDataTable(string sql, object? param = null);
    IEnumerable<T> Query<T>(string sql, object? param = null);
    IEnumerable<dynamic> Query(string sql, object? param = null);
    T? Single<T>(string sql, object? param = null);
    void Query(string sql, object? param, Action<IDataReader> callback);
    #region Async
#if NET40
#else
    IAdo AttachCancellationToken(CancellationToken token);
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<DataTable> ExecuteDataTableAsync(string sql, object? param = null);
    Task<IList<T>> QueryAsync<T>(string sql, object? param = null, bool threadPool = false);
    Task<IList<dynamic>> QueryAsync(string sql, object? param = null);
    Task<T?> SingleAsync<T>(string sql, object? param = null);
    Task QueryAsync(string sql, object? param, Func<IDataReader, Task> callback);
#endif
    #endregion
}
