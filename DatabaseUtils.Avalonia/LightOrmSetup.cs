using LightORM;
using LightORM.Implements;
using LightORM.Models;

namespace DatabaseUtils
{
    /// <summary>
    /// LightORM 表上下文，由 LightOrmTableContextGenerator 源生成器生成实现。
    /// </summary>
    [LightORMTableContext]
    public partial class DbContext
    {
    }

    public class SqlLogger : AdoInterceptorBase
    {
        public override void OnException(SqlExecuteExceptionContext context)
        {
            base.OnException(context);
        }
    }
}
