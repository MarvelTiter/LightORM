using LightORM.Implements;

namespace LightORM.Interfaces;

public interface IExpressionContextSetup
{
    IExpressionContextSetup SetConnectionPoolSize(int poolSize);
    IExpressionContextSetup SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider);
    IExpressionContextSetup SetTableContext(ITableContext context);
    IExpressionContextSetup UseInterceptor<T>() where T : AdoInterceptorBase;
    IExpressionContextSetup UseInitial<T>() where T : DbInitialContext, new();
    IExpressionContextSetup TableConfiguration(Action<TableGenerateOption> action);

}
