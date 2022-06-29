using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>, ITransactionable
    {
        IExpUpdate<T> AppendData(T item);
        IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns);
        IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns);
        IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp);
        IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp);
        IExpUpdate<T> Where(T item);
        IExpUpdate<T> Where(IEnumerable<T> items);
    }
}
