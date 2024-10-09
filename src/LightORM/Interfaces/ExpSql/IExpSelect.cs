using LightORM.Builder;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightORM.Interfaces.ExpSql;

public interface IExpSelect : ISql
{
    internal ISqlExecutor Executor { get; }
    internal SelectBuilder SqlBuilder { get; }
}
public interface IExpSelect0<TSelect, T1> : IExpSelect where TSelect : IExpSelect
{
    TSelect Count(out long total);
    TSelect Where(Expression<Func<T1, bool>> exp);
    TSelect Where<TWhere>(Expression<Func<TWhere, bool>> exp);
    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);
    TSelect WhereIf<TWhere>(bool condition, Expression<Func<TWhere, bool>> exp);
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
    IExpTemp<T1> AsTemp(string name);

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
    //IExpSelect<T1> Named(string tableName);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, TTemp>> exp);
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, TTemp>> exp);
    IExpSelect<T1> Union(IExpSelect<T1> select);
    IExpSelect<T1> UnionAll(IExpSelect<T1> select);
    string ToSql(Expression<Func<T1, object>> exp);
    int Insert<TInsertTable>();
    Task<int> InsertAsync<TInsertTable>();
    int Insert<TInsertTable>(Expression<Func<TInsertTable, object>> exp);
    Task<int> InsertAsync<TInsertTable>(Expression<Func<TInsertTable, object>> exp);
    int Insert(string tableName, params string[] columns);
    Task<int> InsertAsync(string tableName, params string[] columns);

    #region WithTemp
    IExpSelect<T1, TTemp> WithTempQuery<TTemp>(IExpTemp<TTemp> temp);
    IExpSelect<T1, TTemp1, TTemp2> WithTempQuery<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2);
    IExpSelect<T1, TTemp1, TTemp2, TTemp3> WithTempQuery<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3);
    IExpSelect<T1, TTemp1, TTemp2, TTemp3, TTemp4> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4);
    IExpSelect<T1, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4, IExpTemp<TTemp5> temp5);
    #endregion

    #region TypeSet
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    #endregion

}

public interface IExpTemp
{
    ITableEntityInfo ResultTable { get; }
    internal SelectBuilder SqlBuilder { get; }
}

public interface IExpTemp<TTemp> : IExpTemp
{

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
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, T2, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, T2, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, T2, TTemp>> exp);
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, T2, TTemp>> exp);

    string ToSql(Expression<Func<T1, T2, object>> exp);

    #region WithTemp
    IExpSelect<T1, T2, TTemp> WithTempQuery<TTemp>(IExpTemp<TTemp> temp);
    IExpSelect<T1, T2, TTemp1, TTemp2> WithTempQuery<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3> WithTempQuery<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4, IExpTemp<TTemp5> temp5);
    #endregion

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
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<T1, T2>, object>> exp);
    IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<TypeSet<T1, T2>, TTemp>> exp);
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<TypeSet<T1, T2>, TTemp>> exp);
    string ToSql(Expression<Func<TypeSet<T1, T2>, object>> exp);
    #endregion
}
