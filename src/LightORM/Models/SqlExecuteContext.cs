namespace LightORM.Models;
public enum ExecuteMethod
{
    NonQuery,
    Scalar,
    Reader,
    DataSet,
    DataTable,
    BeginTransaction,
    CommitTransaction,
    RollbackTransaction,
}
public class SqlExecuteContext(ExecuteMethod method, string? sql, object? parameter)
{
    public string TraceId { get; } = Guid.NewGuid().ToString("N");
    public ExecuteMethod ExecuteType { get; } = method;
    public string? Sql { get; } = sql;
    public object? Parameter { get; } = parameter;
    public TimeSpan Elapsed { get; set; }
    //public object? Result { get; set; }
}

public class SqlExecuteExceptionContext(SqlExecuteContext ctx, Exception exception)
    : SqlExecuteContext(ctx.ExecuteType, ctx.Sql, ctx.Parameter)
{
    public bool IsHandled { get; set; }
    public Exception Exception { get; set; } = exception;
}
