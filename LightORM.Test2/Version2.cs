using LightORM.Test2.Models;
using MDbContext;
using MDbContext.NewExpSql;
using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
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

        }

        public class ReturnType
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [TestMethod]
        public void V2Select()
        {
            Watch(db =>
            {
                var sql = db.Select<Users, BasicStation>()
                    .Count(out var total)
                    .Where<Users>(u => u.Password == "321" && u.Tel == "123")
                    .Where<BasicStation>(b => b.Znsh > 5).ToList<ReturnType>();
                //Console.WriteLine(sql);
            });
        }

        private void Watch(Action<IExpSql> action)
        {
            IExpSql eSql = new ExpSqlBuilder()
                .SetDatabase(DbBaseType.Sqlite, () => null)
                .Build();
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