﻿using LightORM.ExpressionSql.ExpressionVisitor;
using MDbContext;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightORM.Interfaces;

namespace LightORM.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    protected SqlFragment? select;
    protected SqlFragment? groupBy;
    protected SqlFragment? orderBy;
    private readonly Expression selectBody;
    protected override SqlResolveOptions WhereConfig => SqlResolveOptions.Where;
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

    public bool Any()
    {
        return Count() > 0;
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
    public TSelect Where(Expression<Func<T1, bool>> exp)
    {
        WhereHandle(exp.Body);
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

    public TSelect GroupBy<Another>(Expression<Func<Another, object>> exp)
    {
        GroupByHandle(exp.Body);
        return (this as TSelect)!;
    }

    public TMember Max<TMember>(Expression<Func<T1, TMember>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("MAX(");
        ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
        // tosql去掉最后2个字符
        select.Append(")))");
        SqlArgs args = BuildArgs();
        return InternalSingle<TMember>(args);
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("SUM(");
        ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
        // tosql去掉最后2个字符
        select.Append(")))");
        SqlArgs args = BuildArgs();
        return InternalSingle<double>(args);
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("COUNT(");
        ExpressionVisit.Visit(exp.Body, SqlResolveOptions.SelectFunc, context);
        // tosql去掉最后2个字符
        select.Append(")))");
        SqlArgs args = BuildArgs();
        return InternalSingle<int>(args);
    }

    public int Count()
    {
        select ??= new SqlFragment();
        context.SetFragment(select);
        select.Append("COUNT(*");
        // tosql去掉最后2个字符
        select.Append(")))");
        SqlArgs args = BuildArgs();
        return InternalSingle<int>(args);
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
   
    private string BuildCountSql()
    {
        var tables = context.Tables;
        var main = tables[0];
        StringBuilder sql = new StringBuilder();
        sql.Append($"SELECT COUNT(1) FROM {context.DbHandler.DbEmphasis(main.TableName!)} {main.Alias}");
        for (int i = 1; i < context.Tables.Count; i++)
        {
            var temp = tables[i];
            if (temp.TableType == TableLinkType.None) continue;
            sql.Append($"\n{temp.TableType.ToLabel()} {context.DbHandler.DbEmphasis(temp.TableName!)} {temp.Alias} ON {temp.Fragment}");
        }
        if (where != null)
            sql.Append($"\nWHERE {where}");
        return sql.ToString();
    }

    public string ToSql()
    {
        return context.DbHandler.BuildSelectSql(context, select, distanct, where, groupBy, orderBy, isAsc, index, size);
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

    public TSelect As(string tableName)
    {
        throw new NotImplementedException();
    }

    public TSelect As(Type type)
    {
        throw new NotImplementedException();
    }

    public TSelect As<TOther>()
    {
        throw new NotImplementedException();
    }

    public TSelect Where(string whereString)
    {
        throw new NotImplementedException();
    }

    public TSelect GroupByIf<Another>(bool ifGroupby, Expression<Func<Another, bool>> exp)
    {
        throw new NotImplementedException();
    }
}
