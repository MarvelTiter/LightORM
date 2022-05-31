using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Interface
{
    public interface ISql<TPart, T>
    {
        TPart Where(Expression<Func<T, bool>> exp);
        TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
        int Execute();
        string ToSql();
    }
}
