using System;
using System.Linq.Expressions;
#if NET40
#else
using System.Threading;
using System.Threading.Tasks;
#endif
namespace LightORM.ExpressionSql.Interface;

public interface ISql
{
    string ToSql();
}
public interface ISql<TPart, T> : ISql
{
    TPart Where(Expression<Func<T, bool>> exp);
    TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
    int Execute();
#if NET40
#else
    Task<int> ExecuteAsync();
    TPart AttachCancellationToken(CancellationToken token);
#endif
}
