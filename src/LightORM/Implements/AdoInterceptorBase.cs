
using System.Diagnostics;

namespace LightORM.Implements;

public class AdoInterceptorBase : IAdoInterceptor
{
    public virtual void AfterExecute(SqlExecuteContext context)
    {
        Debug.WriteLine($"{context.TraceId}: AfterExecute");
    }

    public virtual void BeforeExecute(SqlExecuteContext context)
    {
        Debug.WriteLine($"{context.TraceId}: BeforeExecute");

    }

    public virtual void OnException(SqlExecuteExceptionContext context)
    {
        Debug.WriteLine($"{context.TraceId}: OnException");

    }

    public virtual void OnPrepareCommand(SqlExecuteContext context)
    {
        Debug.WriteLine($"{context.TraceId}: OnPrepareCommand");
    }
}
