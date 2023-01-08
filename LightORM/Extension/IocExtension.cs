using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDbContext
{    
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
}
