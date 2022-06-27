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
        public static IServiceCollection UseLightOrm(this IServiceCollection services, Action<ExpressionSqlBuilder> config)
        {
            var builder = new ExpressionSqlBuilder();
            config(builder);
            var ins = builder.Build();
            services.AddSingleton(ins);
            return services;
        }
    }
}
