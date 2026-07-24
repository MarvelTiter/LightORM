

using LightORM.Implements;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LightORM.Providers.Select;

internal class SelectProvider0<TSelect,
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
T1> : IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect
{
    public SelectBuilder SqlBuilder { get; set; } = default!;
    public ISqlExecutor Executor => DbContext.Ado;
    public DbBaseType DbType => Executor.Database.DbBaseType;
    public IDatabaseAdapter Database => Executor.Database.DatabaseAdapter;
    public bool IsSubQuery { get; set; }
    public IContext DbContext { get; }

    protected ExpressionInfo? SelectExpression;
    public SelectProvider0(IContext dbContext, SelectBuilder? builder = null)
    {
        if (builder != null)
            SqlBuilder = builder;
        DbContext = dbContext;
    }
    public IExpTemp<T1> AsTemp(string name)
    {
        return new TempProvider<T1>(name, SqlBuilder);
    }

    #region 日志输出辅助

    public TSelect TagWith(string tag)
    {
        SqlBuilder.AddTag(new(tag, null, null, null, false));
        return (this as TSelect)!;
    }

    public TSelect TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null)
    {
        SqlBuilder.AddTag(new(tag, filePath, callMember, lineNum, true));
        return (this as TSelect)!;
    }

    #endregion

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

    public TSelect Where<TTable>(Expression<Func<TTable, bool>> exp)
    {
        var newExp = WhereLambdaParameterReplace.Default.Replace(exp, SqlBuilder);
        this.WhereHandle(newExp);
        return (this as TSelect)!;
    }
    public TSelect WhereIf<TTable>(bool condition, Expression<Func<TTable, bool>> exp)
    {
        if (condition) this.WhereHandle(exp);
        return (this as TSelect)!;
    }

    #endregion

    #region original sql

    public TSelect WithParameters<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(TParameter parameters)
    {
        SqlBuilder.TryAddParameters(Database.Prefix, "", parameters);
        return (this as TSelect)!;
    }
    public TSelect Where<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string sql, TParameter parameters)
    {
        SqlBuilder.Where.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect WhereIf<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(bool condition, string sql, TParameter parameters)
    {
        if (condition)
        {
            return Where(sql, parameters);
        }
        return (this as TSelect)!;
    }
    public TSelect GroupBy<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string sql, TParameter parameters)
    {
        SqlBuilder.GroupBy.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect Having<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string sql, TParameter parameters)
    {
        //SqlBuilder
        SqlBuilder.Having.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }
    public TSelect OrderBy<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TParameter>(string sql, TParameter parameters)
    {
        SqlBuilder.OrderBy.Add(sql);
        SqlBuilder.TryAddParameters(Database.Prefix, sql, parameters);
        return (this as TSelect)!;
    }

    public TSelect Where(string sql)
    {
        SqlBuilder.Where.Add(sql);
        return (this as TSelect)!;
    }
    public TSelect WhereIf(bool condition, string sql)
    {
        if (condition)
        {
            return Where(sql);
        }
        return (this as TSelect)!;
    }
    public TSelect GroupBy(string sql)
    {
        SqlBuilder.GroupBy.Add(sql);
        return (this as TSelect)!;
    }
    public TSelect Having(string sql)
    {
        //SqlBuilder
        SqlBuilder.Having.Add(sql);
        return (this as TSelect)!;
    }
    public TSelect OrderBy(string sql)
    {
        SqlBuilder.OrderBy.Add(sql);
        return (this as TSelect)!;
    }

    #endregion

    #region 

    public TSelect Count(out long total)
    {
        var count = new ExpressionInfo(SqlResolveOptions.Select, null, "COUNT(*)");
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
        return this.ExecuteScalar<TMember>();
    }

    public TMember? Min<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.ExecuteScalar<TMember>();
    }

    public double Sum<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.ExecuteScalar<double>();
    }

    public int Count<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.ExecuteScalar<int>();
    }

    public int Count()
    {
        this.HandleResult(null, "COUNT(*)");
        return this.ExecuteScalar<int>();
    }

    public double Avg<TMember>(Expression<Func<T1, TMember>> exp)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.ExecuteScalar<double>();
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
        return Executor.Execute(sql, parameters).Single<T1>();
    }

    public TReturn? First<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>()
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            throw new LightOrmException("未调用SelectColumns设置Select的列，不能推断具体是哪一列为TReturn，或者使用First<TReturn>(Expression<Func<..., TReturn>> exp)重载");
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.Execute(sql, parameters).Single<TReturn>();
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
        return Executor.Execute(sql, parameters).ToList<T1>();
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
        return await Executor.Execute(sql, parameters).SingleAsync<T1>(cancellationToken);
    }

    public  async Task<TReturn?> FirstAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(CancellationToken cancellationToken = default)
    {
        if (!SqlBuilder.Expressions.IsAlreadySetSelect())
        {
            throw new LightOrmException("未调用SelectColumns设置Select的列，不能推断具体是哪一列为TReturn，或者使用First<TReturn>(Expression<Func<..., TReturn>> exp)重载");
        }
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return await Executor.Execute(sql, parameters).SingleAsync<TReturn>(cancellationToken);
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
        return await Executor.Execute(sql, parameters).ToListAsync<T1>(cancellationToken);
    }

    public IAsyncEnumerable<T1> ToEnumerableAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T1, T1>> exp = t => t;
        this.HandleResult(exp, null);
        var sql = SqlBuilder.ToSqlString(Database);
        var parameters = SqlBuilder.DbParameters;
        return Executor.Execute(sql, parameters).ToAsyncList<T1>(cancellationToken);
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
        return this.ExecuteScalarAsync<TMember>(cancellationToken);
    }

    public Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "MIN({0})");
        return this.ExecuteScalarAsync<TMember>(cancellationToken);
    }

    public Task<double> SumAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "SUM({0})");
        return this.ExecuteScalarAsync<double>(cancellationToken);
    }

    public Task<int> CountAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "COUNT({0})");
        return this.ExecuteScalarAsync<int>(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        this.HandleResult(null, "COUNT(*)");
        return this.ExecuteScalarAsync<int>(cancellationToken);
    }

    public Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, "AVG({0})");
        return this.ExecuteScalarAsync<double>(cancellationToken);
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
