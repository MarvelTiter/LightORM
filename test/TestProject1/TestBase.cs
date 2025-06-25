using LightORM.Providers.Oracle.Extensions;
using LightORM.Providers.Sqlite.Extensions;
using System.Data.SQLite;
using LightORM.Implements;

namespace TestProject1;

public class TestBase
{
    public IExpressionContext Db { get; }
    internal ResolveContext ResolveCtx { get; }
    protected ITableContext TableContext { get; } = new TestTableContext();
    public TestBase()
    {
        var path = Path.GetFullPath("../../../../../test.db");

        ExpSqlFactory.Configuration(option =>
        {
            //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
            option.UseSqlite("DataSource=" + path);
            option.UseOracle(o =>
            {
                o.DbKey = "Oracle";
                o.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
            });
            option.SetTableContext(TableContext);
            option.UseInterceptor<AdoInterceptorBase>();
        });
        Db = ExpSqlFactory.GetContext();
        ResolveCtx = ResolveContext.Create(DbBaseType.Oracle);
    }
}
