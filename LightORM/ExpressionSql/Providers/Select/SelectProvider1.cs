using LightORM.ExpressionSql.Interface;
using LightORM.ExpressionSql.Interface.Select;
using LightORM.ExpressionSql.Providers.Select;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Providers.Select;

internal partial class SelectProvider1<T1> : BasicSelect0<IExpSelect<T1>, T1>, IExpSelect<T1>
{
    public SelectProvider1(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(body, getContext, connectInfos, life) { }

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
        var args = BuildArgs();
        return InternalQuery<TReturn>(args);
    }
#if !NET40
    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQueryAsync<TReturn>(args);
    }
#endif

}
