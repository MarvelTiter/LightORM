using LightORM.ExpressionSql.Interface;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LightORM.Cache;

namespace LightORM.ExpressionSql;

#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
internal partial class ExpressionCoreSql
{
    public Microsoft.Extensions.Logging.ILogger<IExpressionContext>? Logger { get; set; }
}
#endif
internal partial class ExpressionCoreSql : IExpressionContext, IDisposable
{
    // private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
    private readonly ConcurrentDictionary<string, ISqlExecutor> executors = [];
    internal readonly SqlExecuteLife Life;
    //private IAdo ado;

    public ISqlExecutor Ado => GetExecutor(CurrentKey);

    internal ExpressionCoreSql(SqlExecuteLife life)
    {
        this.Life = life;
        this.Life.Core = this;
        //this.ado = ado ?? new AdoImpl(dbFactories);
    }

    internal ISqlExecutor GetExecutor(string key)
    {
        return executors.GetOrAdd(key, k =>
        {
            var ado = new SqlExecutor.SqlExecutor(GetDbInfo(key));
            //TODO AOPlog
            //TODO Trans setting
            return ado;
        });
    }

    internal static DbConnectInfo GetDbInfo(string key)
    {
        return StaticCache<DbConnectInfo>.Get(key) ?? throw new ArgumentException($"{key} not register");
    }

    private readonly string MainDb = ConstString.Main;
    private string? _dbKey = null;

    internal string CurrentKey
    {
        get
        {
            var k = _dbKey ?? MainDb;
            _dbKey = null;
            return k;
        }
    }

    public IExpressionContext SwitchDatabase(string key)
    {
        _dbKey = key;
        return this;
    }

    public IExpSelect<T> Select<T>() => Select<T>(t => t!);

    public IExpSelect<T> Select<T>(Expression<Func<T, object>> exp) => CreateSelectProvider<T>(CurrentKey, exp.Body);

    IExpSelect<T> CreateSelectProvider<T>(string key, Expression body) =>
        new LightORM.Providers.Select.SelectProvider1<T>(body, GetExecutor(key));

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>(CurrentKey);
    public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(CurrentKey, entity);
    public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(CurrentKey, entities);

    IExpInsert<T> CreateInsertProvider<T>(string key, T? entity = default) =>
        new LightORM.Providers.InsertProvider<T>(GetExecutor(key), entity);
    IExpInsert<T> CreateInsertProvider<T>(string key, IEnumerable<T> entities) =>
        new LightORM.Providers.InsertProvider<T>(GetExecutor(key), entities);


    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>(CurrentKey);
    public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(CurrentKey, entity);
    public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(CurrentKey, entities);

    IExpUpdate<T> CreateUpdateProvider<T>(string key, T? entity = default) =>
        new LightORM.Providers.UpdateProvider<T>(GetExecutor(key), entity);
    IExpUpdate<T> CreateUpdateProvider<T>(string key, IEnumerable<T> entities) =>
        new LightORM.Providers.UpdateProvider<T>(GetExecutor(key), entities);

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>(CurrentKey);
    public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(CurrentKey, entity);
    public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(CurrentKey, entities);

    IExpDelete<T> CreateDeleteProvider<T>(string key, T? entity = default) =>
        new LightORM.Providers.DeleteProvider<T>(GetExecutor(key), entity);
    IExpDelete<T> CreateDeleteProvider<T>(string key, IEnumerable<T> entities) =>
        new LightORM.Providers.DeleteProvider<T>(GetExecutor(key), entities);

    public void BeginTran()
    {
        throw new NotImplementedException();
    }

    public Task BeginTranAsync()
    {
        throw new NotImplementedException();
    }

    public void CommitTran()
    {
        throw new NotImplementedException();
    }

    public Task CommitTranAsync()
    {
        throw new NotImplementedException();
    }

    public void RollbackTran()
    {
        throw new NotImplementedException();
    }

    public Task RollbackTranAsync()
    {
        throw new NotImplementedException();
    }


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var item in executors)
                {
                    item.Value.Dispose();
                }
            }

            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}