﻿using MDbContext;
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

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    protected SqlFragment? select;
    protected SqlFragment? groupBy;
    protected SqlFragment? orderBy;
    private readonly Expression selectBody;
    protected override SqlConfig WhereConfig => SqlConfig.Where;

    public BasicSelect0(Expression body, ITableContext getContext, DbConnectInfo connectInfos, SqlExecuteLife life)
  : base(getContext, connectInfos, life)
    {
        this.selectBody = body;
    }
    public TSelect Count(out long total)
    {
        var sql = BuildCountSql();
        var args = BuildArgs(sql);
        total = InternalSingle<long>(args);
        return (this as TSelect)!;
    }

    #region jion
    public TSelect InnerJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.InnerJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.LeftJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        JoinHandle<TAnother>(TableLinkType.RightJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body, false);
        return (this as TSelect)!;
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body, false);
        return (this as TSelect)!;
    }
    #endregion

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
        SqlArgs args = BuildArgs();
        return InternalSingle<TMember>(args);
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("SUM(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        SqlArgs args = BuildArgs();
        return InternalSingle<double>(args);
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("COUNT(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        SqlArgs args = BuildArgs();
        return InternalSingle<int>(args);
    }

    

    public Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("MAX(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        SqlArgs args = BuildArgs();
        return InternalSingleAsync<TMember>(args);
    }

    public Task<double> SumAsync(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("SUM(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        SqlArgs args = BuildArgs();
        return InternalSingleAsync<double>(args);
    }

    public Task<int> CountAsync(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("COUNT(");
        ExpressionVisit.Visit(exp.Body, SqlConfig.SelectFunc, context);
        // tosql去掉最后一个字符
        select.Append("))");
        SqlArgs args = BuildArgs();
        return InternalSingleAsync<int>(args);
    }


    bool rollup = false;
    public TSelect RollUp()
    {
        rollup = true;
        return (this as TSelect)!;
    }
    bool distanct = false;
    public TSelect Distinct()
    {
        distanct = true;
        return (this as TSelect)!;
    }
    //TSelect? subQuery;
    //public TSelect From(Func<IExpressionContext, TSelect> sub)
    //{
    //    throw new NotImplementedException();
    //    subQuery = sub(Life.Core!);
    //    return (this as TSelect)!;
    //}

    private string BuildCountSql()
    {
        var tables = context.Tables;
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
        return sql.ToString();
    }

    public string ToSql()
    {
        var tables = context.Tables;
        var main = tables[0];
        StringBuilder sql = new StringBuilder();
        select!.Remove(select.Length - 1, 1);
        sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}{select} FROM {main.TableName} {main.Alias}");
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
        return sql.ToString();
    }

    protected SqlArgs BuildArgs(string? sql = null)
    {
        return new SqlArgs()
        {
            Sql = sql ?? ToSql(),
            SqlParameter = context.GetParameters(),
            Action = SqlAction.Select,
        };
    }
}
