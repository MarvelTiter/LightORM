using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface
{
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
}
