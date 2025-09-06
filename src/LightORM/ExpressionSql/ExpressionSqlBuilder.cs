using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace LightORM;

public class SqlAopProvider
{
    public Action<string, object?>? DbLog { get; set; }
}

public static class IExpressionContextSetupEx
{
    public static IExpressionContextSetup SetDatabase(this IExpressionContextSetup setup, IDatabaseProvider provider) => setup.SetDatabase(null, provider.DbBaseType, provider);
    public static IExpressionContextSetup SetDatabase(this IExpressionContextSetup setup, DbBaseType dbBaseType, IDatabaseProvider provider) => setup.SetDatabase(null, dbBaseType, provider);

    public static IExpressionContextSetup SetTableContext<T>(this IExpressionContextSetup setup)
        where T : ITableContext, new()
    {
        setup.SetTableContext(new T());
        return setup;
    }

    public static IExpressionContextSetup SetTableContext(this IExpressionContextSetup setup, Func<ITableContext> createContext)
    {
        setup.SetTableContext(createContext.Invoke());
        return setup;
    }
}

internal class ExpressionSqlOptions
{
    private int poolSize = Environment.ProcessorCount * 4;
    public int PoolSize => poolSize;
    private string? defaultDbKey;
    public string DefaultDbKey
    {
        get
        {
            var d = defaultDbKey?? ConstString.Main;
            if (!DatabaseProviders.ContainsKey(d))
            {
                throw new KeyNotFoundException($"数据库 '{d}' 未注册. 使用'SetDefault'设置默认值.");
            }
            return d;
        }
    }
    public static Lazy<ExpressionSqlOptions> Instance { get; }
    public bool UseParameterized { get; set; } = true;
    public IServiceProvider? Services { get; set; }
    public ICollection<IAdoInterceptor> Interceptors => TypedInterceptors.Values;
    public ConcurrentDictionary<string, IDatabaseProvider> DatabaseProviders { get; }
    public ConcurrentDictionary<string, ICustomDatabase> CustomDatabases { get; }
    public ConcurrentDictionary<string, IDatabaseTableHandler> DatabaseHandlers { get; }
    public ConcurrentDictionary<Type, IAdoInterceptor> TypedInterceptors { get; }
    public TableGenerateOption TableGenOption { get; set; }
    static ExpressionSqlOptions()
    {
        Instance = new(() => new());
    }

    private ExpressionSqlOptions()
    {
        DatabaseProviders = [];
        CustomDatabases = [];
        DatabaseHandlers = [];
        TypedInterceptors = [];
        TableGenOption = new TableGenerateOption();
    }

    public void SetConnectionPoolSize(int poolSize)
    {
        this.poolSize = poolSize;
    }

    public void SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        var k = key ?? ConstString.Main;
        if (!DatabaseProviders.TryGetValue(k, out _))
        {
            DatabaseProviders.TryAdd(k, provider);
        }

        if (!CustomDatabases.TryGetValue(dbBaseType.Name, out _))
        {
            CustomDatabases.TryAdd(dbBaseType.Name, provider.CustomDatabase);
        }

        if (!DatabaseHandlers.TryGetValue(dbBaseType.Name, out _))
        {
            DatabaseHandlers.TryAdd(dbBaseType.Name, provider.DbHandler);
        }
    }

    public void SetTableContext(ITableContext context)
    {
        TableContext.StaticContext = context;
    }

    public void SetDefaultDatabase(string key)
    {
        defaultDbKey = key;
    }

    public void AddInterceptor(Type interceptorType, IAdoInterceptor? interceptor)
    {
        if (!TypedInterceptors.TryGetValue(interceptorType, out _) && interceptor is not null)
        {
            TypedInterceptors.TryAdd(interceptorType, interceptor);
        }
    }
}

internal partial class ExpressionOptionBuilder : IExpressionContextSetup
{
    private readonly ExpressionSqlOptions option = ExpressionSqlOptions.Instance.Value;
    private readonly List<Type> interceptorTypes = [];

    public WeakReference<IServiceCollection>? WeakServices { get; set; }
    public IExpressionContextSetup SetDefault(string key)
    {
        option.SetDefaultDatabase(key);
        return this;
    }
    public IExpressionContextSetup SetUseParameterized(bool use)
    {
        option.UseParameterized = use;
        return this;
    }

    public IExpressionContextSetup SetConnectionPoolSize(int poolSize)
    {
        option.SetConnectionPoolSize(poolSize);
        return this;
    }

    public IExpressionContextSetup SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        option.SetDatabase(key, dbBaseType, provider);
        return this;
    }

    public IExpressionContextSetup SetTableContext(ITableContext context)
    {
        option.SetTableContext(context);
        return this;
    }

    public IExpressionContextSetup UseInterceptor<T>() where T : AdoInterceptorBase
    {
        interceptorTypes.Add(typeof(T));
        if (WeakServices?.TryGetTarget(out var services) == true)
        {
            services?.AddScoped<T>();
        }

        return this;
    }

    public IExpressionContextSetup UseInitial<T>() where T : DbInitialContext, new()
    {
        throw new NotImplementedException();
    }
    public IExpressionContextSetup TableConfiguration(Action<TableGenerateOption> action)
    {
        action.Invoke(option.TableGenOption);
        return this;
    }
    public ExpressionSqlOptions Build(IServiceProvider? provider)
    {
        if (provider is not null)
        {
            foreach (var interceptorType in interceptorTypes)
            {
                var interceptor = provider.GetService(interceptorType) as IAdoInterceptor;
                option.AddInterceptor(interceptorType, interceptor);
            }
        }
        else
        {
            foreach (var item in interceptorTypes)
            {
                var nonParameterCtor = item.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
                if (nonParameterCtor?.Invoke([]) is IAdoInterceptor obj) option.Interceptors.Add(obj);
            }
        }

        return option;
    }

    
}