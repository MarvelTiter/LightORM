using MDbContext;
using MDbContext.ExpressionSql;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace LightORM.Test2
{
    [TestClass]
    public class TestBase
    {
        protected void Watch(Action<IExpressionContext> action)
        {
            IExpressionContext eSql = new ExpressionSqlBuilder()
                .SetDatabase(DbBaseType.Sqlite, SqliteDbContext)
                .SetSalveDatabase("Mysql", DbBaseType.MySql, () => new SqliteConnection())
                .SetWatcher(option =>
                {
                    option.BeforeExecute = e =>
                    {
                        Console.Write(DateTime.Now);
                        Console.WriteLine(" Sql => \n" + e.Sql + "\n");
                    };
                })
                .BuildContext();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action(eSql);
            stopwatch.Stop();
            Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine(eSql);
            Console.WriteLine("====================================");
        }

        private IDbConnection SqliteDbContext()
        {
            DbContext.Init(DbBaseType.Sqlite);
            var path = Path.GetFullPath("../../../Demo.db");
            var conn = new SqliteConnection($"DataSource={path}");
            return conn;
        }
    }
}