#if NET6_0_OR_GREATER
using LightORM;
using LightORM.ExpressionSql;
using Microsoft.Extensions.DependencyInjection;

namespace MDbContext;

public static class IocExtension
{

    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<ExpressionSqlOptions> options)
    {
        var option = new ExpressionSqlOptions();
        options(option);
        services.AddSingleton(option);
        //var builder = new ExpressionSqlBuilder(option);
        //services.AddSingleton(builder);
        //services.AddScoped(provider =>
        //{
        //    var builder = provider.GetService<ExpressionSqlBuilder>()!;
        //    var ins = builder.Build();
        //    return ins;
        //});
        services.AddTransient<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    //public static void InitializedContext<T>(this IExpressionContext context) where T : ExpressionContext, new()
    //{
    //    var ctx = new T();
    //    ctx.Initialized((context as IDbInitial)!);
    //}

}
#endif
