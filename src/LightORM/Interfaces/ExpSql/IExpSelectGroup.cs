namespace LightORM.Interfaces.ExpSql;

public interface IExpSelectGroup<TGroup, TTables> : IExpSelect
{
    internal LambdaExpression KeySelector { get; }
    IExpSelectGroup<TGroup, TTables> Having(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp);
    IExpSelectGroup<TGroup, TTables> OrderBy(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize);
    IExpSelectGroup<TGroup, TTables> Skip(int count);
    IExpSelectGroup<TGroup, TTables> Take(int count);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    /// <summary>
    /// 转换为<see cref="IExpSelect{T1}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTable>> exp, string? alias = null);
    IExpSelectGroup<TGroup, TTables> Rollup();
    //IExpSelectGroup<TGroup, TTables> Rollup(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp);
    string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp);
}

public interface IExpSelectGrouping<TGroup, TTables>
{
    TGroup Group { get; set; }
    TTables Tables { get; set; }
    /// <summary>
    /// <para>COUNT(*)</para>
    /// 等价于<see cref="SqlFn.Count()"/>
    /// </summary>
    /// <returns></returns>
    int Count();
    /// <summary>
    /// <para>当T为返回bool的表达式或者三元表达式时，会解析成CASE WHEN语句</para>
    /// <para>否则COUNT(column)</para>
    /// 等价于<see cref="SqlFn.Count{T}(T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    int Count<TColumn>(TColumn column);
    /// <summary>
    /// <para>当T为返回bool的表达式或者三元表达式时，会解析成CASE WHEN语句</para>
    /// <para>否则COUNT(DISTINCT column)</para>
    /// 等价于<see cref="SqlFn.CountDistinct{T}(T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    int CountDistinct<TColumn>(TColumn column);
    /// <summary>
    /// <para>SUM(val)</para>
    /// 等价于<see cref="SqlFn.Sum{T}(T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    double Sum<TColumn>(TColumn column);
    /// <summary>
    /// <para>SUM(CASE WHEN exp THEN val ELSE 0 END)</para>
    /// 等价于<see cref="SqlFn.Sum{T}(bool, T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="exp"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    double Sum<TColumn>(bool exp, TColumn column);
    /// <summary>
    /// <para>AVG(val)</para>
    /// 等价于<see cref="SqlFn.Average{T}(T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    double Average<TColumn>(TColumn column);
    /// <summary>
    /// <para>AVG(CASE WHEN exp THEN val ELSE 0 END)</para>
    /// 等价于<see cref="SqlFn.Average{T}(bool, T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="exp"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    double Average<TColumn>(bool exp, TColumn column);
    TColumn Max<TColumn>(TColumn column);
    TColumn Max<TColumn>(bool exp, TColumn column);
    TColumn Min<TColumn>(TColumn column);
    TColumn Min<TColumn>(bool exp, TColumn column);
    /// <summary>
    /// 分组数据中拼接字符串
    /// <para>MySql -> GROUP_CONCAT</para>
    /// <para>Sqlite -> GROUP_CONCAT</para>
    /// <para>Oracle -> LISTAGG</para>
    /// <para>SqlServer 2017 -> STRING_AGG</para>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <returns></returns>
    IGroupJoinFn Join<TColumn>(TColumn? column);
    /// <summary>
    /// 等价于 <see cref="SqlFn.NullThen{T}(T, T)"/>
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    /// <param name="column"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    TColumn NullThen<TColumn>(TColumn column, TColumn value);
    /// <summary>
    /// 等价于 <see cref="SqlFn.Case{TReturn}()"/>
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    /// <returns></returns>
    ICaseFragment<TReturn> Case<TReturn>();

}
