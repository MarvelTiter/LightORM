using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpInsert<T> : ISql<IExpInsert<T>, T>
    {
        IExpInsert<T> AppendData(T item);
        IExpInsert<T> AppendData(IEnumerable<T> items);
        IExpInsert<T> SetColumns(Expression<Func<T, object>> columns);
        IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns);
    }
}
