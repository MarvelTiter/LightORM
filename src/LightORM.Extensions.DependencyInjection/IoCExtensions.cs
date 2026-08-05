using LightORM.ExpressionSql;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

public static class IoCExtensions
{
    internal static WeakReference<IServiceCollection>? WeakServices { get; set; }

    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options)
    {
        var builder = new ExpressionOptionBuilder
        {
        };
        WeakServices = new(services);
        options(builder);
        services.AddScoped(provider =>
        {
            var option = Build(provider);
            return option;
        });
        services.AddScoped(typeof(ILightOrmRepository<>), typeof(DefaultRepository<>));
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    private static ExpressionSqlOptions Build(IServiceProvider provider)
    {
        if (provider is not null)
        {
            var interceptor = provider.GetServices<IAdoInterceptor>();
            return new(interceptor);
        }
        return new();
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
