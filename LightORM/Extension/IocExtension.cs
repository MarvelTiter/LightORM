using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Generic;
using System.Text;

namespace MDbContext
{
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
            var builder = new ExpressionSqlBuilder(option);
            var ins = builder.Build();
            //services.AddSingleton(typeof(IExpressionContext), provider => builder.Build());
            services.AddSingleton(ins);
            return services;
        }
    }
#endif
}
