using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.Providers;

internal abstract class BasicProvider<T1>
{
    protected readonly SqlContext context;
    //protected readonly List<TableInfo> tables;
    protected readonly DbConnectInfo dbConnect;
    protected readonly Dictionary<string, SqlFieldInfo> SessionFields = new Dictionary<string, SqlFieldInfo>();
    protected SqlFragment? where;

    public string DbKey { get; }
    public SqlExecuteLife Life { get; }

    public BasicProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
    {
        var tbContext = getContext.Invoke(key);
        DbKey = key;
        dbConnect = connectInfos;
        Life = life;
        context = new SqlContext(tbContext);
        //tables = new List<TableInfo>();
        var main = context.AddTable(typeof(T1));
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
