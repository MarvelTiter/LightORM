using LightORM.ExpressionSql;
using LightORM.ExpressionSql.ExpressionVisitor;
using LightORM.ExpressionSql.Interface.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightORM.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    protected void JoinHandle<TAnother>(TableLinkType tableLinkType, Expression body, bool checkTable = true)
    {
        //var table = context.AddTable(typeof(TAnother), tableLinkType);
        var table = context.Tables.FirstOrDefault(t => t.Compare(typeof(TAnother)));
        if (table is null)
        {
            if (checkTable)
                throw new InvalidOperationException();
            else
            {
                table = context.AddTable(typeof(TAnother));
            }
        }
        var join = new SqlFragment();
        table!.TableType = tableLinkType;
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
