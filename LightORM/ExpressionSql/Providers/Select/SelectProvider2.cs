using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class SelectProvider2<T1, T2> : BasicSelect0<IExpSelect<T1, T2>, T1>, IExpSelect<T1, T2>
{
    public SelectProvider2(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
    }

    public IExpSelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2> GroupBy(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2> InnerJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> LeftJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> RightJoin<TAnother>(Expression<Func<TypeSet<TAnother, T1, T2>, bool>> exp)
    {
        throw new NotImplementedException();
    }

    public IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }


    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQuery<TReturn>(args);
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQuery<TReturn>(args);
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQueryAsync<TReturn>(args);
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQueryAsync<TReturn>(args);
    }

    public IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}
