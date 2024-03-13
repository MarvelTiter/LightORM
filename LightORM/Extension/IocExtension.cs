using LightORM.ExpressionSql;
#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endif

namespace MDbContext;

#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
public static class IocExtension
{
    [Obsolete]
    public static IServiceCollection UseLightOrm(this IServiceCollection services, Action<ExpressionSqlBuilder> config)
    {
        var builder = new ExpressionSqlBuilder();
        config(builder);
        var ins = builder.BuildContext();
        services.AddSingleton(ins);
        return services;
    }

    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<ExpressionSqlOptions> options)
    {
        var option = new ExpressionSqlOptions();
        options(option);
        //services.AddSingleton(typeof(IExpressionContext), provider => builder.Build());
        services.AddSingleton(provider =>
        {
            var builder = new ExpressionSqlBuilder(option);
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
