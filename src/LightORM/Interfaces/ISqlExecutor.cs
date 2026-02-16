using System.Data.Common;
using System.Threading;

namespace LightORM;

public partial interface ISqlExecutor
{
    internal ConnectionPool Pool { get; }
    internal AdoInterceptor Interceptor { get; }
    internal IDatabaseProvider Database { get; }
    internal void InitTransactionContext();
    internal void InitTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    /// <summary>
    /// 开启事务异步
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    internal Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default);
}
public partial interface ISqlExecutor : IDisposable, ICloneable
{
    ///// <summary>
    ///// 数据库日志
    ///// </summary>
    //public Action<string, object?>? DbLog { get; set; }
    ///// <summary>
    ///// 数据库事务
    ///// </summary>
    public DbTransaction? DbTransaction { get; }

    //public DbConnection GetConnection();
    internal string Id { get; }
    void UseExternalTransaction(DbTransaction externalTransaction);

    /// <summary>
    /// 开启事务
    /// </summary>
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);

    /// <summary>
    /// 提交事务
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <returns></returns>
    void RollbackTransaction();

    /// <summary>
    /// 提交事务异步
    /// </summary>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 回滚事务异步
    /// </summary>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行非查询
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <returns></returns>
    int ExecuteNonQuery(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// 执行标量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <returns></returns>
    T? ExecuteScalar<T>(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// 执行阅读器
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="behavior">命令行为</param>
    /// <returns></returns>
    DbDataReader ExecuteReader(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null);

    /// <summary>
    /// 多结果查询, 使用<see cref="MultipleResult"/>的<see cref="MultipleResult.Read{T}"/>或者<see cref="MultipleResult.ReadFirst{T}"/>或者对应的异步版本
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="dbParameters"></param>
    /// <param name="commandType"></param>
    /// <param name="behavior"></param>
    /// <returns></returns>
    MultipleResult QueryMultiple(string sql, object? dbParameters = null, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null);
    /// <summary>
    /// 执行数据集
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <returns></returns>
    DataSet ExecuteDataSet(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// 执行数据表格
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <returns></returns>
    DataTable ExecuteDataTable(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text);

    /// <summary>
    /// 执行非查询异步
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行标量异步
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<T?> ExecuteScalarAsync<T>(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行阅读器异步
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="behavior">命令行为</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 多结果查询, 使用<see cref="MultipleResult"/>的<see cref="MultipleResult.Read{T}"/>或者<see cref="MultipleResult.ReadFirst{T}"/>或者对应的异步版本
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="dbParameters"></param>
    /// <param name="commandType"></param>
    /// <param name="behavior"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MultipleResult> QueryMultipleAsync(string commandText, object? dbParameters = null, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行数据集异步
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<DataSet> ExecuteDataSetAsync(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行数据表格异步
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="commandText">命令文本</param>
    /// <param name="dbParameters">数据库参数</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<DataTable> ExecuteDataTableAsync(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
}
