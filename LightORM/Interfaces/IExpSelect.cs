using LightORM.Builder;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightORM;

public interface IExpSelect : ISql
{
    internal SelectBuilder SqlBuilder { get; }
}
public interface IExpSelect0<TSelect, T1> : IExpSelect where TSelect : IExpSelect
{
    TSelect Count(out long total);
    TSelect Where(Expression<Func<T1, bool>> exp);
    TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp);
    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);
    TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp);
    //TSelect GroupBy<Another>(Expression<Func<Another, object>> exp);
    //TSelect GroupByIf<Another>(bool ifGroupby, Expression<Func<Another, bool>> exp);
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
    IExpSelect<T1> OrderBy(Expression<Func<T1, object>> exp);
    IExpSelect<T1> OrderByDesc(Expression<Func<T1, object>> exp);
    IExpSelectGroup<TGroup, T1> GroupBy<TGroup>(Expression<Func<T1, TGroup>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> exp);
    IExpSelect<T1> As(string alias);
    IExpSelect<T1> Named(string tableName);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, TTemp>> exp);
    IExpSelect<T1, TTemp> WithTempQuery<TTemp>(IExpSelect<TTemp> temp);
    string ToSql(Expression<Func<T1, object>> exp);

    #region TypeSet
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    #endregion

}

public interface IExpInclude<T1, TMember> : IExpSelect<T1>
{
    internal ISqlExecutor Executor { get; }
}

public interface IExpSelectGroup<TGroup, TTables>
{
    IExpSelectGroup<TGroup, TTables> Having(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp);
    IExpSelectGroup<TGroup, TTables> OrderBy(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp);
    IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp);
    IExpSelect<TTemp> ToSelect<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp);
    IExpSelect<TTemp> AsTempQuery<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp);
    string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    string ToSql();
}
public interface IExpSelectGrouping<TGroup, TTables>
{
    TGroup Group { get; set; }
    TTables Tables { get; set; }
    int Count();
    int Count<TColumn>(TColumn column);
    decimal Sum<TColumn>(TColumn column);
    decimal Sum<TColumn>(bool exp, TColumn column);
    decimal Avg<TColumn>(TColumn column);
    decimal Avg<TColumn>(bool exp, TColumn column);
    TColumn Max<TColumn>(TColumn column);
    TColumn Max<TColumn>(bool exp, TColumn column);
    TColumn Min<TColumn>(TColumn column);
    TColumn Min<TColumn>(bool exp, TColumn column);

}

public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
{
    IExpSelect<T1, T2> OrderBy(Expression<Func<T1, T2, object>> exp);
    IExpSelect<T1, T2> OrderByDesc(Expression<Func<T1, T2, object>> exp);
    IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
    IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<T1, T2, TGroup>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, T2, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, T2, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, T2, TTemp>> exp);
    string ToSql(Expression<Func<T1, T2, object>> exp);

    #region TypeSet
    IExpSelect<T1, T2> OrderBy(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<T1, T2> OrderByDesc(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);
    IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<TypeSet<T1, T2>, TGroup>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<TypeSet<T1, T2>, TTemp>> exp);
    string ToSql(Expression<Func<TypeSet<T1, T2>, object>> exp);
    #endregion
}
