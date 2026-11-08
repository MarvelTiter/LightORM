using LightORM.Implements;
using LightORM.Options;
using System.Collections.Concurrent;

namespace LightORM;

internal partial class ExpressionSqlOptions
{
    // 共享不可变的数据
    private readonly static ConcurrentDictionary<string, IDatabaseProvider> databaseProviders = [];
    private readonly static ConcurrentDictionary<string, IDatabaseAdapter> customDatabases = new(StringComparer.OrdinalIgnoreCase);
    private readonly static ConcurrentDictionary<string, IDatabaseTableHandler> databaseHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly static ConcurrentDictionary<Type, IAdoInterceptor> stateLessInterceptors = [];
    private readonly static ConcurrentDictionary<string, Func<DatabaseOptions, IDatabaseProvider>> databaseCreators = new(StringComparer.OrdinalIgnoreCase);
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

    public static void RegisterCreator(string name, Func<DatabaseOptions, IDatabaseProvider> action)
    {
        _ = databaseCreators.GetOrAdd(name, action);
    }

    /// <summary>已登记的方言名，用于配置写错时给出可读的错误信息。</summary>
    internal static IEnumerable<string> CreatorNames => databaseCreators.Keys;

    public static void SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        var k = key ?? ConstString.Main;
        _ = databaseProviders.TryAdd(k, provider);
        _ = customDatabases.TryAdd(dbBaseType.Name, provider.DatabaseAdapter);
        _ = databaseHandlers.TryAdd(dbBaseType.Name, provider.DbHandler);
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
        AOTSupported = true;
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

    internal static bool TryGetCreator(string? type, out Func<DatabaseOptions, IDatabaseProvider>? creator)
    {
        creator = null;
        return !string.IsNullOrWhiteSpace(type) && databaseCreators.TryGetValue(type!, out creator);
    }

    /// <summary>
    /// 应用一个来自配置的数据库实例。
    /// <para>
    /// <paramref name="replace"/> = true 时强制覆盖同 key 的 provider，并让 adapter / handler 跟随新的 provider；
    /// 否则保持"先注册者胜"的旧语义。
    /// </para>
    /// <para>
    /// adapter / handler 按 <see cref="DbBaseType.Name"/> 只存一份，所以后注册的 provider 如果方言相同，
    /// 必须覆盖才能让它自己的能力位（版本探测结果、标识符引用开关等）生效。
    /// </para>
    /// </summary>
    internal static void ApplyDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider, bool replace)
    {
        ArgumentNullException.ThrowIfNull(dbBaseType);
        LightOrmException.ThrowIfNull(provider, "provider 不能为 null");

        var k = key ?? ConstString.Main;
        if (replace)
        {
            databaseProviders[k] = provider;
            customDatabases[dbBaseType.Name] = provider.DatabaseAdapter;
            databaseHandlers[dbBaseType.Name] = provider.DbHandler;
            return;
        }

        _ = databaseProviders.TryAdd(k, provider);
        _ = customDatabases.TryAdd(dbBaseType.Name, provider.DatabaseAdapter);
        _ = databaseHandlers.TryAdd(dbBaseType.Name, provider.DbHandler);
    }

    /// <summary>
    /// 移除一个数据库实例；当没有其它实例复用同一方言时，一并移除它的 adapter / handler。
    /// </summary>
    internal static bool RemoveDatabase(string key)
    {
        if (!databaseProviders.TryRemove(key, out var provider))
        {
            return false;
        }

        var typeName = provider.DbBaseType.Name;
        foreach (var item in databaseProviders.Values)
        {
            if (string.Equals(item.DbBaseType.Name, typeName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        _ = customDatabases.TryRemove(typeName, out _);
        _ = databaseHandlers.TryRemove(typeName, out _);
        return true;
    }
}
