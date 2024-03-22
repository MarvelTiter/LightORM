using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightORM.Interfaces;
using LightORM.Cache;
using LightORM.ExpressionSql;
using System.Data;
using System.Threading.Tasks;
using LightORM.Builder;

namespace LightORM.Providers.Select;

internal class SelectProvider0<TSelect, T1> : IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    protected SelectBuilder SqlBuilder { get; }
    protected ISqlExecutor Executor { get; }
    protected DbBaseType DbType => Executor.ConnectInfo.DbBaseType;
    protected ExpressionInfo? SelectExpression;
    public SelectProvider0(ISqlExecutor executor)
    {
        Executor = executor;
        SqlBuilder = new SelectBuilder();
        SqlBuilder.DbType = DbType;
        SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T1>();
    }



    public bool Any()
    {
        return Count() > 0;
    }

    protected TSelect OrderByHandle(Expression exp, bool asc)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Order,
            AdditionalParameter = asc ? "ASC" : "DESC"
        });
        return (this as TSelect)!;
    }

    #region jion

    protected TSelect JoinHandle<TJoin>(Expression exp, TableLinkType joinType)
    {
        var expression = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = exp,
            DbParameterIndex = SqlBuilder.DbParameters.Count + 1
        };

        SqlBuilder.Expressions.Add(expression);

        SqlBuilder.Joins.Add(new JoinInfo()
        {
            ExpressionId = expression.Id,
            JoinType = joinType,
            EntityInfo = Cache.TableContext.GetTableInfo<TJoin>()
        });
        return (this as TSelect)!;
    }

    public TSelect InnerJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, TableLinkType.InnerJoin);
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.InnerJoin);
    }

    public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.InnerJoin);
    }

    public TSelect LeftJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, TableLinkType.LeftJoin);
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.LeftJoin);
    }

    public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.LeftJoin);
    }

    public TSelect RightJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    {
        return JoinHandle<TAnother>(exp, TableLinkType.RightJoin);
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.RightJoin);
    }

    public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    {
        return JoinHandle<TAnother1>(exp, TableLinkType.RightJoin);
    }

    #endregion

    #region where

    protected TSelect WhereHandle(Expression exp)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Where,
            Expression = exp,
        });
        return (this as TSelect)!;
    }

    public TSelect Where(Expression<Func<T1, bool>> exp)
    {
        return WhereHandle(exp);
    }
    public TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp)
    {
        return WhereHandle(exp);
    }

    public TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp)
    {
        if (condition)
        {
            return WhereHandle(exp);
        }
        return (this as TSelect)!;
    }

    public TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp)
    {
        if (condition) return WhereHandle(exp);
        return (this as TSelect)!;
    }

    #endregion

    public TSelect Paging(int pageIndex, int pageSize)
    {
        SqlBuilder.PageIndex = pageIndex;
        SqlBuilder.PageSize = pageSize;
        return (this as TSelect)!;
    }


    #region group

    protected TSelect GroupByHandle(Expression exp)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            ResolveOptions = SqlResolveOptions.Group,
            Expression = exp
        });
        return (this as TSelect)!;
    }

    public TSelect GroupBy<Another>(Expression<Func<Another, object>> exp)
    {
        return GroupByHandle(exp.Body);
    }

    public TSelect GroupByIf<Another>(bool ifGroupby, Expression<Func<Another, bool>> exp)
    {
        if (ifGroupby) return GroupByHandle(exp.Body);
        return (this as TSelect)!;
    }

    #endregion

    public TMember Max<TMember>(Expression<Func<T1, TMember>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "MAX({0})";
        });
        return ToList<TMember>().First();
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "SUM({0})";
        });
        return ToList<double>().First();
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "COUNT({0})";
        });
        return ToList<int>().First();
    }

    public int Count()
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = null;
            e.Template = "COUNT(*)";
        });
        return ToList<int>().First();
    }

    public TSelect Count(out long total)
    {
        total = Count();
        SqlBuilder.Expressions.Update(SelectExpression?.Id, SelectExpression);
        return (this as TSelect)!;
    }

    public TSelect RollUp()
    {
        SqlBuilder.IsRollup = true;
        return (this as TSelect)!;
    }

    public TSelect Distinct()
    {
        SqlBuilder.IsDistinct = true;
        return (this as TSelect)!;
    }

    public TSelect As(string tableName)
    {
        SqlBuilder.TableInfo.CustomName = tableName;
        return (this as TSelect)!;
    }

    public TSelect As(Type type)
    {
        var info = Cache.TableContext.GetTableInfo(type);
        SqlBuilder.TableInfo.CustomName = info.TableName;
        return (this as TSelect)!;
    }

    public TSelect As<TOther>()
    {
        return As(typeof(TOther));
    }

    public TSelect Where(string whereString)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = null,
            ResolveOptions = SqlResolveOptions.Where,
            Template = whereString
        });
        return (this as TSelect)!;
    }


    public IEnumerable<T1> ToList()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query<T1>(sql, parameters);
    }

    public IEnumerable<dynamic> ToDynamicList()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query(sql, parameters);
    }

    public IEnumerable<TReturn> ToList<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query<TReturn>(sql, parameters);
    }

    public DataTable ToDataTable()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTable(sql, parameters);
    }

    public Task<IList<T1>> ToListAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QueryAsync<T1>(sql, parameters);
    }

    public Task<IList<dynamic>> ToDynamicListAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QueryAsync(sql, parameters);
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QueryAsync<TReturn>(sql, parameters);
    }

    public Task<DataTable> ToDataTableAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTableAsync(sql, parameters);
    }

    public async Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "MAX({0})";
        });
        var list = await ToListAsync<TMember>();
        return list.First();
    }

    public async Task<double> SumAsync(Expression<Func<T1, object>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "SUM({0})";
        });
        var list = await ToListAsync<double>();
        return list.First();
    }

    public async Task<int> CountAsync(Expression<Func<T1, object>> exp)
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = exp;
            e.Template = "COUNT({0})";
        });
        var list = await ToListAsync<int>();
        return list.First();
    }

    public async Task<int> CountAsync()
    {
        SqlBuilder.Expressions.Update(SelectExpression?.Id, e =>
        {
            e.Expression = null;
            e.Template = "COUNT(*)";
        });
        var list = await ToListAsync<int>();
        return list.First();
    }

    public async Task<bool> AnyAsync()
    {
        var c = await CountAsync();
        return c > 0;
    }

    public string ToSql()
    {
        return SqlBuilder.ToSqlString();
    }
}
