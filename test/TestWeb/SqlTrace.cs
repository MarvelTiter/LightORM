using LightORM.Implements;
using LightORM.Models;

namespace TestWeb
{
    public class Ctx
    {
        public int Id { get; set; }
    }
    public class SqlTrace(Ctx ctx, ILogger<SqlTrace> logger) : AdoInterceptorBase
    {
        public override void BeforeExecute(SqlExecuteContext context)
        {
            logger.LogInformation("ctx => {id} : {Sql}", ctx.Id, context.Sql);
        }
    }
}
