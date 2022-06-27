using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface
{
    public interface ISql<TPart, T>
    {
        TPart Where(Expression<Func<T, bool>> exp);
        TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
        int Execute();
        Task<int> ExecuteAsync();
        string ToSql();
    }
}
