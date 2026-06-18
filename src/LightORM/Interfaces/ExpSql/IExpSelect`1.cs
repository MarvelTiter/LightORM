using System.Threading;

namespace LightORM;

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