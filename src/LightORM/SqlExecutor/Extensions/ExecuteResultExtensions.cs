using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightORM;

public static class ExecuteResultExtensions
{

    extension(ExecuteResult r)
    {
        public IEnumerable<T> ToList<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T>()
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = r.Ado.ExecuteReader(r.Sql, r.Param, r.CommandType);
                var des = reader.BuildDeserializer<T>();
                while (reader.Read())
                {
                    yield return des.Invoke(reader);
                }
            }
            finally
            {
                reader?.Close();
            }
        }

        public IEnumerable<dynamic> ToList()
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = r.Ado.ExecuteReader(r.Sql, r.Param, r.CommandType);
                var des = reader.DynamicDeserializer();
                while (reader.Read())
                {
                    yield return des.Invoke(reader);
                }
            }
            finally
            {
                reader?.Close();
            }
        }


        public T? Single<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T>()
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = r.Ado.ExecuteReader(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult);
                var des = reader.BuildDeserializer<T>();
                T? result = default;
                if (reader.Read())
                {
                    result = des.Invoke(reader);
                }
                return result;
            }
            finally
            {
                reader?.Close();
            }
        }

        public async Task<IList<T>> ToListAsync<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T>(CancellationToken cancellationToken = default)
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = await r.Ado.ExecuteReaderAsync(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleResult, cancellationToken);
                var des = reader.BuildDeserializer<T>();
                List<T> list = [];
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    list.Add(des.Invoke(reader));
                }
                return list;
            }
            finally
            {
#if NET6_0_OR_GREATER
                if (reader is not null)
                {
                    await reader.CloseAsync();
                }
#else
            reader?.Close();
#endif
            }
        }

        public async Task<IList<dynamic>> ToListAsync(CancellationToken cancellationToken = default)
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = await r.Ado.ExecuteReaderAsync(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleResult, cancellationToken);
                var des = reader.DynamicDeserializer();
                List<object> list = [];
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    list.Add(des.Invoke(reader));
                }
                return list;
            }
            finally
            {
#if NET6_0_OR_GREATER
                if (reader is not null)
                {
                    await reader.CloseAsync();
                }
#else
            reader?.Close();
#endif
            }
        }

        public async IAsyncEnumerable<T> ToAsyncList<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = await r.Ado.ExecuteReaderAsync(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleResult, cancellationToken);
                var des = reader.BuildDeserializer<T>();
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    yield return des.Invoke(reader);
                }
            }
            finally
            {
#if NET6_0_OR_GREATER
                if (reader is not null)
                {
                    await reader.CloseAsync();
                }
#else
            reader?.Close();
#endif
            }
        }

        public async IAsyncEnumerable<dynamic> ToAsyncList([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = await r.Ado.ExecuteReaderAsync(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleResult, cancellationToken);
                var des = reader.DynamicDeserializer();
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    yield return des.Invoke(reader);
                }
            }
            finally
            {
#if NET6_0_OR_GREATER
                if (reader is not null)
                {
                    await reader.CloseAsync();
                }
#else
            reader?.Close();
#endif
            }
        }

        public async Task<T?> QuerySingleAsync<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T>(CancellationToken cancellationToken = default)
        {
            DbDataReader? reader = null;
            try
            {
                if (r.Trans != null)
                {
                    r.Ado.UseExternalTransaction(r.Trans);
                }
                reader = await r.Ado.ExecuteReaderAsync(r.Sql, r.Param, r.CommandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, cancellationToken);
                var des = reader.BuildDeserializer<T>();
                T? result = default;
                if (await reader.ReadAsync(cancellationToken))
                {
                    result = des.Invoke(reader);
                }
                return result;
            }
            finally
            {
#if NET6_0_OR_GREATER
                if (reader is not null)
                {
                    await reader.CloseAsync();
                }
#else
            reader?.Close();
#endif
            }
        }
    }
}
