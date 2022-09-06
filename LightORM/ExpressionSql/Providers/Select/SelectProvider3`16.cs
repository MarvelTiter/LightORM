using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.NewExpSql.ExpressionVisitor;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class SelectProvider3<T1, T2, T3> : BasicSelect0<IExpSelect<T1, T2, T3>, T1>, IExpSelect<T1, T2, T3>
{
    public SelectProvider3(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
    }

    public IExpSelect<T1, T2, T3> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3> GroupBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3> OrderBy(Expression<Func<TypeSet<T1, T2, T3>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider4<T1, T2, T3, T4> : BasicSelect0<IExpSelect<T1, T2, T3, T4>, T1>, IExpSelect<T1, T2, T3, T4>
{
    public SelectProvider4(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
    }

    public IExpSelect<T1, T2, T3, T4> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider5<T1, T2, T3, T4, T5> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5>, T1>, IExpSelect<T1, T2, T3, T4, T5>
{
    public SelectProvider5(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
    }

    public IExpSelect<T1, T2, T3, T4, T5> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider6<T1, T2, T3, T4, T5, T6> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6>
{
    public SelectProvider6(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider7<T1, T2, T3, T4, T5, T6, T7> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7>
{
    public SelectProvider7(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8>
{
    public SelectProvider8(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    public SelectProvider9(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    public SelectProvider10(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    public SelectProvider11(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    public SelectProvider12(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
        context.AddTable(typeof(T12));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    public SelectProvider13(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
        context.AddTable(typeof(T12));
        context.AddTable(typeof(T13));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    public SelectProvider14(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
        context.AddTable(typeof(T12));
        context.AddTable(typeof(T13));
        context.AddTable(typeof(T14));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    public SelectProvider15(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
        context.AddTable(typeof(T12));
        context.AddTable(typeof(T13));
        context.AddTable(typeof(T14));
        context.AddTable(typeof(T15));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}

internal partial class SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : BasicSelect0<IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T1>, IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    public SelectProvider16(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    : base(body, getContext, connectInfos, life)
    {
        context.AddTable(typeof(T2));
        context.AddTable(typeof(T3));
        context.AddTable(typeof(T4));
        context.AddTable(typeof(T5));
        context.AddTable(typeof(T6));
        context.AddTable(typeof(T7));
        context.AddTable(typeof(T8));
        context.AddTable(typeof(T9));
        context.AddTable(typeof(T10));
        context.AddTable(typeof(T11));
        context.AddTable(typeof(T12));
        context.AddTable(typeof(T13));
        context.AddTable(typeof(T14));
        context.AddTable(typeof(T15));
        context.AddTable(typeof(T16));
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.InnerJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.LeftJoin, exp.Body);
        return this;
    }
    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
    {
        JoinHandle<TJoin>(TableLinkType.RightJoin, exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp)
    {
        GroupByHandle(exp.Body);
        return this;
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp, bool asc = true)
    {
        OrderByHandle(exp.Body, asc);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQuery<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp)
    {
        SelectHandle(exp.Body);
        return InternalQueryAsync<TReturn>();
    }

    public IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, bool>> exp)
    {
        WhereHandle(exp.Body);
        return this;
    }
}