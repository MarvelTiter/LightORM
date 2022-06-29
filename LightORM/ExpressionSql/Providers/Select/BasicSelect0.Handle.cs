using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class
{
    protected void JoinHandle<TAnother>(TableLinkType tableLinkType, Expression body)
    {
        //var table = context.AddTable(typeof(TAnother), tableLinkType);        
        if (!context.Tables.TryGetValue(typeof(TAnother).Name, out var table))
        {
            throw new InvalidOperationException();
        }
        var join = new SqlFragment();
        table.TableType = tableLinkType;
        context.SetFragment(join);
        ExpressionVisit.Visit(body, SqlConfig.Join, context);
        table.Fragment = join;
    }

    internal void SelectHandle(Expression body)
    {
        select ??= new SqlFragment();
        select.Clear();
        context.SetFragment(select);
        ExpressionVisit.Visit(body, SqlConfig.Select, context);
    }

    internal void GroupByHandle(Expression body)
    {
        groupBy ??= new SqlFragment();
        context.SetFragment(groupBy);
        ExpressionVisit.Visit(body, SqlConfig.Group, context);
    }
    bool isAsc;
    internal void OrderByHandle(Expression body, bool asc)
    {
        isAsc = asc;
        orderBy ??= new SqlFragment();
        context.SetFragment(orderBy);
        ExpressionVisit.Visit(body, SqlConfig.Order, context);
    }
}
