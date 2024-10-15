using LightORM.Providers.Sqlite.Extensions;
using System.Data.SQLite;
namespace TestProject1;

public class TestBase
{
    protected IExpressionContext Db { get; }
    protected ResolveContext ResolveCtx { get; }
    public TestBase()
    {
        var path = Path.GetFullPath("../../../test.db");

        ExpSqlFactory.Configuration(option =>
        {
            //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
            option.UseSqlite(option =>
            {
                option.MasterConnectionString = "DataSource=" + path;
                //option.MethodResolver.AddOrUpdateMethod()
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
        ResolveCtx = ResolveContext.Create(DbBaseType.Sqlite);
    }
}