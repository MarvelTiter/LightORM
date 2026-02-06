using System.Threading;

#pragma warning disable CS0419
namespace LightORM.Interfaces.ExpSql;

public interface IExpSelect : ISql
{
    internal bool IsSubQuery { get; set; }
    internal ISqlExecutor Executor { get; }
    internal SelectBuilder SqlBuilder { get; }
}

public interface IExpSelect0<out TSelect, T1> : IExpSelect where TSelect : IExpSelect
{
    TSelect Count(out long total);
    TSelect Where(Expression<Func<T1, bool>> exp);

    TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp);

    IEnumerable<T1> ToList();
    T1? First();
    DataTable ToDataTable();
    Task<IList<T1>> ToListAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<T1> ToEnumerableAsync(CancellationToken cancellationToken = default);
    Task<T1?> FirstAsync(CancellationToken cancellationToken = default);
    Task<DataTable> ToDataTableAsync(CancellationToken cancellationToken = default);
    Task<TMember?> MaxAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<TMember?> MinAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<double> SumAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<int> CountAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> exp, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    TSelect Paging(int pageIndex, int pageSize);
    TSelect Skip(int count);
    TSelect Take(int count);
    TMember? Max<TMember>(Expression<Func<T1, TMember>> exp);
    TMember? Min<TMember>(Expression<Func<T1, TMember>> exp);
    double Sum<TMember>(Expression<Func<T1, TMember>> exp);
    int Count<TMember>(Expression<Func<T1, TMember>> exp);
    int Count();
    double Avg<TMember>(Expression<Func<T1, TMember>> exp);

    bool Any();

    //TSelect RollUp();
    TSelect Distinct();
    IExpTemp<T1> AsTemp(string name);
    TSelect Parameterized(bool use = true);

    #region 使用原生sql

    /// <summary>
    /// 共享参数, 跟<see cref="Where(string, object?)"/>, <see cref="WhereIf(bool, string, object?)"/>等搭配使用
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    TSelect WithParameters<TParameter>(TParameter parameters);

    TSelect Where(string sql, object? parameters = default!);
    TSelect WhereIf(bool condition, string sql, object? parameters = default!);
    TSelect GroupBy(string sql, object? parameters = default!);
    TSelect Having(string sql, object? parameters = default!);

    #endregion
}

public interface IExpSelect<T1> : IExpSelect0<IExpSelect<T1>, T1>
{
    IExpSelect<T1> OrderBy<TOrder>(Expression<Func<T1, TOrder>> exp);
    IExpSelect<T1> OrderByDesc<TOrder>(Expression<Func<T1, TOrder>> exp);
    IExpSelectGroup<TGroup, T1> GroupBy<TGroup>(Expression<Func<T1, TGroup>> exp);

    #region join methods

    // IExpSelect<T1, TJoin> Join<TJoin>(TableLinkType joinType, Expression<Func<T1, TJoin, bool>> exp);

    IExpSelect<T1, TJoin> InnerJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<T1, TJoin, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpTemp<TJoin> subQuery, Expression<Func<T1, TJoin, bool>> where);

    #endregion

    IExpInclude<T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> exp);
    IExpSelect<T1> As(string alias);

    /// <summary>
    /// 在外面套一层SELECT * FROM ( current ), 将当前查询转换为子查询
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    IExpSelect<T1> AsSubQuery(string? alias = null);

    /// <summary>
    /// 转换成<see cref="IExpSelect{T1}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<T1, TTable>> exp);

    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, TTemp>> exp);
    IExpSelect<T1> Union(IExpSelect<T1> select);
    IExpSelect<T1> UnionAll(IExpSelect<T1> select);
    string ToSql(Expression<Func<T1, object>> exp);
    ISelectInsert<TInsertTable> Insert<TInsertTable>();
    ISelectInsert<TInsertTable> Insert<TInsertTable>(Expression<Func<TInsertTable, object>> exp);
    ISelectInsert<object> Insert(string tableName, string[] columns);

    //Task<int> InsertAsync<TInsertTable>(CancellationToken cancellationToken = default);
    //Task<int> InsertAsync<TInsertTable>(Expression<Func<TInsertTable, object>> exp, CancellationToken cancellationToken = default);
    //Task<int> InsertAsync(string tableName, string[] columns, CancellationToken cancellationToken = default);

    #region Result

    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> exp, CancellationToken cancellationToken = default);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default);


    /// <summary>
    /// 需要确保SELECT的列名与TReturn类型的属性名一致
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    /// <returns></returns>
    IEnumerable<TReturn> ToList<TReturn>();

    /// <summary>
    /// 需要确保SELECT的列名与TReturn类型的属性名一致
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    /// <returns></returns>
    Task<IList<TReturn>> ToListAsync<TReturn>(CancellationToken cancellationToken = default);

    /// <summary>
    /// 需要确保SELECT的列名与TReturn类型的属性名一致
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    /// <returns></returns>
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(CancellationToken cancellationToken = default);

    #endregion

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
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, TJoin>, bool>> exp);
    IExpSelect<T1, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);
    IExpSelect<T1, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, TJoin>, bool>> where);

    #endregion
}

public interface ISelectInsert<T> : ISql
{
    int Execute();
    Task<int> ExecuteAsync();
}

public interface IExpTemp
{
    string Id { get; }
    TableInfo ResultTable { get; }
    internal SelectBuilder SqlBuilder { get; }
}

public interface IExpTemp<TTemp> : IExpTemp
{
    //IExpSelect<TTemp> AsSelect(string? alias = null);
}

public interface IExpSelect<T1, T2> : IExpSelect0<IExpSelect<T1, T2>, T1>
{
    IExpSelect<T1, T2> OrderBy<TOrder>(Expression<Func<T1, T2, TOrder>> exp);
    IExpSelect<T1, T2> OrderByDesc<TOrder>(Expression<Func<T1, T2, TOrder>> exp);

    IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);

    //IExpSelect<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp);
    IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<T1, T2, TGroup>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<T1, T2, TJoin, bool>> exp);

    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<T1, T2, TJoin, bool>> where);

    //IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<T1, T2, TTemp>> exp, string? alias = null);
    /// <summary>
    /// 转换成<see cref="IExpSelect{TTable}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<T1, T2, TTable>> exp);

    /// <summary>
    /// 转换成WITH查询，用于<see cref="WithTempQuery"/>
    /// </summary>
    /// <typeparam name="TTemp"></typeparam>
    /// <param name="name"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<T1, T2, TTemp>> exp);

    string ToSql(Expression<Func<T1, T2, object>> exp);

    #region Result

    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default);

    #endregion

    #region WithTemp

    IExpSelect<T1, T2, TTemp> WithTempQuery<TTemp>(IExpTemp<TTemp> temp);
    IExpSelect<T1, T2, TTemp1, TTemp2> WithTempQuery<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3> WithTempQuery<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4);
    IExpSelect<T1, T2, TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> WithTempQuery<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp3> temp3, IExpTemp<TTemp4> temp4, IExpTemp<TTemp5> temp5);

    #endregion

    #region TypeSet

    IExpSelect<T1, T2> OrderBy<TOrder>(Expression<Func<TypeSet<T1, T2>, TOrder>> exp);
    IExpSelect<T1, T2> OrderByDesc<TOrder>(Expression<Func<TypeSet<T1, T2>, TOrder>> exp);

    IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp);

    //IExpSelect<T1, T2> WhereIf(bool condition, Expression<Func<TypeSet<T1, T2>, bool>> exp);
    IExpSelectGroup<TGroup, TypeSet<T1, T2>> GroupBy<TGroup>(Expression<Func<TypeSet<T1, T2>, TGroup>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> exp);
    IExpSelect<T1, T2, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where);
    IExpSelect<T1, T2, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where);
    IExpSelect<T1, T2, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where);
    IExpSelect<T1, T2, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<T1, T2, TJoin>, bool>> where);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default);

    ///// <summary>
    ///// 外部套一层 SELECT * FROM ( ... ) 后转换成<see cref="IExpSelect{T1}"/>
    ///// </summary>
    ///// <typeparam name="TTemp"></typeparam>
    ///// <param name="exp"></param>
    ///// <param name="alias"></param>
    ///// <returns></returns>
    //IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<TypeSet<T1, T2>, TTemp>> exp, string? alias = null);
    /// <summary>
    /// 转换成<see cref="IExpSelect{T1}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<TypeSet<T1, T2>, TTable>> exp);

    /// <summary>
    /// 转换成WITH查询
    /// </summary>
    /// <typeparam name="TTemp"></typeparam>
    /// <param name="name"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<TypeSet<T1, T2>, TTemp>> exp);

    string ToSql(Expression<Func<TypeSet<T1, T2>, object>> exp);

    #endregion
}