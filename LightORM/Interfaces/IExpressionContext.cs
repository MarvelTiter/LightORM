using System.Threading.Tasks;

namespace LightORM;

public interface IExpressionContext : IDbAction
{

    IExpSelect<T> Select<T>();
    IExpSelect Select(string tableName);
    IExpSelect<T> Select<T>(Expression<Func<T, object>> exp);
    //IExpInsert<T> Insert<T>();
    IExpInsert<T> Insert<T>(T entity);
    IExpInsert<T> Insert<T>(IEnumerable<T> entities);
    IExpUpdate<T> Update<T>();
    IExpUpdate<T> Update<T>(T entity);
    //IExpUpdate<T> Update<T>(IEnumerable<T> entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(T entity);
    //IExpDelete<T> Delete<T>(IEnumerable<T> entities);
    ISqlExecutor Ado { get; }
}


public interface IDbAction
{
    IExpressionContext SwitchDatabase(string key);
    void BeginTran();
    Task BeginTranAsync();
    void CommitTran();
    Task CommitTranAsync();
    void RollbackTran();
    Task RollbackTranAsync();

    void BeginTran(string key);
    Task BeginTranAsync(string key);
    void CommitTran(string key);
    Task CommitTranAsync(string key);
    void RollbackTran(string key);
    Task RollbackTranAsync(string key);

}

public static class ExpSqlExtensions
{
    //public static IExpSelect<T> Select<T,T1>(this IExpSql self)
    //{
    //    return new SelectProvider<T>()
    //}
}
