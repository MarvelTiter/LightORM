using LightORM.Providers.Sqlite;
using System.Linq.Expressions;

namespace TestProject1.ExpressionTest
{
    [TestClass]
    public class ConditionalTest : TestBase
    {
        [TestMethod]
        public void Test()
        {
            Expression<Func<User, int?>> exp = u => u.Age > 10 ? 1 : null;
            var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance);
            HandleExpressionParameters(ctx, exp);
            var result = exp.Resolve(SqlResolveOptions.Select, ctx);
            Console.WriteLine(result.SqlString);
        }
    }
}
