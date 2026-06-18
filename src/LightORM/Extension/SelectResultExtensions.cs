using System.Diagnostics.CodeAnalysis;
using System.Threading;


namespace LightORM;

public static class SelectResultExtensions
{
    extension(IExpSelect select)
    {
        public IEnumerable<TReturn> InternalToList<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TReturn>()
        {
            if (select.IsSubQuery) return [];
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.Execute(sql, parameters).ToList<TReturn>();
        }

        public TReturn? InternalSingle<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TReturn>()
        {
            if (select.IsSubQuery) return default;
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.Execute(sql, parameters).Single<TReturn>();
        }

        public Task<IList<TReturn>> InternalToListAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TReturn>(CancellationToken cancellationToken = default)
        {
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.Execute(sql, parameters).ToListAsync<TReturn>(cancellationToken);
        }

        public IAsyncEnumerable<TReturn> InternalToEnumerableAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TReturn>(CancellationToken cancellationToken = default)
        {
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.Execute(sql, parameters).ToAsyncList<TReturn>(cancellationToken);
        }

        public Task<TReturn?> InternalSingleAsync<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TReturn>(CancellationToken cancellationToken = default)
        {
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.Execute(sql, parameters).SingleAsync<TReturn>(cancellationToken);
        }

        public T? ExecuteScalar<T>()
        {
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteScalar(sql, parameters).As<T>();
        }

        public async Task<T?> ExecuteScalarAsync<T>(CancellationToken cancellationToken = default)
        {
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return (await select.Executor.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken)).As<T>();
        }
    }
}