using LightORM.Implements;
using LightORM.Models;

internal class SqlTrace : AdoInterceptorBase
{
    public override void BeforeExecute(SqlExecuteContext context)
    {
        Console.WriteLine(context.Sql);
        Console.WriteLine();
    }
}
