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
        private static readonly ExpressionSqlOptions option = new();
        private static readonly Lazy<IExpressionContext> lazyContext;

        public static void Configuration(Action<ExpressionSqlOptions> config)
        {
            config.Invoke(option);
            if (option.InitialContexts.Count > 0)
            {
                option.Check();
            }
        }
        static ExpSqlFactory()
        {
            lazyContext = new Lazy<IExpressionContext>(() =>
            {
                return new ExpressionCoreSql(option);
            });
        }

        public static IExpressionContext GetContext() => lazyContext.Value;

        /// <summary>
        /// 需要自己释放对象
        /// </summary>
        /// <returns></returns>
        public static IExpressionContext GetTransientContext()
        {
            return new ExpressionCoreSql(option);
        }
    }
}
