using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.ExpSql
{
    public interface ISqlCore<T>
    {
        ISqlCore<T> Select(Expression<Func<T, object>> exp);
        ISqlCore<T> Select<T1>(Expression<Func<T, T1, object>> exp);
        ISqlCore<T> Select<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        ISqlCore<T> Select<T1, T2, T3>(Expression<Func<T, T1, T2, T3, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4>(Expression<Func<T, T1, T2, T3, T4, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4, T5>(Expression<Func<T, T1, T2, T3, T4, T5, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4, T5, T6>(Expression<Func<T, T1, T2, T3, T4, T5, T6, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, object>> exp);
        ISqlCore<T> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp);
        ISqlCore<T> Distinct();
        ISqlCore<T> Update(Expression<Func<object>> exp, Expression<Func<T, object>> pkExp = null);
        ISqlCore<T> Insert(Expression<Func<object>> exp);
    }
}
