using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM;

internal static class SelectResultExtensions
{
    public static IEnumerable<TReturn> InternalToList<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return [];
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Query<TReturn>(sql, parameters);
    }

    public static TReturn? InternalSingle<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return default;
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QuerySingle<TReturn>(sql, parameters);
    }

    public static Task<IList<TReturn>> InternalToListAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QueryListAsync<TReturn>(sql, parameters, cancellationToken: cancellationToken);
    }

    public static IAsyncEnumerable<TReturn> InternalToEnumerableAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QueryAsync<TReturn>(sql, parameters, cancellationToken: cancellationToken);
    }

    public static Task<TReturn?> InternalSingleAsync<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.QuerySingleAsync<TReturn>(sql, parameters, cancellationToken: cancellationToken);
    }
}

