using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class SelectProvider1<T1> : BasicSelect0<IExpSelect<T1>, T1>, IExpSelect<T1>
{
    public SelectProvider1(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(key, getContext, connectInfos, life) { }

    public IExpSelect<T1> GroupBy(Expression<Func<T1, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1> Where(Expression<Func<T1, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}
