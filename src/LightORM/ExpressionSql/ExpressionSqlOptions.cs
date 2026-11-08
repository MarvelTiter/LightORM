using LightORM.Implements;
using System.Collections.Concurrent;

namespace LightORM;

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
    public Func<string, Type, object>? Deserializer { get; set; }
    public Func<byte[], Type, object>? DeserializerBytes { get; set; }

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
