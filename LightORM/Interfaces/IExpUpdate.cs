using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LightORM.Interfaces;

namespace LightORM;

public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
{
    IExpUpdate<T> UpdateColumns(Expression<Func<T, object>> columns);
    IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns);
    IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns);
    IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetNull<TField>(Expression<Func<T, TField>> exp);
    IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, TField value);
    IExpUpdate<T> SetNullIf<TField>(bool condition, Expression<Func<T, TField>> exp);
}
