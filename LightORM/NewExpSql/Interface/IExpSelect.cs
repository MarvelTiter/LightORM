using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpSelect<T> : ISql<IExpSelect<T>, T>
    {
        IExpSelect<T> ToList(Expression<Func<T, bool>> exp);
        IExpSelect<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp);
        IExpSelect<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp);
        IExpSelect<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp);
        //IExpSelect<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        //IExpSelect<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        //IExpSelect<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> RightJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        IExpSelect<T> Where<T1>(Expression<Func<T1, bool>> exp);
        IExpSelect<T> Where<T1>(Expression<Func<T, T1, bool>> exp);
        //IExpSelect<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> exp);
        //IExpSelect<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> exp);
        //IExpSelect<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, bool>> exp);
        //IExpSelect<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> exp);
        IExpSelect<T> Count(out long total);
    }
}
