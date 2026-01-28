using LightORM.Interfaces;
using LightORM.Models;
using System.Diagnostics;
using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LightORMTest;

public class TestBase
{
    protected IExpressionContext Db { get; }
    internal ResolveContext ResolveCtx { get; set; }
    protected IServiceProvider Services { get; }
    public ITableContext TableContext { get; } = new TestTableContext();
    [NotNull] public virtual DbBaseType? DbType { get; }

    private readonly Dictionary<string, string> sqlResults = [];

    protected TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLightOrm(option =>
        {
            Configura(option);
            option.UseInterceptor<LightOrmAop>();
            //option.SetTableContext(TableContext);
        });

        Services = services.BuildServiceProvider();

        Db = Services.GetRequiredService<IExpressionContext>();

        ResolveCtx = ResolveContext.Create(DbType);
        ConfiguraSqlResults(sqlResults);
    }

    protected void AssertSqlResult(string methodName, string sql)
    {
        if (sqlResults.TryGetValue(methodName, out var sqlResult))
        {
            Assert.IsTrue(SqlNormalizer.AreSqlEqual(sqlResult, sql));
        }
    }

    protected virtual void Configura(IExpressionContextSetup option)
    {
    }

    protected virtual void ConfiguraSqlResults(Dictionary<string, string> results)
    {
    }
}

public class LightOrmAop : AdoInterceptorBase
{
    public override void AfterExecute(SqlExecuteContext context)
    {
        Debug.WriteLine($"{context.TraceId} {Environment.NewLine}{context.Sql} {Environment.NewLine}耗时:{context.Elapsed}");
    }

    public override void BeforeExecute(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }

    public override void OnException(SqlExecuteExceptionContext context)
    {
        Debug.WriteLine($"{context.TraceId}:{context.Exception.Message}");
        Debug.WriteLine(context.Sql);
        Debug.WriteLine("=====================================");
    }

    public override void OnPrepareCommand(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }
}