using LightORM.Test2.DemoModel;
using MDbContext.ExpressionSql;
using MDbContext.Repository;

namespace LightORM.Test2
{
    [TestClass]
    public class ReporisotyTest : TestBase
    {
        [TestMethod]
        public void RepositorySelect()
        {
            Watch(async db =>
            {
                var username = "admin";
                var u = await db.Repository<User>().GetSingleAsync(u => u.UserId.RightLike(username));
                Console.WriteLine(u.Password);
            });
        }
    }
}