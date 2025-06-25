using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM;

public interface ISqlExecutor : IDisposable, ICloneable
{
    /// <summary>
    /// 数据库日志
    /// </summary>
    public Action<string, object?>? DbLog { get; set; }

    internal IDatabaseProvider Database { get; }
    ///// <summary>
    ///// 数据库事务
    ///// </summary>
    public DbTransaction? DbTransaction { get; set; }

    //public DbConnection GetConnection();

    /// <summary>
    /// 开启事务
    /// </summary>
    void BeginTran();

    /// <summary>
    /// 提交事务
    /// </summary>
    void CommitTran();

    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <returns></returns>
    void RollbackTran();

    /// <summary>
    /// 开启事务异步
    /// </summary>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task BeginTranAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交事务异步
    /// </summary>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task CommitTranAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 回滚事务异步
    /// </summary>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task RollbackTranAsync(CancellationToken cancellationToken = default);

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
    /// <returns></returns>
    DbDataReader ExecuteReader(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text);

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
    Task<int> ExecuteNonQueryAsync(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

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
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<DbDataReader> ExecuteReaderAsync(string commandText, object? dbParameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

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