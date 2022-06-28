using MDbContext;
using MDbContext.ExpressionSql.ExpressionVisitor;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class
{
    protected SqlFragment? select;
    protected SqlFragment? groupBy;
    protected SqlFragment? orderBy;
    protected override SqlConfig WhereConfig => SqlConfig.Where;

    public BasicSelect0(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(key, getContext, connectInfos, life) { }
    public TSelect Count(out long total)
    {
        var sql = BuildCountSql();
        total = InternalExecute<long>(sql, context.GetParameters());
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.InnerJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.LeftJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.RightJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body);
        return (this as TSelect)!;
    }

    int index = 0;
    int size = 0;
    public TSelect Paging(int pageIndex, int pageSize)
    {
        index = pageIndex;
        size = pageSize;
        return (this as TSelect)!;
    }

    public TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp)
    {
        WhereHandle(exp.Body);
        return (this as TSelect)!;
    }

    public TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp)
    {
        if (condition) WhereHandle(exp.Body);
        return (this as TSelect)!;
    }

    public TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp)
    {
        if (condition) WhereHandle(exp.Body);
        return (this as TSelect)!;
    }

    public TMember Max<TMember>(Expression<Func<T1, TMember>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("MAX(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        return InternalExecute<TMember>();
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("SUM(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        return InternalExecute<double>();
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("COUNT(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        return InternalExecute<int>();
    }
    bool rollup = false;
    public TSelect RollUp()
    {
        rollup = true;
        return (this as TSelect)!;
    }

    private string BuildCountSql()
    {
        var tables = context.Tables.Values.ToArray();
        var main = tables[0];
        StringBuilder sql = new StringBuilder();
        sql.Append($"SELECT COUNT(1) FROM {main.TableName} {main.Alias}");
        for (int i = 1; i < context.Tables.Count; i++)
        {
            var temp = tables[i];
            if (temp.TableType == TableLinkType.None) continue;
            sql.Append($"\n{temp.TableType.ToLabel()} {temp.TableName} {temp.Alias} ON {temp.Fragment}");
        }
        if (where != null)
            sql.Append($"\nWHERE {where}");
        Life.BeforeExecute?.Invoke(sql.ToString());
        return sql.ToString();
    }

    public string ToSql()
    {
        var tables = context.Tables.Values.ToArray();
        var main = tables[0];
        StringBuilder sql = new StringBuilder();
        select!.Remove(select.Length - 1, 1);
        sql.Append($"SELECT {select} FROM {main.TableName} {main.Alias}");
        for (int i = 1; i < context.Tables.Count; i++)
        {
            var temp = tables[i];
            if (temp.TableType == TableLinkType.None) continue;
            sql.Append($"\n{temp.TableType.ToLabel()} {temp.TableName} {temp.Alias} ON {temp.Fragment}");
        }
        if (where != null)
            sql.Append($"\nWHERE {where}");

        if (groupBy != null)
            sql.Append($"\nGROUP BY {groupBy}");

        if (orderBy != null)
            sql.Append($"\nORDER BY {orderBy} {(isAsc ? "ASC" : "DESC")}");

        if (index * size > 0)
        {
            //分页处理
            context.DbHandler.DbPaging(context, select, sql, index, size);
        }

        Life.BeforeExecute?.Invoke(sql.ToString());
        return sql.ToString();
    }

}
