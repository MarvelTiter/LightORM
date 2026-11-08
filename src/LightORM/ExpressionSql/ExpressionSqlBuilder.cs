using LightORM.Implements;
using LightORM.Options;
using System.Collections.Concurrent;

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

    /// <summary>
    /// 登记"从配置创建 provider"的委托（key 为 <see cref="DbBaseType.Name"/>，大小写不敏感）。
    /// <para>
    /// 由各方言的空参数 <c>UseXxx()</c> 调用。登记只是登记，provider 的创建发生在读取配置时。
    /// </para>
    /// </summary>
    public static IExpressionContextSetup RegisterDatabase(this IExpressionContextSetup setup, DbBaseType type, Func<DatabaseOptions, IDatabaseProvider> create)
    {
        if (setup is not IDatabaseRegistry registry)
        {
            throw new LightOrmException($"当前 IExpressionContextSetup 实现（{setup?.GetType().Name ?? "null"}）不支持配置驱动注册。");
        }

        registry.RegisterDatabase(type, create);
        return setup;
    }
}

internal partial class ExpressionOptionBuilder : IExpressionContextSetup, IDatabaseRegistry
{
    public bool ThrowExceptionWhileMethodResolveNotMap
    {
        get => ExpressionSqlOptions.Instance.Value.ThrowExceptionWhileMethodResolveNotMap;
        set => ExpressionSqlOptions.Instance.Value.ThrowExceptionWhileMethodResolveNotMap = value;
    }

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

    public IExpressionContextSetup UseInterceptor<T>(T interceptor) where T : AdoInterceptorBase
    {
        ExpressionSqlOptions.AddStateLessInterceptor(typeof(T), interceptor);
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

    public void RegisterDatabase(DbBaseType type, Func<DatabaseOptions, IDatabaseProvider> create)
    {
        ArgumentNullException.ThrowIfNull(type);
        LightOrmException.ThrowIfNull(create, "create 不能为 null");
        ExpressionSqlOptions.RegisterCreator(type.Name, create);
    }
}