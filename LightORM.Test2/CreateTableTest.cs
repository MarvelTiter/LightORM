using LightORM.Test2.DemoModel;
using MDbContext;
using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
using Microsoft.Data.Sqlite;

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
        public void SqliteTest()
        {
            IDbInitial? context = CreateDbInitial(DbBaseType.Sqlite);
            var sql = context!.GenerateCreateSql<User>();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SqlServerTest()
        {
            IDbInitial? context = CreateDbInitial(DbBaseType.SqlServer);
            context!.Configuration(option =>
            {
                //option.SupportComment = true;
            });
            var sql = context!.GenerateCreateSql<MachineStatus>();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void MySqlTest()
        {
            IDbInitial? context = CreateDbInitial(DbBaseType.MySql);
            context!.Configuration(option =>
            {
                //option.SupportComment = true;
            });
            var sql = context!.GenerateCreateSql<User>();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void OracleTest()
        {
            IDbInitial? context = CreateDbInitial(DbBaseType.Oracle);
            context!.Configuration(option =>
            {
                option.SupportComment = true;
            });
            var sql = context!.GenerateCreateSql<User>();
            Console.WriteLine(sql);
        }

        private static IDbInitial? CreateDbInitial(DbBaseType sqlite)
        {
            var option = new ExpressionSqlOptions().SetDatabase(sqlite, () => new SqliteConnection())
                            .SetWatcher(option =>
                            {
                                option.BeforeExecute = e =>
                                {
                                    Console.Write(DateTime.Now);
                                    Console.WriteLine(" Sql => \n" + e.Sql + "\n");
                                };
                            });

            var context = new ExpressionSqlBuilder(option).Build(null) as IDbInitial;
            return context;
        }
    }
}