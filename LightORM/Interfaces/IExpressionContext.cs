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
    IExpUpdate<T> Update<T>(IEnumerable<T> entities);
    IExpDelete<T> Delete<T>();
    IExpDelete<T> Delete<T>(T entity);
    //IExpDelete<T> Delete<T>(IEnumerable<T> entities);
    ISqlExecutor Ado { get; }
}


public interface IDbAction
{
    IExpressionContext Use(DbConnectInfo db);
    IExpressionContext SwitchDatabase(string key);
    void BeginTranAll();
    Task BeginTranAllAsync();
    void CommitTranAll();
    Task CommitTranAllAsync();
    void RollbackTranAll();
    Task RollbackTranAllAsync();

    void BeginTran(string key = ConstString.Main);
    Task BeginTranAsync(string key = ConstString.Main);
    void CommitTran(string key = ConstString.Main);
    Task CommitTranAsync(string key = ConstString.Main);
    void RollbackTran(string key = ConstString.Main);
    Task RollbackTranAsync(string key = ConstString.Main);

}

public static class ExpSqlExtensions
{
    //public static IExpSelect<T> Select<T,T1>(this IExpSql self)
    //{
    //    return new SelectProvider<T>()
    //}
}
