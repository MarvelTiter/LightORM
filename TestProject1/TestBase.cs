using LightORM.Providers.Oracle.Extensions;
using LightORM.Providers.Sqlite.Extensions;
using System.Data.SQLite;
namespace TestProject1;

public class TestBase
{
    protected IExpressionContext Db { get; }
    protected ResolveContext ResolveCtx { get; }
    public TestBase()
    {
        var path = Path.GetFullPath("../../../../test.db");

        ExpSqlFactory.Configuration(option =>
        {
            //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
            option.UseSqlite("DataSource=" + path);
            option.UseOracle(option =>
            {
                option.DbKey = "Oracle";
                option.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
            });
            option.SetTableContext(new TestTableContext());
            option.SetWatcher(aop =>
            {
                aop.DbLog = (sql, p) =>
                {
                    Console.WriteLine(sql);
                };
            });//.InitializedContext<TestInitContext>();
        });
        Db = ExpSqlFactory.GetContext();
        ResolveCtx = ResolveContext.Create(DbBaseType.Oracle);
    }
}