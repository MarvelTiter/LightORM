namespace LightORM.Interfaces;

public interface IAdoInterceptor
{
    void OnPrepareCommand(SqlExecuteContext context);
    void BeforeExecute(SqlExecuteContext context);
    void AfterExecute(SqlExecuteContext context);
    void OnException(SqlExecuteExceptionContext context);
}
