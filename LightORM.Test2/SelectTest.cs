using LightORM.Test2.DemoModel;
using MDbContext.ExpressionSql;

namespace LightORM.Test2
{
    [TestClass]
    public class SelectTest : TestBase
    {
        [TestMethod]
        public void SelectModel()
        {
            Watch(context =>
            {
                var sql = context.Select<Power, RolePower, User>()
                    .ToList();
                Console.WriteLine(sql);
            });
        }
    }
}