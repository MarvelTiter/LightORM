using LightORM.Test2.Models;
using MDbContext;
using MDbContext.NewExpSql;
using MDbContext.NewExpSql.Interface;
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

        [TestMethod]
        public void V2Select()
        {
            Watch(db =>
            {
                var sql = db.Select<Users>()
                    .InnerJoin<BasicStation>((u, b) => b.Cjsqbh == "12")
                    .Count(out var total)
                    .Where(u => u.Password == "321" && u.Tel == "123")
                    .Where<BasicStation>(b => b.Znsh > 5).ToSql();
                Console.WriteLine(sql);
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