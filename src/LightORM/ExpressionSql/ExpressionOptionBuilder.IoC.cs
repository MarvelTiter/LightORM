#if NET462_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET8_0_OR_GREATER
using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

internal partial class ExpressionOptionBuilder
{
    public WeakReference<IServiceCollection>? WeakServices { get; set; }
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
        else if (WeakServices?.TryGetTarget(out var services) == true)
        {
            services?.AddScoped(typeof(IAdoInterceptor), item);
        }

        return this;
    }

    public ExpressionSqlOptions Build(IServiceProvider provider)
    {
        if (provider is not null)
        {
            var interceptor = provider.GetServices<IAdoInterceptor>();
            return new(interceptor);
        }
        return new();
    }
}
#endif