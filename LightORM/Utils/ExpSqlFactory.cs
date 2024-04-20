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
        static ExpressionSqlOptions option = new ExpressionSqlOptions();

        public static void Configuration(Action<ExpressionSqlOptions> config)
        {
            config.Invoke(option);
            if (option.InitialContexts.Count > 0)
            {
                option.Check();
            }
        }
        static IExpressionContext? context;
        static object locker = new object();
        public static IExpressionContext GetContext()
        {
            lock (locker)
            {
                if (context == null)
                {
                    lock (locker)
                    {
                        context = new ExpressionCoreSql(option);
                    }
                }
            }
            return context;
        }

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
