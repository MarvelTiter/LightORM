using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
    {
        IExpUpdate<T> AppendData(T item);
        IExpInsert<T> UpdateColumns(Expression<Func<T, object>> columns);
        IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns);
        IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp);
        IExpUpdate<T> SetIf<TField>(bool confition, Expression<Func<T, TField>> exp);
        IExpUpdate<T> Where(T item);
        IExpUpdate<T> Where(IEnumerable<T> items);
    }
}
