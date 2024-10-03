namespace LightORM.Interfaces.ExpSql;

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
    int Count(bool exp);
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
