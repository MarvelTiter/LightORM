using LightORM.ExpressionSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LightORM;
public static class IocExtension
{

    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options)
    {
        //var option = ExpressionSqlOptions.Instance.Value;
        var builder = new ExpressionOptionBuilder();
        builder.WeakServices = new WeakReference<IServiceCollection>(services);
        options(builder);
        //if (option.InitialContexts.Count > 0)
        //{
        //    option.Check();
        //}
        services.AddSingleton(provider =>
        {
            var option = builder.Build(provider);
            return option;
        });
       
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }

    //public static void InitializedContext<T>(this IExpressionContext context) where T : ExpressionContext, new()
    //{
    //    var ctx = new T();
    //    ctx.Initialized((context as IDbInitial)!);
    //}

}
