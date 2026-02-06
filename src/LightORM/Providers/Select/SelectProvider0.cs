

using LightORM.Implements;
using System.Text;
using System.Threading;

namespace LightORM.Providers.Select;

internal class SelectProvider0<TSelect, T1> : IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect
{
    public SelectBuilder SqlBuilder { get; set; } = default!;
    public ISqlExecutor Executor { get; }
    public DbBaseType DbType => Executor.Database.DbBaseType;
    public ICustomDatabase Database => Executor.Database.CustomDatabase;
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


    public TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp)
    {
        if (condition)
        {
            this.WhereHandle(exp);
        }
        return (this as TSelect)!;
    }

    public TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp)
    {
        this.WhereHandle(exp);
        return (this as TSelect)!;
    }
    public TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp)
    {
        if (condition) this.WhereHandle(exp);
        return (this as TSelect)!;
    }

    public TSelect WithParameters<TParameter>(TParameter parameters)
    {
        //if (parameters is not null)
        //{
        //    if (parameters is not IDictionary<string, object> dic)
        //    {
        //        var reader = DbParameterReader.CreateReadToDictionary("", typeof(TParameter));
        //        dic = reader.Invoke(parameters);
        //    }
        //    SqlBuilder.DbParameters.TryAddDictionary(dic);
        //}

        SqlBuilder.TryAddParameters(Database.Prefix, "", parameters);
        return (this as TSelect)!;
    }

    public TSelect Where(string sql, object? parameters = default!)
    {
        SqlBuilder.Where.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }

    public TSelect WhereIf(bool condition, string sql, object? parameters = default!)
    {
        if (condition)
        {
            return Where(sql, parameters);
        }
        return (this as TSelect)!;
    }
    public TSelect GroupBy(string sql, object? parameters = default!)
    {
        SqlBuilder.GroupBy.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect Having(string sql, object? parameters = default!)
    {
        //SqlBuilder
        SqlBuilder.Having.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect OrderBy(string sql, object? parameters = default!)
    {
        SqlBuilder.OrderBy.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
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
        total = this.InternalToList<int>().First();
        SqlBuilder.Expressions.Remove(count);
        //SqlBuilder.Expressions.Update(SelectExpression?.Id, SelectExpression);
        return (this as TSelect)!;
    }

    //public TSelect RollUp()
    //{
    //    SqlBuilder.IsRollup = true;
    //    return (this as TSelect)!;
    //}

    public TSelect Distinct()
    {
        SqlBuilder.IsDistinct = true;
        return (this as TSelect)!;
    }

    public TSelect Parameterized(bool use = true)
    {
        SqlBuilder.IsParameterized = use;
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
        return this.InternalSingle<TMember>();
    }

    public TMember? Min<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.InternalSingle<TMember>();
    }

    public double Sum<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.InternalSingle<double>();
    }

    public int Count<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.InternalSingle<int>();
    }

    public int Count()
    {
        this.HandleResult(null, "COUNT(*)");
        return this.InternalSingle<int>();
    }

    public double Avg<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.InternalSingle<double>();
    }

    public virtual T1? First()
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            Expression<Func<T1, T1>> exp = t => t;
            this.HandleResult(exp, null);
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.QuerySingle<T1>(sql, parameters);
    }

    public virtual IEnumerable<T1> ToList()
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            Expression<Func<T1, T1>> exp = t => t;
            this.HandleResult(exp, null);
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.Query<T1>(sql, parameters);
    }

    public DataTable ToDataTable()
    {
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTable(sql, parameters);
    }

    public virtual async Task<T1?> FirstAsync(CancellationToken cancellationToken = default)
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            Expression<Func<T1, T1>> exp = t => t;
            this.HandleResult(exp, null);
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QuerySingleAsync<T1>(sql, parameters, cancellationToken: cancellationToken);
    }

    public virtual async Task<IList<T1>> ToListAsync(CancellationToken cancellationToken = default)
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            Expression<Func<T1, T1>> exp = t => t;
            this.HandleResult(exp, null);
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return await Executor.QueryListAsync<T1>(sql, parameters, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<T1> ToEnumerableAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T1, T1>> exp = t => t;
        this.HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.QueryAsync<T1>(sql, parameters, cancellationToken: cancellationToken);
    }

    public Task<DataTable> ToDataTableAsync(CancellationToken cancellationToken = default)
    {
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
    }

    public Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "MAX({0})");
        return this.InternalSingleAsync<TMember>(cancellationToken);
    }

    public Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.InternalSingleAsync<TMember>(cancellationToken);
    }

    public Task<double> SumAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.InternalSingleAsync<double>(cancellationToken);
    }

    public Task<int> CountAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.InternalSingleAsync<int>(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        this.HandleResult(null, "COUNT(*)");
        return this.InternalSingleAsync<int>(cancellationToken);
    }

    public Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.InternalSingleAsync<double>(cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        var c = await CountAsync(cancellationToken);
        return c > 0;
    }



    #endregion

    public TSelect Paging(int pageIndex, int pageSize)
    {
        //SqlBuilder.PageIndex = pageIndex;
        //SqlBuilder.PageSize = pageSize;
        SqlBuilder.Skip = (pageIndex - 1) * pageSize;
        SqlBuilder.Take = pageSize;
        return (this as TSelect)!;
    }

    public TSelect Skip(int count)
    {
        SqlBuilder.Skip = count;
        return (this as TSelect)!;
    }

    public TSelect Take(int count)
    {
        SqlBuilder.Take = count;
        return (this as TSelect)!;
    }

    public string ToSql() => SqlBuilder.ToSqlString(Database);

    public string ToSqlWithParameters()
    {
        var sql = SqlBuilder.ToSqlString(Database);
        StringBuilder sb = new(sql);
        sb.AppendLine();
        sb.AppendLine("参数列表: ");
        foreach (var item in SqlBuilder.DbParameters)
        {
            sb.AppendLine($"{item.Key} - {item.Value}");
        }
        return sb.ToString();
    }
}
