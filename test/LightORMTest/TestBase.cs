using LightORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Providers.Sqlite.Extensions;
using LightORM.Providers.Oracle.Extensions;
using LightORM.Providers.MySql.Extensions;
using LightORM.Interfaces;
using LightORM.Models;
using System.Diagnostics;
using LightORM.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
namespace LightORMTest;

public class TestBase
{
    public IExpressionContext Db { get;  }
    internal ResolveContext ResolveCtx { get; set; }
    public ITableContext TableContext { get; } = new TestTableContext();
    [NotNull]
    public virtual DbBaseType? DbType { get; }
    public TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLightOrm(option =>
        {
            Configura(option);
            option.UseInterceptor<LightOrmAop>();
        });

        var provider = services.BuildServiceProvider();

        Db = provider.GetRequiredService<IExpressionContext>();

        ResolveCtx = ResolveContext.Create(DbType);
    }

    public virtual void Configura(IExpressionContextSetup option)
    {

    }
}

public class LightOrmAop : AdoInterceptorBase
{
    public override void AfterExecute(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId} 语句:{Environment.NewLine}{context.Sql}");
        //Debug.WriteLine($"{context.TraceId} 耗时:{context.Elapsed}");
    }

    public override void BeforeExecute(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }

    public override void OnException(SqlExecuteExceptionContext context)
    {
        Debug.WriteLine($"{context.TraceId}:{context.Exception.Message}");
        context.IsHandled = true;
    }

    public override void OnPrepareCommand(SqlExecuteContext context)
    {
        //Debug.WriteLine($"{context.TraceId}:{context.Elapsed}");
    }
}
