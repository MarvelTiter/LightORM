using System.Diagnostics.CodeAnalysis;

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
public class SqlExecuteContext(ExecuteMethod method, string? sql, object? parameter,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    Type parameterType, CommandType commandType = CommandType.Text)
{
    public string TraceId { get; } = Guid.NewGuid().ToString("N");
    public ExecuteMethod ExecuteType { get; } = method;
    public string? Sql { get; } = sql;
    public CommandType CommandType { get; } = commandType;
    public object? Parameter { get; } = parameter;
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    public Type ParameterType { get; set; } = parameterType;
    public TimeSpan Elapsed { get; set; }
    //public object? Result { get; set; }
}

public class SqlExecuteExceptionContext(SqlExecuteContext ctx, Exception exception)
    : SqlExecuteContext(ctx.ExecuteType, ctx.Sql, ctx.Parameter, ctx.ParameterType, ctx.CommandType)
{
    public bool IsHandled { get; set; }
    public Exception Exception { get; set; } = exception;
}
