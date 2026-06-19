using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LightORM;

public readonly struct ExecuteResult<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
TParameter>(ISqlExecutor ado, string sql, TParameter? param, DbTransaction? trans, CommandType commandType = CommandType.Text)
{
    internal ISqlExecutor Ado { get; } = ado;
    internal string Sql { get; } = sql;
    internal TParameter? Param { get; } = param;
    internal DbTransaction? Trans { get; } = trans;
    internal CommandType CommandType { get; } = commandType;

    public IEnumerable<T> ToList<
#if NET8_0_OR_GREATER
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>()
    {
        DbDataReader? reader = null;
        try
        {
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = Ado.ExecuteReader(Sql, Param, CommandType);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = Ado.ExecuteReader(Sql, Param, CommandType);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = Ado.ExecuteReader(Sql, Param, CommandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = await Ado.ExecuteReaderAsync(Sql, Param, CommandType, CommandBehavior.SingleResult, cancellationToken);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = await Ado.ExecuteReaderAsync(Sql, Param, CommandType, CommandBehavior.SingleResult, cancellationToken);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = await Ado.ExecuteReaderAsync(Sql, Param, CommandType, CommandBehavior.SingleResult, cancellationToken);
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
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = await Ado.ExecuteReaderAsync(Sql, Param, CommandType, CommandBehavior.SingleResult, cancellationToken);
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

    public async Task<T?> SingleAsync<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(CancellationToken cancellationToken = default)
    {
        DbDataReader? reader = null;
        try
        {
            if (Trans != null)
            {
                Ado.UseExternalTransaction(Trans);
            }
            reader = await Ado.ExecuteReaderAsync(Sql, Param, CommandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, cancellationToken);
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

public readonly struct ExecuteResult(ISqlExecutor ado, string sql, DbTransaction? trans, CommandType commandType = CommandType.Text)
{
    internal ISqlExecutor Ado { get; } = ado;
    internal string Sql { get; } = sql;
    internal NullDbParameter Param { get; } = NullDbParameter.Instance;
    internal DbTransaction? Trans { get; } = trans;
    internal CommandType CommandType { get; } = commandType;
}