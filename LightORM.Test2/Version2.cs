using LightORM.Test2.Models;
using MDbContext;
using MDbContext.NewExpSql;
using System.Diagnostics;

namespace LightORM.Test2
{
    [TestClass]
    public class Version2
    {
        P p = new P { Age = 5, Bns = 10 };
        [TestMethod]
        public void V2Insert()
        {
            Watch(sql =>
            {
                var u = new Users();
                u.UserName = "Hello";
                u.IsUse = true;
                u.Age = 18;
                sql.Insert(u);
            });
        }

        [TestMethod]
        public void V2Select()
        {
            Watch(sql =>
            {
                Watch(sql =>
                {
                    sql.Select<Users, Job>().Where<Job>(j => j.BNS_ID > p.Bns && j.USR_ID == "h").Where(u => u.Age > 5);
                });
            });
        }

        private void Watch(Action<ExpressionSql> action)
        {
            ExpressionSql eSql = new ExpressionSql(DbBaseType.SqlServer);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action(eSql);
            stopwatch.Stop();
            Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine(eSql);
            Console.WriteLine("====================================");
        }

        class P
        {
            public int Age { get; set; }
            public double Bns { get; set; }
        }
    }
}