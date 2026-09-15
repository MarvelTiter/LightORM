using LightORM.Interfaces;
using LightORM.Models;
using System.Diagnostics;
using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

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
        if (context.Sql?.StartsWith("DROP") == true)
            return;
        if (context.Sql?.StartsWith("CREATE") == true)
            return;
        if (context.Sql?.StartsWith("SET") == true)
            return;
        if (context.Sql?.StartsWith("COMMENT") == true)
            return;
        if (context.Sql?.StartsWith("ALTER") == true)
            return;
        Debug.WriteLine($"""

            {context.TraceId}[{context.ConnectionId}]: 
            SQL: 
            {context.Sql}
            ===============
            参数:
            {DisplayParameter(context.Parameter)}

            耗时:{context.Elapsed}

            """);


    }

    private static string DisplayParameter(object? p)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Serialize(p);
        }
        catch (Exception ex)
        {
            // 参数对象可能含 Type 等不可序列化成员(如 Column.TableType), 此处必须兜底:
            // 否则序列化异常会覆盖原始的 SQL 异常, 让排错看不到真正的报错。
            return $"[参数序列化失败: {ex.Message}] {p?.GetType().Name}";
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
        Debug.WriteLine(DisplayParameter(context.Parameter));
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