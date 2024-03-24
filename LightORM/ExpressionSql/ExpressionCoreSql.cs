using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using LightORM.Cache;
using LightORM.Providers;
using LightORM.Utils;
namespace LightORM.ExpressionSql;

public class ExpressionCoreSql : IExpressionContext, IDisposable
{
    public Microsoft.Extensions.Logging.ILogger<IExpressionContext>? Logger { get; set; }
    // private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
    private readonly ConcurrentDictionary<string, ISqlExecutor> executors = [];
    internal readonly SqlAopProvider Aop;
    //private IAdo ado;
    private readonly List<ISqlExecutor> queryExecutors = [];
    public ISqlExecutor Ado
    {
        get
        {
            var ado = new SqlExecutor.SqlExecutor(GetDbInfo(CurrentKey));
            queryExecutors.Add(ado);
            //System.Diagnostics.Debug.WriteLine($"创建 sqlexecutor [{directlyUsed.Count}]");
            if (useTrans)
            {
                ado.BeginTran();
            }
            return ado;
        }
    }

    public ExpressionCoreSql(ExpressionSqlOptions option)
    {
        Debug.WriteLine($"创建ExpressionCoreSql：{DateTime.Now}");
        this.Aop = option.Aop;
    }

    internal ISqlExecutor GetExecutor(string key)
    {
        return executors.GetOrAdd(key, k =>
        {
            var ado = new SqlExecutor.SqlExecutor(GetDbInfo(key));
            //TODO AOPlog
            ado.DbLog = Aop.DbLog;
            //TODO Trans setting
            if (useTrans)
            {
                ado.BeginTran();
            }
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

    public IExpSelect<T> Select<T>(Expression<Func<T, object>> exp) => CreateSelectProvider<T>(exp.Body);

    Providers.Select.SelectProvider1<T> CreateSelectProvider<T>(Expression body) => new(body, Ado);

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);

    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(GetExecutor(CurrentKey), entity);
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities) => new(GetExecutor(CurrentKey), entities);


    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);

    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(GetExecutor(CurrentKey), entity);
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities) => new(GetExecutor(CurrentKey), entities);

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);

    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(GetExecutor(CurrentKey), entity);
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities) => new(GetExecutor(CurrentKey), entities);
    bool useTrans;
    public void BeginTran()
    {
        useTrans = true;
        executors.ForEach(ado =>
        {
            try { ado.BeginTran(); } catch { }
        });
    }

    public async Task BeginTranAsync()
    {
        useTrans = true;
        await executors.ForEachAsync(async ado =>
         {
             try
             {
                 await ado.BeginTranAsync();
             }
             catch { }
         });
    }

    public void CommitTran()
    {
        useTrans = false;
        executors.ForEach(ado =>
        {
            try { ado.CommitTran(); } catch { }
        });
    }

    public async Task CommitTranAsync()
    {
        useTrans = false;
        await executors.ForEachAsync(async ado =>
        {
            try
            {
                await ado.CommitTranAsync();
            }
            catch { }
        });
    }

    public void RollbackTran()
    {
        useTrans = false;
        executors.ForEach(ado =>
        {
            try { ado.RollbackTran(); } catch { }
        });
    }

    public async Task RollbackTranAsync()
    {
        useTrans = false;
        await executors.ForEachAsync(async ado =>
        {
            try
            {
                await ado.RollbackTranAsync();
            }
            catch { }
        });
    }


    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Debug.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
                foreach (var item in executors.Values)
                {
                    item.Dispose();
                }
                executors.Clear();
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