using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.Providers;

internal abstract class BasicProvider<T1>
{
    protected readonly SqlContext context;
    //private readonly ITableContext tableContext;

    //protected readonly List<TableInfo> tables;
    protected readonly DbConnectInfo dbConnect;
    protected readonly Dictionary<string, SqlFieldInfo> SessionFields = new Dictionary<string, SqlFieldInfo>();
    protected SqlFragment? where;

    public string? DbKey { get; set; }
    public SqlExecuteLife Life { get; }

    public BasicProvider(ITableContext tableContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    {
        //var tbContext = getContext.Invoke(key);

        //this.tableContext = tableContext;
        dbConnect = connectInfos;
        Life = life;
        context = new SqlContext(tableContext);
        //tables = new List<TableInfo>();
        context.AddTable(typeof(T1));
    }

    protected abstract SqlConfig WhereConfig { get; }
    protected virtual void WhereHandle(Expression body)
    {
        where ??= new SqlFragment();
        if (where.Length > 0) where.Append("\nAND ");
        where.Append("(");
        if (body?.ToString() == "True")
        {
            where.Append(" 1 = 1 ");
        }
        else
        {
            context.SetFragment(where);
            ExpressionVisit.Visit(body, WhereConfig, context);
        }
        where.Append(")");
    }
}
