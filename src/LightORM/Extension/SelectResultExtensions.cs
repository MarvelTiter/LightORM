namespace LightORM;

internal static class SelectResultExtensions
{
    public static IEnumerable<TReturn> ToList<TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return [];
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Query<TReturn>(sql, parameters);
    }

    public static TReturn? Single<TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return default;
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QuerySingle<TReturn>(sql, parameters);
    }

    public static Task<IList<TReturn>> ToListAsync<TReturn>(this IExpSelect select)
    {
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QueryAsync<TReturn>(sql, parameters);
    }
    public static Task<TReturn?> SingleAsync<TReturn>(this IExpSelect select)
    {
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QuerySingleAsync<TReturn>(sql, parameters);
    }
}

