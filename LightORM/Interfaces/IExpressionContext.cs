using LightORM.ExpressionSql.Ado;
using LightORM.ExpressionSql.Interface;
using System;
using System.Data;
using System.Linq.Expressions;
#if NET40
#else
using System.Threading.Tasks;
#endif
namespace LightORM.Interfaces;

public interface IExpressionContext : IDbAction
{

    IExpSelect<T> Select<T>();
    IExpSelect<T> Select<T>(Expression<Func<T, object>> exp);
    IExpInsert<T> Insert<T>();
    IExpUpdate<T> Update<T>();
    IExpDelete<T> Delete<T>();
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
}

public static class ExpSqlExtensions
{
    //public static IExpSelect<T> Select<T,T1>(this IExpSql self)
    //{
    //    return new SelectProvider<T>()
    //}
}
