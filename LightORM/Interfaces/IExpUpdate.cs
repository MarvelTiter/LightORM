using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LightORM.Interfaces;

namespace LightORM.ExpressionSql.Interface;

public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
{
    IExpUpdate<T> AppendData(T item);
    IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns);
    IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns);
    IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, object value);
    IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, object value);
    IExpUpdate<T> Where(T item);
    IExpUpdate<T> Where(IEnumerable<T> items);
}
