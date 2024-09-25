using LightORM.Builder;
using System.Data;
using System.Threading.Tasks;

namespace LightORM;

public interface IExpSelect : ISql
{
    //IEnumerable<TReturn> ToList<TReturn>();
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TReturn>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>();
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TReturn>> exp);
    //IEnumerable<dynamic> ToDynamicList();
    //Task<IList<dynamic>> ToDynamicListAsync();
}
public interface IExpSelect0<TSelect, T1> : IExpSelect where TSelect : IExpSelect
{
    TSelect Count(out long total);
    TSelect Where(Expression<Func<T1, bool>> exp);
    TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp);
    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);
    TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp);
    TSelect GroupBy<Another>(Expression<Func<Another, object>> exp);
    TSelect GroupByIf<Another>(bool ifGroupby, Expression<Func<Another, bool>> exp);
    IEnumerable<T1> ToList();
    T1? First();
    DataTable ToDataTable();
    Task<IList<T1>> ToListAsync();
    Task<T1?> FirstAsync();
    Task<DataTable> ToDataTableAsync();
    Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp);
    Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp);
    Task<double> SumAsync(Expression<Func<T1, object>> exp);
    Task<int> CountAsync(Expression<Func<T1, object>> exp);
    Task<int> CountAsync();
    Task<double> AvgAsync(Expression<Func<T1, object>> exp);
    Task<bool> AnyAsync();

    TSelect Paging(int pageIndex, int pageSize);
    TMember? Max<TMember>(Expression<Func<T1, TMember>> exp);
    TMember? Min<TMember>(Expression<Func<T1, TMember>> exp);
    double Sum(Expression<Func<T1, object>> exp);
    int Count(Expression<Func<T1, object>> exp);
    int Count();
    double Avg(Expression<Func<T1, object>> exp);
    bool Any();
    TSelect RollUp();
    TSelect Distinct();
    TSelect From(Func<IExpSelect> sub);
    
    //IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, object>> exp);
    TSelect UnionAll(params IExpSelect[] querys);
    #region 使用原生sql
    /// <summary>
    /// 共享参数
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    TSelect WithParameters(object parameters);
    TSelect Where(string sql, object? parameters = null);
    TSelect WhereIf(bool condition, string sql, object? parameters = null);
    TSelect GroupBy(string sql, object? parameters = null);
    TSelect Having(string sql, object? parameters = null);
    TSelect OrderBy(string sql, object? parameters = null);
    #endregion
}

public interface IExpSelect<T1> : IExpSelect0<IExpSelect<T1>, T1>
{
    IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp, bool asc = true);
    IExpSelect<T1> GroupBy(Expression<Func<T1, object>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> exp);
    IExpSelect<T1> As(string alias);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, object>> exp);
    string ToSql(Expression<Func<T1, object>> exp);
}

public interface IExpInclude<T1, TMember> : IExpSelect<T1>
{
    internal SelectBuilder SqlBuilder { get; set; }
    internal ISqlExecutor Executor { get; }
    //internal IncludeContext IncludeContext { get; set; }
}

public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
{
    IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp, bool asc = true);
    IExpSelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> exp);
    IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
    IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp, bool asc = true);
    IExpSelect<T1, T2> GroupBy(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, T2, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, T2, object>> exp);
    //IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    //Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<T1, T2>, object>> exp);
    string ToSql(Expression<Func<T1, T2, object>> exp);
    string ToSql(Expression<Func<TypeSet<T1, T2>, object>> exp);
}
