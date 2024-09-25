using System.Linq;
using LightORM.Cache;
using LightORM.ExpressionSql;
using System.Data;
using LightORM.Extension;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Providers.Select;

internal class SelectProvider0<TSelect, T1> : IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect
{
    [NotNull] public SelectBuilder? SqlBuilder { get; set; }
    public ISqlExecutor Executor { get; }
    public DbBaseType DbType => Executor.ConnectInfo.DbBaseType;

    protected ExpressionInfo? SelectExpression;
    public SelectProvider0(ISqlExecutor executor, SelectBuilder? builder = null)
    {
        Executor = executor;
        if (builder != null)
            SqlBuilder = builder;
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

    protected void JoinHandle<TJoin>(Expression exp, TableLinkType joinType)
    {
        var expression = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = exp,
        };

        SqlBuilder.Expressions.Add(expression);

        SqlBuilder.Joins.Add(new JoinInfo()
        {
            ExpressionId = expression.Id,
            JoinType = joinType,
            EntityInfo = Cache.TableContext.GetTableInfo<TJoin>()
        });
        //SqlBuilder.OtherTables.Add()
        //return (this as TSelect)!;
    }

    //public void InnerJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    //{
    //    return JoinHandle<TAnother>(exp, TableLinkType.InnerJoin);
    //}

    //public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.InnerJoin);
    //}

    //public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.InnerJoin);
    //}

    //public TSelect LeftJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    //{
    //    return JoinHandle<TAnother>(exp, TableLinkType.LeftJoin);
    //}

    //public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.LeftJoin);
    //}

    //public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.LeftJoin);
    //}

    //public TSelect RightJoin<TAnother>(Expression<Func<TAnother, T1, bool>> exp)
    //{
    //    return JoinHandle<TAnother>(exp, TableLinkType.RightJoin);
    //}

    //public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.RightJoin);
    //}

    //public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
    //{
    //    return JoinHandle<TAnother1>(exp, TableLinkType.RightJoin);
    //}

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

    public TSelect WithParameters(object parameters)
    {
        return (this as TSelect)!;
    }

    public TSelect Where(string sql, object? parameters = null)
    {
        SqlBuilder.Where.Add(sql);
        SqlBuilder.TryAddParameters(sql, parameters);
        return (this as TSelect)!;
    }

    public TSelect WhereIf(bool condition, string sql, object? parameters = null)
    {
        if (condition)
        {
            return Where(sql, parameters);
        }
        return (this as TSelect)!;
    }
    public TSelect GroupBy(string sql, object? parameters = null)
    {
        SqlBuilder.GroupBy.Add(sql);
        SqlBuilder.TryAddParameters(sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect Having(string sql, object? parameters = null)
    {
        //SqlBuilder
        return (this as TSelect)!;
    }
    public TSelect OrderBy(string sql, object? parameters = null)
    {
        SqlBuilder.OrderBy.Add(sql);
        SqlBuilder.TryAddParameters(sql, parameters);
        return (this as TSelect)!;
    }

    #endregion

    #region group

    protected IExpGroupSelect<TGroup, TTables> GroupByHandle<TGroup, TTables>(Expression exp)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            ResolveOptions = SqlResolveOptions.Group,
            Expression = exp
        });
        return new GroupSelectProvider<TGroup, TTables>(Executor, SqlBuilder);
    }

    //public TSelect GroupBy<Another>(Expression<Func<Another, object>> exp)
    //{
    //    return GroupByHandle(exp.Body);
    //}

    //public TSelect GroupByIf<Another>(bool ifGroupby, Expression<Func<Another, bool>> exp)
    //{
    //    if (ifGroupby) return GroupByHandle(exp.Body);
    //    return (this as TSelect)!;
    //}

    #endregion

    #region 

    public TSelect Count(out long total)
    {
        var count = new ExpressionInfo()
        {
            Expression = null,
            ResolveOptions = SqlResolveOptions.Select,
            Template = "COUNT(*)"
        };
        SqlBuilder.Expressions.Add(count);
        total = ToList<int>().First();
        SqlBuilder.Expressions.Remove(count);
        //SqlBuilder.Expressions.Update(SelectExpression?.Id, SelectExpression);
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

    //public TSelect As(string tableName)
    //{
    //    SqlBuilder.TableInfo.CustomName = (tableName);
    //    return (this as TSelect)!;
    //}

    //public TSelect As(Type type)
    //{
    //    var info = Cache.TableContext.GetTableInfo(type);
    //    SqlBuilder.TableInfo.CustomName = (info.TableName);
    //    return (this as TSelect)!;
    //}

    //public TSelect As<TOther>()
    //{
    //    return As(typeof(TOther));
    //}

    public TSelect As(string alias)
    {
        SqlBuilder.MainTable.Alias = alias;
        return (this as TSelect)!;
    }

    #endregion

    #region ToResult
    public bool Any()
    {
        return Count() > 0;
    }

    public TMember? Max<TMember>(Expression<Func<T1, TMember>> exp)
    {
        HandleResult(exp, "MAX({0})");
        return Single<TMember>();
    }

    public TMember? Min<TMember>(Expression<Func<T1, TMember>> exp)
    {
        HandleResult(exp, "MIN({0})");
        return Single<TMember>();
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "SUM({0})");
        return Single<double>();
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "COUNT({0})");
        return Single<int>();
    }

    public int Count()
    {
        HandleResult(null, "COUNT(*)");
        return Single<int>();
    }

    public double Avg(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "AVG({0})");
        return Single<double>();
    }

    public virtual T1? First()
    {
        Expression<Func<T1, T1>> exp = t => t;
        HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QuerySingle<T1>(sql, parameters);
    }

    public virtual IEnumerable<T1> ToList()
    {
        Expression<Func<T1, T1>> exp = t => t;
        HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query<T1>(sql, parameters);
    }

    public DataTable ToDataTable()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTable(sql, parameters);
    }

    public virtual async Task<T1?> FirstAsync()
    {
        Expression<Func<T1, T1>> exp = t => t;
        HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QuerySingleAsync<T1>(sql, parameters);
    }

    public virtual async Task<IList<T1>> ToListAsync()
    {
        Expression<Func<T1, T1>> exp = t => t;
        HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QueryAsync<T1>(sql, parameters);
    }

    protected IEnumerable<TReturn> ToList<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query<TReturn>(sql, parameters);
    }

    protected TReturn? Single<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QuerySingle<TReturn>(sql, parameters);
    }

    protected Task<IList<TReturn>> ToListAsync<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QueryAsync<TReturn>(sql, parameters);
    }
    protected Task<TReturn?> SingleAsync<TReturn>()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QuerySingleAsync<TReturn>(sql, parameters);
    }

    public Task<DataTable> ToDataTableAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTableAsync(sql, parameters);
    }

    public Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        HandleResult(exp, "MAX({0})");
        return SingleAsync<TMember>();
    }

    public Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        HandleResult(exp, "MIN({0})");
        return SingleAsync<TMember>();
    }

    public Task<double> SumAsync(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "SUM({0})");
        return SingleAsync<double>();
    }

    public Task<int> CountAsync(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "COUNT({0})");
        return SingleAsync<int>();
    }

    public Task<int> CountAsync()
    {
        HandleResult(null, "COUNT(*)");
        return SingleAsync<int>();
    }

    public Task<double> AvgAsync(Expression<Func<T1, object>> exp)
    {
        HandleResult(exp, "AVG({0})");
        return SingleAsync<double>();
    }

    public async Task<bool> AnyAsync()
    {
        var c = await CountAsync();
        return c > 0;
    }

    protected void HandleResult(Expression? exp, string? template)
    {
        SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Select,
            Template = template
        });
    }

    #endregion

    public TSelect Paging(int pageIndex, int pageSize)
    {
        SqlBuilder.PageIndex = pageIndex;
        SqlBuilder.PageSize = pageSize;
        return (this as TSelect)!;
    }

    public TSelect From(Func<IExpSelect> sub)
    {
        var subSelect = sub.Invoke();
        //TODO 子查询
        return (this as TSelect)!;
    }

    public string ToSql() => SqlBuilder.ToSqlString();

    public TSelect UnionAll(params IExpSelect[] querys)
    {
        throw new NotImplementedException();
    }
}
