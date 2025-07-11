using System.Threading.Tasks;
using LightORM.Interfaces.ExpSql;

namespace LightORM;

public interface IExpressionContext : IDbAction
{
    string Id { get; }

    /// <summary>
    /// 与<see cref="IExpSelect{T1}.Union(IExpSelect{T1})"/>不同的是，当Union个数大于1时，该方法会嵌套为子查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selects"></param>
    /// <returns></returns>
    IExpSelect<T> Union<T>(params IExpSelect<T>[] selects);
    /// <summary>
    /// 与<see cref="IExpSelect{T1}.UnionAll(IExpSelect{T1})"/>不同的是，当Union个数大于1时，该方法会嵌套为子查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selects"></param>
    /// <returns></returns>
    IExpSelect<T> UnionAll<T>(params IExpSelect<T>[] selects);
    IExpSelect<T> FromQuery<T>(IExpSelect<T> select);
    IExpSelect<T> FromTemp<T>(IExpTemp<T> temp);
    IExpSelect<T> Select<T>();
    //IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(params T[] entities);
    IExpUpdate<T> Update<T>();
    //IExpUpdate<T> Update<T>(T entity);
    IExpUpdate<T> Update<T>(params T[] entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(bool force, bool truncate = false);
    //IExpDelete<T> Delete<T>(T entity);
    IExpDelete<T> Delete<T>(params T[] entities);
    ISqlExecutor Ado { get; }
}

public interface IDbAction
{

    //void BeginTranAll();
    //Task BeginTranAllAsync();
    //void CommitTranAll();
    //Task CommitTranAllAsync();
    //void RollbackTranAll();
    //Task RollbackTranAllAsync();

    //void BeginTran(string key = ConstString.Main);
    //Task BeginTranAsync(string key = ConstString.Main);
    //void CommitTran(string key = ConstString.Main);
    //Task CommitTranAsync(string key = ConstString.Main);
    //void RollbackTran(string key = ConstString.Main);
    //Task RollbackTranAsync(string key = ConstString.Main);
    IExpressionContext Use(IDatabaseProvider db);
    IExpressionContext SwitchDatabase(string key);
    /// <summary>
    /// 创建指定数据库的单元操作对象，支持事务
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ISingleScopedExpressionContext CreateScoped(string key);
    ///// <summary>
    ///// 创建指定数据库的单元操作对象，开启事务
    ///// </summary>
    ///// <param name="key"></param>
    ///// <returns></returns>
    //Task<ISingleScopedExpressionContext> CreateScopedAsync(string key = ConstString.Main);
    /// <summary>
    /// 创建单元操作对象，支持事务
    /// </summary>
    /// <returns></returns>
    IScopedExpressionContext CreateScoped();
    string? CreateTableSql<T>();
    Task<bool> CreateTableAsync<T>();

}

/// <summary>
/// UnitOfWork, 该对象的所有操作，支持事务
/// </summary>
public interface IScopedExpressionContext : IDisposable// IExpressionSql<IScopedExpressionContext>
{
    IScopedExpressionContext SwitchDatabase(string key);
    string Id { get; }
    ISqlExecutor Ado { get; }
    IExpSelect<T> Select<T>();
    //IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(params T[] entities);
    IExpUpdate<T> Update<T>();
    //IExpUpdate<T> Update<T>(T entity);
    IExpUpdate<T> Update<T>(params T[] entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(bool force, bool truncate = false);
    IExpDelete<T> Delete<T>(params T[] entity);
    void BeginTransaction(string key = ConstString.Main, IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    Task BeginTransactionAsync(string key = ConstString.Main, IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    void CommitTransaction(string key = ConstString.Main);
    Task CommitTransactionAsync(string key = ConstString.Main);
    void RollbackTransaction(string key = ConstString.Main);
    Task RollbackTransactionAsync(string key = ConstString.Main);
    void BeginAllTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    Task BeginAllTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    void CommitAllTransaction();
    Task CommitAllTransactionAsync();
    void RollbackAllTransaction();
    Task RollbackAllTransactionAsync();
}
/// <summary>
/// 只能对单个数据库操作
/// </summary>
public interface ISingleScopedExpressionContext : IDisposable
{
    string Id { get; }
    ISqlExecutor Ado { get; }
    IExpSelect<T> Select<T>();
    //IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(params T[] entities);
    IExpUpdate<T> Update<T>();
    //IExpUpdate<T> Update<T>(T entity);
    IExpUpdate<T> Update<T>(params T[] entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(bool force, bool truncate = false);
    IExpDelete<T> Delete<T>(params T[] entity);
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    void CommitTransaction();
    Task CommitTransactionAsync();
    void RollbackTransaction();
    Task RollbackTransactionAsync();
}
