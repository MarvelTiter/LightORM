using System.Data.SQLite;
namespace TestProject1;

public class TestBase
{
    protected IExpressionContext Context { get; }
    public TestBase()
    {
        var options = new ExpressionSqlOptions();
        var path = Path.GetFullPath("../../../test.db");
        options.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
        options.SetWatcher(aop =>
        {
            aop.DbLog = (sql, p) =>
            {
                Console.WriteLine(sql);
            };
        });
        var builder = new ExpressionSqlBuilder(options);
        Context = builder.Build();
    }
}