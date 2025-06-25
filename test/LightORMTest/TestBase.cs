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
namespace LightORMTest;

public class TestBase
{
    public IExpressionContext Db { get; }
    internal ResolveContext ResolveCtx { get; }
    protected ITableContext TableContext { get; } = new TestTableContext();
    public TestBase()
    {
        var path = Path.GetFullPath("../../../../../test.db");
        IServiceCollection services = new ServiceCollection();
        services.AddLightOrm(option =>
        {
            option.UseSqlite("DataSource=" + path);
            option.UseOracle(option =>
            {
                option.DbKey = "Oracle";
                option.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
            });
            option.UseMySql(o =>
            {
                o.DbKey = "v";
                o.MasterConnectionString = "Data Source=localhost;Database=videocollection;User ID=root;Password=123456;charset=gbk";
            });
            option.UseInterceptor<LightOrmAop>();
        });
        
        var provider = services.BuildServiceProvider();
        
        Db = provider.GetRequiredService<IExpressionContext>();

        //ExpSqlFactory.Configuration(option =>
        //{
        //    //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
        //    option.UseSqlite("DataSource=" + path);
        //    option.UseOracle(option =>
        //    {
        //        option.DbKey = "Oracle";
        //        option.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
        //    });
        //    option.UseMySql(o =>
        //    {
        //        o.DbKey = "v";
        //        o.MasterConnectionString = "Data Source=localhost;Database=videocollection;User ID=root;Password=123456;charset=gbk";
        //    });
        //    option.SetTableContext<TestTableContext>();
        //    option.UseInterceptor<LightOrmAop>();
        //    //option.SetTableContext(TableContext);
        //    //option.SetWatcher(aop =>
        //    //{
        //    //    aop.DbLog = (sql, p) =>
        //    //    {
        //    //        Console.WriteLine(sql);
        //    //        Console.WriteLine();
        //    //    };
        //    //});
        //    //.InitializedContext<TestInitContext>();
        //});
        //Db = ExpSqlFactory.GetContext();

        ResolveCtx = ResolveContext.Create(DbBaseType.Oracle);
    }
}

public class LightOrmAop : AdoInterceptorBase
{
    public override void AfterExecute(SqlExecuteContext context)
    {
        Debug.WriteLine($"{context.TraceId} 语句:{Environment.NewLine}{context.Sql}");
        Debug.WriteLine($"{context.TraceId} 耗时:{context.Elapsed}");
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
