using LightORM.Test2.Models;
using MDbContext;
using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace LightORM.Test2
{
    [TestClass]
    public class Version2
    {
        P p = new P { Age = 5, Bns = 10 };

        public class ReturnType
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }
        class M
        {
            public string? Tel { get; set; }
            public string? Bz { get; set; }
        }
        [TestMethod]
        public void V2Select()
        {
            Watch(db =>
            {
                var sql = db.Select<Users, BasicStation, Job>()
                    .InnerJoin<BasicStation>((u, b) => u.Sex == b.Sflw)
                    .Where<Users>(u => u.Password == "321" && u.Tel == "123")
                    .Where<BasicStation>(b => b.Znsh > 5)
                    //.Count(out var total)
                    .Paging(1, 5)
                    .ToList<M>(w => new { w.Tb1.Tel, w.Tb2.Bz }).ToList();
                Console.WriteLine(sql);
            });
        }

        [TestMethod]
        public void V2SelectFunc()
        {
            Watch(db =>
            {
                var s = "sss";
                db.Select<Users>().ToList(w => new
                {
                    UM = SqlFn.Count(() => w.Age > 10),
                    UM2 = SqlFn.Count(() => w.Duty == s),
                });
            });
        }

        [TestMethod]
        public void V2Update()
        {
            Watch(db =>
            {
                var u = new Users();
                db.Update<Users>().AppendData(u).IgnoreColumns(u => new { u.Tel }).Where(u => u.Age == 10).ExecuteAsync();
            });
        }

        [TestMethod]
        public void V2Insert()
        {
            Watch(db =>
            {
                var u = new Users();
                db.Insert<Users>().AppendData(u).ExecuteAsync();
            });
        }
        [TestMethod]
        public void V2Delete()
        {
            Watch(db =>
            {
                var u = new Users();
                db.Delete<Users>().Where(u => u.Age > 10).ExecuteAsync();
            });
        }

        [TestMethod]
        public void V2Max()
        {
            Watch(db =>
            {
                db.Select<Users>().Max(u => u.Age);
            });
        }

        [TestMethod]
        public void V2Sum()
        {
            Watch(db =>
            {
                db.Select<Users>().Sum(u => u.Age);
            });
        }

        [TestMethod]
        public void WhereLike()
        {
            Watch(db =>
            {
                db.Select<Users>().Where(u => u.Duty.Like("HH"));
            });
        }
        [TestMethod]
        public void DynamicTest()
        {
            Watch(db =>
            {
                var result = db.Select<Power>().ToDynamicList(u => new { u.PowerName, u.PowerId });
                foreach (var item in result)
                {
                    Console.WriteLine($"{item.PowerId} - {item.PowerName}");
                }
            });
        }

        [TestMethod]
        public void GroupByTest()
        {
            Watch(db =>
            {
                var result = db.Select<Power>().GroupBy(p => p.PowerId).ToList<int>(p => new
                {
                    G = SqlFn.Count(() => p.PowerId.Like("2"))
                });
                Console.WriteLine(result.Count());
            });
        }

        private void Watch(Action<IExpSql> action)
        {
            IExpSql eSql = new ExpressionSqlBuilder()
                .SetDatabase(DbBaseType.Sqlite, SqliteDbContext)
                .Build();
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
        class P
        {
            public int Age { get; set; }
            public double Bns { get; set; }
        }
    }
}