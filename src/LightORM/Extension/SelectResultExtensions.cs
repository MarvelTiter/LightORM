using System.Diagnostics.CodeAnalysis;
using System.Threading;


namespace LightORM;

internal static class SelectResultExtensions
{
    public static IEnumerable<TReturn> InternalToList<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return [];
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Execute(sql, parameters).ToList<TReturn>();
    }

    public static TReturn? InternalSingle<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>(this IExpSelect select)
    {
        if (select.IsSubQuery) return default;
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Execute(sql, parameters).Single<TReturn>();
    }

    public static Task<IList<TReturn>> InternalToListAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Execute(sql, parameters).ToListAsync<TReturn>(cancellationToken);
    }

    public static IAsyncEnumerable<TReturn> InternalToEnumerableAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Execute(sql, parameters).ToAsyncList<TReturn>(cancellationToken);
    }

    public static Task<TReturn?> InternalSingleAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    TReturn>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.Execute(sql, parameters).SingleAsync<TReturn>(cancellationToken);
    }

    public static T? ExecuteScalar<T>(this IExpSelect select)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteScalar(sql, parameters).As<T>();
    }

    public static async Task<T?> ExecuteScalarAsync<T>(this IExpSelect select, CancellationToken cancellationToken = default)
    {
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
        var parameters = select.SqlBuilder.DbParameters;
        return (await select.Executor.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken)).As<T>();
    }
}

