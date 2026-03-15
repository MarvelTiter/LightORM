using LightORM.Implements;

namespace LightORM.Interfaces;

public interface IExpressionContextSetup
{
    bool ThrowExceptionWhileMethodResolveNotMap { get; set; }
    IExpressionContextSetup SetDefault(string key);
    IExpressionContextSetup SetUseParameterized(bool use);
    IExpressionContextSetup SetConnectionPoolSize(int poolSize);
    IExpressionContextSetup SetEnableExpressionCache(bool enable);
    IExpressionContextSetup SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider);
    IExpressionContextSetup SetTableContext(ITableContext context);
    IExpressionContextSetup UseInterceptor<T>() where T : AdoInterceptorBase;
    IExpressionContextSetup UseInitial<T>() where T : DbInitialContext, new();

    IExpressionContextSetup ConfigJsonHandler<T>() where T : ILightJsonHelper, new();
    IExpressionContextSetup ConfigJsonHandler(Action<IJsonConfiguration> config);

    [Obsolete("使用IDbOption配置")]
    IExpressionContextSetup TableConfiguration(Action<TableOptions> action);
    // IExpressionContextSetup UseIdentifierQuote(bool value = true);
}

public interface IJsonConfiguration
{
    Func<object, string>? Serializer { get; set; }
    Func<string, object>? Deserializer { get; set; }
    Func<byte[], object>? DeserializerBytes { get; set; }
}
