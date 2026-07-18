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
public class SqlExecuteContext
{

    public string TraceId { get; }
    public ExecuteMethod ExecuteType { get; }
    public string? Sql { get; }
    public CommandType CommandType { get; }
    public object? Parameter { get; }
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    public Type ParameterType { get; }
    public TimeSpan Elapsed { get; set; }

    public SqlExecuteContext(SqlExecuteContext other)
    {
        TraceId = other.TraceId;
        ExecuteType = other.ExecuteType;
        Sql = other.Sql;
        CommandType = other.CommandType;
        Parameter = other.Parameter;
        ParameterType = other.ParameterType;
    }

    public SqlExecuteContext(ExecuteMethod method, string? sql, object? parameter,
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type parameterType, CommandType commandType = CommandType.Text)
    {
        TraceId = Guid.NewGuid().ToString("N");
        ExecuteType = method;
        Sql = sql;
        CommandType = commandType;
        Parameter = parameter;
        ParameterType = parameterType;
    }
    //public object? Result { get; set; }
}

public class SqlExecuteExceptionContext(SqlExecuteContext ctx, Exception exception)
    : SqlExecuteContext(ctx)
{
    public bool IsHandled { get; set; }
    public Exception Exception { get; set; } = exception;
}
