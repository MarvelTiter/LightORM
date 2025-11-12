using LightORM.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace LightORM;
public static class IocExtension
{

    public static IServiceCollection AddLightOrm(this IServiceCollection services, Action<IExpressionContextSetup> options)
    {
        var builder = new ExpressionOptionBuilder
        {
            WeakServices = new(services)
        };
        options(builder);
        services.AddScoped(provider =>
        {
            var option = builder.Build(provider);
            return option;
        });
        services.AddScoped(typeof(ILightOrmRepository<>), typeof(DefaultRepository<>));
        services.AddScoped<IExpressionContext, ExpressionCoreSql>();
        return services;
    }
}
