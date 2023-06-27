using LightORM.Test2.DemoModel;
using MDbContext;
using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
using SQLitePCL;

namespace LightORM.Test2
{
    [TestClass]
    public class CreateTableTest
    {
        class TestContext : ExpressionContext
        {
            public override void Initialized(IDbInitial db)
            {
                db.CreateTable<User>();
            }
        }
        [TestMethod]
        public void SqlServerTest()
        {
            var option = new ExpressionSqlOptions().SetDatabase(DbBaseType.SqlServer, () => null)
                .SetWatcher(option =>
                {
                    option.BeforeExecute = e =>
                    {
                        Console.Write(DateTime.Now);
                        Console.WriteLine(" Sql => \n" + e.Sql + "\n");
                    };
                }).InitializedContext<TestContext>();

            IExpressionContext eSql = new ExpressionSqlBuilder(option).Build(null);
        }

        [TestMethod]
        public void T()
        {
            byte[] bytes = new byte[1024];
            var name = bytes.GetType().FullName;
            Console.WriteLine(name);
        }
    }
}