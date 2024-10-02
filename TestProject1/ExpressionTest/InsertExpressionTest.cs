using LightORM.Providers.Sqlite;
using System.Linq.Expressions;

namespace TestProject1.ExpressionTest;

[TestClass]
public class InsertExpressionTest : TestBase
{
    [TestMethod]
    public void Insert()
    {
        Expression<Func<User, object>> exp = u => u;
        var table = TestTableContext.TestProject1_Models_User;
        var ctx = new ResolveContext(CustomSqlite.Instance, table);

        var result = exp.Resolve(SqlResolveOptions.Insert, ctx);
        Console.WriteLine(result.SqlString);
    }
}
