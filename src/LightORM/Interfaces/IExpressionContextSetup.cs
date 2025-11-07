using LightORM.Implements;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Interfaces;

public interface IExpressionContextSetup
{
    IExpressionContextSetup SetDefault(string key);
    IExpressionContextSetup SetUseParameterized(bool use);
    IExpressionContextSetup SetConnectionPoolSize(int poolSize);
    IExpressionContextSetup SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider);
    IExpressionContextSetup SetTableContext(ITableContext context);
#if NET8_0_OR_GREATER
    IExpressionContextSetup UseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
#else
    IExpressionContextSetup UseInterceptor<T>() 
#endif
     where T : AdoInterceptorBase;
    IExpressionContextSetup UseInitial<T>() where T : DbInitialContext, new();
    IExpressionContextSetup TableConfiguration(Action<TableGenerateOption> action);
    // IExpressionContextSetup UseIdentifierQuote(bool value = true);

}
