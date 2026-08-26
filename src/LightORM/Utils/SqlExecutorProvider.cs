using LightORM.Performances;
using System.Collections.Concurrent;

namespace LightORM.Utils;

internal class ConnectionFactory(ExpressionSqlOptions option)
{
    public DatabaseConnection GetDatabaseConnection(string key)
    {
        var provider = GetDbInfo(key, option);
        var pool = ConnectionPool.Pools.GetOrAdd(provider, p =>
        {
            return new ConnectionPool(() =>
            {
                var conn = p.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = p.MasterConnectionString;
                return conn;
            }, option.PoolSize);
        });
        var conn = pool.Get();
        return new DatabaseConnection(conn, provider,new(option.Interceptors));
    }

    public DatabaseConnection GetDatabaseConnection(IDatabaseProvider provider)
    {
        var pool = ConnectionPool.Pools.GetOrAdd(provider, p =>
        {
            return new ConnectionPool(() =>
            {
                var conn = p.DbProviderFactory.CreateConnection()!;
                conn.ConnectionString = p.MasterConnectionString;
                return conn;
            }, option.PoolSize);
        });
        var conn = pool.Get();
        return new DatabaseConnection(conn, provider, new(option.Interceptors));
    }

    public static IDatabaseProvider GetDbInfo(string key, ExpressionSqlOptions option)
    {
        return option.DatabaseProviders.TryGetValue(key, out var db) ? db : throw new ArgumentException($"{key} not register");
    }
}

[Obsolete]
internal class SqlExecutorProvider : IDisposable
{
    //public static ISqlExecutor GetExecutor(string key = ConstString.Main)
    //{
    //    var dbInfo = StaticCache<IDatabaseProvider>.Get(key) ?? throw new LightOrmException($"{key} not register");
    //    return new SqlExecutor.SqlExecutor(dbInfo, 5);
    //}

    public IDatabaseProvider GetDbInfo(string key)
    {
        return option.DatabaseProviders.TryGetValue(key, out var db) ? db : throw new ArgumentException($"{key} not register");
    }

    private readonly ConcurrentDictionary<string, ISqlExecutor> executors = [];
    private readonly ExpressionSqlOptions option;
    public SqlExecutorProvider(ExpressionSqlOptions option)
    {
        this.option = option;
    }

    public ConcurrentDictionary<string, ISqlExecutor> Executors => executors;

    public ISqlExecutor GetSqlExecutor(string key = ConstString.Main) => InternalCreator(key);

    private ISqlExecutor InternalCreator(string key)
    {
        return executors.GetOrAdd(key, k =>
        {
            var ado = new SqlExecutor.OrignalSqlExecutor(GetDbInfo(k), option.PoolSize, new AdoInterceptor(option.Interceptors), k);
            //if (useTrans)
            //{
            //    ado.BeginTran();
            //}
            return ado;
        });
    }

    #region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var item in executors.Values)
                {
                    item.Dispose();
                }
                executors.Clear();
                //foreach (var item in queryExecutors)
                //{
                //    item?.Dispose();
                //}
                //queryExecutors.Clear();
            }
            disposedValue = true;
        }
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
