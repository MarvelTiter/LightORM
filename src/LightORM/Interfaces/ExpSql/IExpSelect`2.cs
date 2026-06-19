using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM;

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
    IExpSelect<TTable> AsTable<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TTable>(Expression<Func<T1, T2, TTable>> exp);

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

    IEnumerable<TReturn> ToList<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default);
    IEnumerable<TReturn> ToList<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, object>> exp);
    Task<IList<TReturn>> ToListAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default);

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
    IEnumerable<TReturn> ToList<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default);
    IEnumerable<TReturn> ToList<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp);
    Task<IList<TReturn>> ToListAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TReturn>(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default);

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
    IExpSelect<TTable> AsTable<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TTable>(Expression<Func<TypeSet<T1, T2>, TTable>> exp);

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