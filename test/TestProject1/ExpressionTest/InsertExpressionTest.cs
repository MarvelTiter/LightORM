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
        var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance);
        HandleExpressionParameters(ctx, exp);
        var result = exp.Resolve(SqlResolveOptions.Insert, ctx);
        Console.WriteLine(result.SqlString);
    }
}
