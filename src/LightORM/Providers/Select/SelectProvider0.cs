

namespace LightORM.Providers.Select;

internal class SelectProvider0<TSelect, T1> : IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect
{
    public SelectBuilder SqlBuilder { get; set; } = default!;
    public ISqlExecutor Executor { get; }
    public DbBaseType DbType => Executor.Database.DbBaseType;
    public bool IsSubQuery { get; set; }
    protected ExpressionInfo? SelectExpression;
    public SelectProvider0(ISqlExecutor executor, SelectBuilder? builder = null)
    {
        Executor = executor;
        if (builder != null)
            SqlBuilder = builder;
    }
    public IExpTemp<T1> AsTemp(string name)
    {
        return new TempProvider<T1>(name, SqlBuilder);
    }

    #region where

    public TSelect Where(Expression<Func<T1, bool>> exp)
    {
        this.WhereHandle(exp);
        return (this as TSelect)!;
    }
    public TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp)
    {
        this.WhereHandle(exp);
        return (this as TSelect)!;
    }

    public TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp)
    {
        if (condition)
        {
            this.WhereHandle(exp);
        }
        return (this as TSelect)!;
    }

    public TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp)
    {
        if (condition) this.WhereHandle(exp);
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
        total = this.ToList<int>().First();
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

    #endregion

    #region ToResult
    public bool Any()
    {
        return Count() > 0;
    }

    public TMember? Max<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MAX({0})");
        return this.Single<TMember>();
    }

    public TMember? Min<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.Single<TMember>();
    }

    public double Sum(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.Single<double>();
    }

    public int Count(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.Single<int>();
    }

    public int Count()
    {
        this.HandleResult(null, "COUNT(*)");
        return this.Single<int>();
    }

    public double Avg(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.Single<double>();
    }

    public virtual T1? First()
    {
        Expression<Func<T1, T1>> exp = t => t;
        this.HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.QuerySingle<T1>(sql, parameters);
    }

    public virtual IEnumerable<T1> ToList()
    {
        Expression<Func<T1, T1>> exp = t => t;
        this.HandleResult(exp, null);
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
        this.HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QuerySingleAsync<T1>(sql, parameters);
    }

    public virtual async Task<IList<T1>> ToListAsync()
    {
        Expression<Func<T1, T1>> exp = t => t;
        this.HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QueryAsync<T1>(sql, parameters);
    }

    public Task<DataTable> ToDataTableAsync()
    {
        var sql = SqlBuilder.ToSqlString();
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTableAsync(sql, parameters);
    }

    public Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MAX({0})");
        return this.SingleAsync<TMember>();
    }

    public Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.SingleAsync<TMember>();
    }

    public Task<double> SumAsync(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.SingleAsync<double>();
    }

    public Task<int> CountAsync(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.SingleAsync<int>();
    }

    public Task<int> CountAsync()
    {
        this.HandleResult(null, "COUNT(*)");
        return this.SingleAsync<int>();
    }

    public Task<double> AvgAsync(Expression<Func<T1, object>> exp)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.SingleAsync<double>();
    }

    public async Task<bool> AnyAsync()
    {
        var c = await CountAsync();
        return c > 0;
    }


    #endregion

    public TSelect Paging(int pageIndex, int pageSize)
    {
        SqlBuilder.PageIndex = pageIndex;
        SqlBuilder.PageSize = pageSize;
        return (this as TSelect)!;
    }

    public string ToSql() => SqlBuilder.ToSqlString();

}
