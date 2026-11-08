using LightORM.ExpressionSql;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Options;
using LightORM.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

public static class IoCExtensions
{
    internal static WeakReference<IServiceCollection>? WeakServices { get; set; }

    /// <summary>
    /// 注册 LightORM。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="options">代码方式的配置（优先级低于配置文件：配置文件里的同 key 条目会覆盖它）。</param>
    /// <param name="configuration">
    /// 提供 <c>LightOrmDatabases</c> 节的配置。传入后会：
    /// ① 在首次解析 <see cref="IExpressionContext"/> 时按配置注册数据库；
    /// ② 订阅文件变更，配置变化后<b>覆盖/新增/移除</b>对应的库（连接串改动会生效）。
    /// </param>
    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options, IConfiguration? configuration = null)
    {
        var builder = new ExpressionOptionBuilder();
        WeakServices = new(services);
        options(builder);

        if (configuration is not null)
        {
            services.TryAddSingleton<IConfiguration>(configuration);
            // AOT 友好的手写绑定（BindingExtensions.cs）。用 lambda 而不是 method group，
            // 这样 DatabaseList 类型变化时是编译期错误而不是运行期 InvalidCastException。
            services.Configure<DatabaseList>(list => configuration.Bind_DatabaseList(list));
            // IOptionsMonitor 依赖 change token 才会在文件变更时重算。
            // 显式注册，不要靠"再 Configure 一次"的副作用来拿 token。
            services.AddSingleton<IOptionsChangeTokenSource<DatabaseList>>(
                new ConfigurationChangeTokenSource<DatabaseList>(configuration));
            // 单例：首次构造时做初始加载并订阅变更，容器 dispose 时退订。
            services.AddSingleton<DatabaseListReloader>();
        }

        services.AddScoped(Build);
        services.AddScoped(typeof(ILightOrmRepository<>), typeof(DefaultRepository<>));
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    private static ExpressionSqlOptions Build(IServiceProvider provider)
    {
        // 触发单例构造：初始加载 + 订阅变更都在 DatabaseListReloader 里完成。
        // 没传 configuration 时它没被注册，GetService 返回 null，行为与以前一致。
        _ = provider.GetService<DatabaseListReloader>();
        return new(provider.GetServices<IAdoInterceptor>());
    }
}

public static class ExtenIExpressionContextSetup
{
    extension(IExpressionContextSetup setup)
    {
#if NET8_0_OR_GREATER
    public IExpressionContextSetup UseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
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
            else if (IoCExtensions.WeakServices?.TryGetTarget(out var services) == true)
            {
                services?.AddScoped(typeof(IAdoInterceptor), item);
            }

            return setup;
        }


    }
}
