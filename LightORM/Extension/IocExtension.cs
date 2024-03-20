using LightORM.ExpressionSql;
#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endif

namespace MDbContext;

#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
public static class IocExtension
{
   
    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<ExpressionSqlOptions> options)
    {
        var option = new ExpressionSqlOptions();
        options(option);
        services.AddSingleton(option);
        services.AddTransient(provider =>
        {
            var o = provider.GetService<ExpressionSqlOptions>()!;
            var builder = new ExpressionSqlBuilder(o);
            var ins = builder.Build(provider);
            return ins;
        });
        return services;
    }

    //public static void InitializedContext<T>(this IExpressionContext context) where T : ExpressionContext, new()
    //{
    //    var ctx = new T();
    //    ctx.Initialized((context as IDbInitial)!);
    //}

}
#endif
