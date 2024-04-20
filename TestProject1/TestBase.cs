using System.Data.SQLite;
namespace TestProject1;

public class TestBase
{
    protected IExpressionContext Context { get; }
    public TestBase()
    {
        var path = Path.GetFullPath("../../../test.db");

        ExpSqlFactory.Configuration(option =>
        {
            option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
            option.SetWatcher(aop =>
            {
                aop.DbLog = (sql, p) =>
                {
                    Console.WriteLine(sql);
                };
            }).InitializedContext<TestInitContext>();
        });
        Context = ExpSqlFactory.GetContext();
    }
}