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
    IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(IEnumerable<T> entities);
    IExpUpdate<T> Update<T>();
    IExpUpdate<T> Update<T>(T entity);
    IExpUpdate<T> Update<T>(IEnumerable<T> entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(T entity);
    //IExpDelete<T> Delete<T>(IEnumerable<T> entities);
    ISqlExecutor Ado { get; }
    
}

public interface IDbAction
{
    IExpressionContext Use(IDatabaseProvider db);
    IExpressionContext SwitchDatabase(string key);
    void BeginTranAll();
    //Task BeginTranAllAsync();
    void CommitTranAll();
    Task CommitTranAllAsync();
    void RollbackTranAll();
    Task RollbackTranAllAsync();

    void BeginTran(string key = ConstString.Main);
    //Task BeginTranAsync(string key = ConstString.Main);
    IScopedExpressionContext BeginScopedTran(string key = ConstString.Main);
    //Task<IScopedExpressionContext> BeginScopedTranAsync(string key = ConstString.Main);
    void CommitTran(string key = ConstString.Main);
    Task CommitTranAsync(string key = ConstString.Main);
    void RollbackTran(string key = ConstString.Main);
    Task RollbackTranAsync(string key = ConstString.Main);

}

public interface IScopedExpressionContext
{
    string Id { get; }

    IExpSelect<T> Select<T>();
    IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(IEnumerable<T> entities);
    IExpUpdate<T> Update<T>();
    IExpUpdate<T> Update<T>(T entity);
    IExpUpdate<T> Update<T>(IEnumerable<T> entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(T entity);
    //IExpDelete<T> Delete<T>(IEnumerable<T> entities);
    ISqlExecutor Ado { get; }

    void CommitTran();
    Task CommitTranAsync();
    void RollbackTran();
    Task RollbackTranAsync();
}
