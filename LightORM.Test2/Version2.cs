using LightORM.Test2.DemoModel;
using LightORM.Test2.Models;
using MDbContext;
using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.Repository;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace LightORM.Test2
{

    [TestClass]
    public class Version2 : TestBase
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
        class InheritTest : M
        {

        }

        [TestMethod]
        public void V2SelectObject()
        {
            Watch(async db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                var usrId = "admin";
                var powers = await db.Select<User>(u => new { u.UserName, u.Enable }).ToListAsync<string>();
                //Console.WriteLine(powers.Count);
            });
        }
        [TestMethod]
        public void V2SelectMember()
        {
            Watch(async db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                var usrId = "admin";
                var powers = await db.Select<User>(u => u.UserName).ToListAsync<string>();
                //Console.WriteLine(powers.Count);
            });
        }

        [TestMethod]
        public void V2Select()
        {
            Watch(async db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                await db.Delete<User>().AppendData(u).ExecuteAsync();
                await db.Insert<User>().AppendData(u).ExecuteAsync();
                var usrId = "admin";
                var powers = await db.Select<Power, RolePower, UserRole>()
                                     .Distinct()
                                     .InnerJoin<RolePower>(w => w.Tb1.PowerId == w.Tb2.PowerId)
                                     .InnerJoin<UserRole>(w => w.Tb2.RoleId == w.Tb3.RoleId)
                                     .Where(w => w.Tb3.UserId == usrId && w.Tb2.RoleId == "123")
                                     .OrderBy(w => new { w.Tb1.Sort, w.Tb1.ParentId })
                                     .ToListAsync();
                Console.WriteLine(powers.Count);
            });
        }

        [TestMethod]
        public void V2SelectInherit()
        {
            Watch(db =>
            {
                db.Repository<InheritTest>().GetListAsync(i => i.Tel == "123");
            });
        }
        [TestMethod]
        public void V2SelectSub()
        {
            Watch(db =>
            {
                var p = new Power();
                p.PowerId = "StationRecord";
                p.PowerName = " µ ±º‡øÿ";
                p.ParentId = "BussinessFunc";
                p.PowerType = PowerType.Page;
                p.Path = "stationrecord";
                p.Sort = 203;
                var sql = db.SwitchDatabase("Mysql").Insert<Power>().AppendData(p).ToSql();
                Console.WriteLine(sql);
            });
        }

        [TestMethod]
        public void V2SelectFunc()
        {
            Watch(db =>
            {
                var s = "sss";
                db.Select<Users>(w => new
                {
                    UM = SqlFn.Count(() => w.Age > 10),
                    UM2 = SqlFn.Count(() => w.Duty == s),
                }).ToList();
            });
        }

        [TestMethod]
        public void V2Update()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User001";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                db.Update<User>().AppendData(u).IgnoreColumns(u => new { u.Password }).ToSql();
                db.Update<User>().UpdateColumns(() => new { u.UserName, u.Password }).Where(u => u.UserId == "000").Execute();
                db.Update<User>().Set(p => p.Password, u.Password).Set(p => p.UserName, "123").Where(u => u.UserId == "000").ToSql();
            });
        }

        [TestMethod]
        public void V2SetUpdate()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User001";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                var sql = db.Update<User>().Set(p => p.Password, u.Password).Set(p => p.UserName, "123").Where(u => u.UserId == "000").ToSql();
                Console.WriteLine(sql);
            });
        }

        [TestMethod]
        public void V2Insert()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User001";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                db.Insert<User>().AppendData(u).ToSql();
            });
        }
        [TestMethod]
        public void V2Delete()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User001";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                db.Delete<User>().AppendData(u).ToSql();
            });
        }

        [TestMethod]
        public void V2Max()
        {
            Watch(db =>
            {
                var result = db.Select<User>().Max(u => u.UserId);
            });
        }

        [TestMethod]
        public void V2Sum()
        {
            Watch(db =>
            {
                db.Select<User>().Sum(u => u.UserId);
            });
        }

        [TestMethod]
        public void WhereLike()
        {
            Watch(db =>
            {
                db.Select<User>().Where(u => u.UserName.Like("HH"));
            });
        }
        [TestMethod]
        public void V2WhereIn()
        {
            Watch(db =>
            {
                //var arr = new string[] { "admin","123" };
                var arr = new List<string>() { "admin","123" };
                var result = db.Select<User>().Where(u => u.UserId.In(arr)).ToList();
                //var result = db.Select<User>().Where(u => u.UserId.In("admin")).ToList();
                foreach (var item in result)
                {
                    Console.WriteLine(item);
                }
            });
        }
        [TestMethod]
        public void DynamicTest()
        {
            Watch(db =>
            {
                var result = db.Select<Power>(u => new { u.PowerName, u.PowerId }).ToList();
                foreach (var item in result)
                {
                    Console.WriteLine($"{item.PowerId} - {item.PowerName}");
                }
            });
        }

        [TestMethod]
        public void GroupByTest()
        {
            Watch(async db =>
            {
                var result = await db.Select<Power>(p => new
                {
                    G = SqlFn.Count(() => p.PowerId.Like("2"))
                }).GroupBy(p => p.PowerId).ToListAsync<int>();
                Console.WriteLine(result.Count());
            });
        }

        [TestMethod]
        public void TransactionTest()
        {
            Watch(db =>
            {
                //var r = db.SwitchDatabase("Mysql").Repository<User>().GetListAsync(u => true);
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                var trans = db.BeginTransaction();
                trans.Update<User>().UpdateColumns(() => new { u.UserName }).Where(u => u.UserId == "admin").AttachTransaction();
                trans.Insert<User>().AppendData(u).AttachTransaction();
                trans.CommitTransaction();
            });
        }

        [TestMethod]
        public void InWhereTest()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                string[] sss = new[] { "123", "321" };
                db.Update<User>().UpdateColumns(() => new { u.Password }).Where(u => u.UserId.In(sss)).ToSql();
            });
        }

        [TestMethod]
        public void AdoTest()
        {
            Watch(db =>
            {
                var users = db.Ado.Query<User>("select * from user").ToList();
                foreach (var u in users)
                {
                    Console.WriteLine($"{u.UserId} - {u.UserName}");
                }
            });
        }
        [TestMethod]
        public void IgnoreTest()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                db.Select<User>().ToList();
                db.Insert<User>().AppendData(u).ToSql();
                db.Update<User>().AppendData(u).ToSql();
            });
        }

        [TestMethod]
        public void WhereNullCondition()
        {
            Watch(db =>
            {
                var u = new User();
                u.UserId = "User002";
                u.UserName = "≤‚ ‘001";
                u.Password = "0000";
                db.Update<User>().AppendData(u).Where(p => p.UserId != null).ToSql();
            });
        }
        [TestMethod]
        public void DataReaderTest()
        {
            Watch(db =>
            {
                db.Ado.Query("select * from user", null, reader =>
                {

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]} - ");
                        }
                        Console.WriteLine();
                    }
                });
            });
        }

        [TestMethod]
        public void DataReaderAsyncTest()
        {
            Watch(db =>
            {
                db.Ado.QueryAsync("select * from user", null, reader =>
                {

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]} - ");
                        }
                        Console.WriteLine();
                    }
                    return Task.CompletedTask;
                });
            });
        }

        [TestMethod]
        public void CountTest()
        {
            Watch(async db =>
            {
                await Task.CompletedTask;
                var count = db.Select<User>().Count();
                Console.WriteLine(count);
            });
        }

        class P
        {
            public int Age { get; set; }
            public double Bns { get; set; }
        }
    }
}