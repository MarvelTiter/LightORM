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
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2074", Justification = "非AOT环境会传入object")]
#endif
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
        // 如果有参数传入，但是参数类型为object，说明是非AOT环境调用扩展方法，需要获取参数的真实类型
        ParameterType = parameterType == typeof(object) && parameter is not null ? parameter.GetType() : parameterType;
    }
    //public object? Result { get; set; }
}

public class SqlExecuteExceptionContext(SqlExecuteContext ctx, Exception exception)
    : SqlExecuteContext(ctx)
{
    public bool IsHandled { get; set; }
    public Exception Exception { get; set; } = exception;
}
