using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

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

internal partial class ExpressionSqlOptions
{
    // 共享不可变的数据
    private readonly static ConcurrentDictionary<string, IDatabaseProvider> databaseProviders = [];
    private readonly static ConcurrentDictionary<string, IDatabaseAdapter> customDatabases = [];
    private readonly static ConcurrentDictionary<string, IDatabaseTableHandler> databaseHandlers = [];
    private readonly static ConcurrentDictionary<Type, IAdoInterceptor> stateLessInterceptors = [];
    private static int poolSize = Environment.ProcessorCount * 4;
    private static int objectPoolSize = Environment.ProcessorCount * 8;
    private static ILightJsonHelper? specificInstance;

    private static string? defaultDbKey;
    private static bool useParameterized = true;
    private static bool enableExpressionCache = true;
    static ExpressionSqlOptions()
    {
        Instance = new(() => new ExpressionSqlOptions());
    }

    public static void SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        var k = key ?? ConstString.Main;
        if (!databaseProviders.TryGetValue(k, out _))
        {
            databaseProviders.TryAdd(k, provider);
        }

        if (!customDatabases.TryGetValue(dbBaseType.Name, out _))
        {
            customDatabases.TryAdd(dbBaseType.Name, provider.DatabaseAdapter);
        }

        if (!databaseHandlers.TryGetValue(dbBaseType.Name, out _))
        {
            databaseHandlers.TryAdd(dbBaseType.Name, provider.DbHandler);
        }
    }

    public static void AddStateLessInterceptor(Type interceptorType, IAdoInterceptor? interceptor)
    {
        if (!stateLessInterceptors.TryGetValue(interceptorType, out _) && interceptor is not null)
        {
            stateLessInterceptors.TryAdd(interceptorType, interceptor);
        }
    }

    public static void SetTableContext(ITableContext context)
    {
        TableContext.StaticContext = context;
    }

    public static void SetDefaultDatabase(string key)
    {
        defaultDbKey = key;
    }
    public static void SetConnectionPoolSize(int size)
    {
        poolSize = size;
    }
    public static void SetUseParameterized(bool use)
    {
        useParameterized = use;
    }
    public static void SetEnableExpressionCache(bool enable)
    {
        enableExpressionCache = enable;
    }
    public static void SetSpecificJsonHandler(ILightJsonHelper instance)
    {
        specificInstance = instance;
    }

}

internal partial class ExpressionSqlOptions : IJsonConfiguration
{
    public static Lazy<ExpressionSqlOptions> Instance { get; }
    public int PoolSize => poolSize;
    public int InternalObjectPoolSize => objectPoolSize;
    public bool EnableExpressionCache => enableExpressionCache;
    public bool ThrowExceptionWhileMethodResolveNotMap { get; set; }
    public string DefaultDbKey
    {
        get
        {
            var d = defaultDbKey ?? ConstString.Main;
            if (!DatabaseProviders.ContainsKey(d))
            {
                throw new KeyNotFoundException($"数据库 '{d}' 未注册. 使用'SetDefault'设置默认值.");
            }
            return d;
        }
    }
    public bool UseParameterized => useParameterized;
    public IServiceProvider? Services { get; set; }
    private readonly ICollection<IAdoInterceptor> allInterceptors;
    public ICollection<IAdoInterceptor> Interceptors => allInterceptors;
    public ConcurrentDictionary<string, IDatabaseProvider> DatabaseProviders { get; }
    public ConcurrentDictionary<string, IDatabaseAdapter> CustomDatabases { get; }
    public ConcurrentDictionary<string, IDatabaseTableHandler> DatabaseHandlers { get; }
    public Func<object, string>? Serializer { get; set; }
    public Func<string, object>? Deserializer { get; set; }
    public Func<byte[], object>? DeserializerBytes { get; set; }

    public ExpressionSqlOptions()
    {
        DatabaseProviders = databaseProviders;
        CustomDatabases = customDatabases;
        DatabaseHandlers = databaseHandlers;
        allInterceptors = stateLessInterceptors.Values;
    }

    public ILightJsonHelper GetJsonHandler()
    {
        if (specificInstance is not null) return specificInstance;
        if (Serializer is null || Deserializer is null || DeserializerBytes is null)
        {
            throw new LightOrmException("未配置ILightJsonHelper实例的情况下，Serializer和Deserializer和DeserializerBytes不能为null");
        }
        return specificInstance ??= new DefaultJsonHelper(Serializer, Deserializer, DeserializerBytes);
    }

    public ExpressionSqlOptions(IEnumerable<IAdoInterceptor> interceptors) : this()
    {
        if (interceptors.Any())
        {
            allInterceptors = [.. interceptors, .. stateLessInterceptors.Values];
        }
        else
        {
            allInterceptors = stateLessInterceptors.Values;
        }
    }
}

internal partial class ExpressionOptionBuilder : IExpressionContextSetup
{
    public bool ThrowExceptionWhileMethodResolveNotMap
    {
        get => ExpressionSqlOptions.Instance.Value.ThrowExceptionWhileMethodResolveNotMap;
        set => ExpressionSqlOptions.Instance.Value.ThrowExceptionWhileMethodResolveNotMap = value;
    }
    public WeakReference<IServiceCollection>? WeakServices { get; set; }
    public IExpressionContextSetup SetDefault(string key)
    {
        ExpressionSqlOptions.SetDefaultDatabase(key);
        return this;
    }
    public IExpressionContextSetup SetUseParameterized(bool use)
    {
        ExpressionSqlOptions.SetUseParameterized(use);
        return this;
    }

    public IExpressionContextSetup SetConnectionPoolSize(int poolSize)
    {
        ExpressionSqlOptions.SetConnectionPoolSize(poolSize);
        return this;
    }

    public IExpressionContextSetup SetEnableExpressionCache(bool enable)
    {
        ExpressionSqlOptions.SetEnableExpressionCache(enable);
        return this;
    }

    public IExpressionContextSetup SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        ExpressionSqlOptions.SetDatabase(key, dbBaseType, provider);
        return this;
    }

    public IExpressionContextSetup SetTableContext(ITableContext context)
    {
        ExpressionSqlOptions.SetTableContext(context);
        return this;
    }
#if NET8_0_OR_GREATER
    public IExpressionContextSetup UseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
#else
    public IExpressionContextSetup UseInterceptor<T>()
#endif
        where T : AdoInterceptorBase
    {
        var item = typeof(T);
        var nonParameterCtor = item.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
        if (nonParameterCtor != null)
        {
            // 如果有无参构造器，当作无状态的处理
            if (nonParameterCtor?.Invoke([]) is IAdoInterceptor obj)
                ExpressionSqlOptions.AddStateLessInterceptor(item, obj);
        }
        else if (WeakServices?.TryGetTarget(out var services) == true)
        {
            services?.AddScoped(typeof(IAdoInterceptor), item);
        }
        return this;
    }

    public IExpressionContextSetup ConfigJsonHandler<T>() where T : ILightJsonHelper, new()
    {
        ExpressionSqlOptions.SetSpecificJsonHandler(new T());
        return this;
    }
    public IExpressionContextSetup ConfigJsonHandler(Action<IJsonConfiguration> config)
    {
        config.Invoke(ExpressionSqlOptions.Instance.Value);
        return this;
    }
    public IExpressionContextSetup UseInitial<T>() where T : DbInitialContext, new()
    {
        throw new NotImplementedException();
    }
    public IExpressionContextSetup TableConfiguration(Action<TableOptions> action)
    {
        throw new NotSupportedException();
    }

    public ExpressionSqlOptions Build(IServiceProvider? provider)
    {
        if (provider is not null)
        {
            var interceptor = provider.GetServices<IAdoInterceptor>();
            return new(interceptor);
        }
        return new();
    }


}