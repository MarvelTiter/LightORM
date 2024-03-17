using System;
using System.Linq.Expressions;
#if NET40
#else
using System.Threading;
using System.Threading.Tasks;
using LightORM;
using LightORM.ExpressionSql;
using LightORM.ExpressionSql.Interface;
using LightORM.Interfaces;
#endif
namespace LightORM.Interfaces;

public interface ISql
{
    string ToSql();
}
public interface ISql<TPart, T> : ISql
{
    TPart Where(Expression<Func<T, bool>> exp);
    TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
    int Execute();
    Task<int> ExecuteAsync();
}
