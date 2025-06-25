using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM
{
    public static class ExpSqlFactory
    {
        private static ExpressionSqlOptions? option;
        private static readonly Lazy<IExpressionContext> lazyContext;

        public static void Configuration(Action<IExpressionContextSetup> config)
        {
            var builder = new ExpressionOptionBuilder();
            config.Invoke(builder);
            option = builder.Build(null);
        }

        static ExpSqlFactory()
        {
            lazyContext = new Lazy<IExpressionContext>(() =>
            {
                if (option is null)
                {
                    throw new InvalidOperationException("未配置");
                }
                return new ExpressionCoreSql(option);
            });
        }

        public static IExpressionContext GetContext() => lazyContext.Value;
    }
}
