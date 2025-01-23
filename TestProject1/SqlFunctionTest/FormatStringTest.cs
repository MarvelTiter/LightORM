using System.Linq.Expressions;
using LightORM.Providers.Sqlite;

namespace TestProject1.SqlFunctionTest;

[TestClass]
public class FormatStringTest: TestBase
{
    [TestMethod]
    public void InterpolationFormat()
    {
        var name = "test";
        var seq = 0;
        Expression<Func<User, bool>> exp = u => u.UserName == $"{name}{seq}";
        var table = TestTableContext.TestProject1_Models_User;
        var ctx = new ResolveContext(CustomSqlite.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }
    
    [TestMethod]
    public void InterpolationFormatOption()
    {
        Expression<Func<User, bool>> exp = u => u.UserName == $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var table = TestTableContext.TestProject1_Models_User;
        var ctx = new ResolveContext(CustomSqlite.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }
}