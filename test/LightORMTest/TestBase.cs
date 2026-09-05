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
    protected IDatabaseProvider CurrentDefaultProvider => Db.Options.DatabaseProviders.First().Value;
    protected TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLightOrm(option =>
        {
            Configura(option);
            option.UseInterceptor<LightOrmAop>();
            option.SetTableContext(TableContext);
            option.ConfigJsonHandler<JsonHandler>();

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
        if (context.Sql?.Contains("初始化数据") == true)
            return;

        Debug.WriteLine($"""

            {context.TraceId}[{context.ConnectionId}]: 
            SQL: 
            {context.Sql}
            ===============
            参数:
            {string.Join($"  {Environment.NewLine}", DisplayParameter(context.Parameter))}

            耗时:{context.Elapsed}

            """);


    }

    private static IEnumerable<string> DisplayParameter(object? p)
    {
        if (p is Dictionary<string, object> dic)
        {
            foreach (var item in dic)
            {
                yield return $"{item.Key} - {item.Value}";
            }
        }
    }

    public override void BeforeExecute(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }

    public override void OnException(SqlExecuteExceptionContext context)
    {
        Debug.WriteLine($"{context.TraceId}[{context.ConnectionId}]:{context.Exception.Message}");
        Debug.WriteLine(context.Sql);
        Debug.WriteLine("=====================================");
        Debug.WriteLine("参数:");
        foreach (var item in DisplayParameter(context.Parameter))
        {
            Debug.WriteLine(item);
        }
    }

    public override void OnPrepareCommand(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }
}

public class JsonHandler : ILightJsonHelper
{
    public object? Deserialize(string json, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(json, type);
    }

    public object? Deserialize(byte[] json, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(json, type);
    }

    public string Serialize<T>(T value)
    {
        return System.Text.Json.JsonSerializer.Serialize(value);
    }
}