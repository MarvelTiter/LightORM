using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest.Select
{
    [TestClass]
    public class SelectSql : TestBase
    {
        [TestMethod]
        public void SelectSingle()
        {
            var sql = Db.Select<Product>()
                .Where(p => p.ModifyTime > DateTime.Now)
                .ToSql(p => new { p.ProductId, p.ProductName });
            Console.WriteLine(sql);
            var result = $"""
                SELECT `a`.`ProductId`, `a`.`ProductName`
                FROM `Product` `a`
                WHERE (`a`.`ModifyTime` > @Now_0)
                """;
            Assert.IsTrue(result == sql);
        }

        [TestMethod]
        public void SelectMulti()
        {
            var sql = Db.Select<User>()
                .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
                .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
                .Where(u => u.UserId == "admin")
                .ToSql(w => w.Tb1);
            Console.WriteLine(sql);
            var result = """
                SELECT `a`.*
                FROM `USER` `a`
                INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
                INNER JOIN `ROLE` `c` ON (`b`.`ROLE_ID` = `c`.`ROLE_ID`)
                WHERE (`a`.`USER_ID` = 'admin')
                """;
            Assert.IsTrue(result == sql);
        }

        [TestMethod]
        public void SelectExtension()
        {
            var sql = Db.Select<Power, RolePower, Role>()
                .Distinct()
                .Where(w => w.Tb1.PowerId == w.Tb2.PowerId && w.Tb2.RoleId == w.Tb3.RoleId)
                .ToSql(w => new { w.Tb1 });
            Console.WriteLine(sql);
            var result = """
                SELECT DISTINCT `a`.*
                FROM `POWERS` `a`, `ROLE_POWER` `b`, `ROLE` `c`
                WHERE ((`a`.`POWER_ID` = `b`.`POWER_ID`) AND (`b`.`ROLE_ID` = `c`.`ROLE_ID`))
                """;
            Assert.IsTrue(result == sql);
        }

        class Jobs
        {
            public string? Plate { get; set; }
            public string? StnId { get; set; }
        }

        [TestMethod]
        public void ComplexSelect()
        {
            var info = Db.Select<Jobs>().AsTemp("info", j => new
            {
                Fzjg = j.Plate!.Substring(1, 2),
                j.StnId
            });
            var stnFzjg = Db.FromTemp(info)
                .GroupBy(a => new { a.StnId, a.Fzjg })
                .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
                .AsTemp("stn_fzjg", g => new
                {
                    g.Group.StnId,
                    g.Group.Fzjg,
                    Count = g.Count(),
                    Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
                });
            var allFzjg = Db.FromTemp(info).GroupBy(a => new { a.Fzjg })
                .OrderByDesc(a => a.Count())
                .AsTemp("all_fzjg", g => new
                {
                    StnId = "合计",
                    g.Group.Fzjg,
                    Count = g.Count(),
                    Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
                });
            var allStation = Db.FromTemp(info).GroupBy("ROLLUP(\"StnId\")")
                .AsTemp("all_station", g => new
                {
                    StnId = SqlFn.NullThen(g.StnId, "合计"),
                    Total = SqlFn.Count()
                });
            var result = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
                .GroupBy(t => new { t.StnId })
                .AsTable(g => new
                {
                    StnId = g.Group.StnId!,
                    FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value(),
                    FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value()
                }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
                {
                    StnId = "合计",
                    FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderBy(g.StnId).Value(),
                    FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderBy(g.StnId).Value()
                })).AsSubQuery()
                .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
                .ToSql((t, a) => new
                {
                    Jczmc = SqlFn.NullThen(t.StnId, "TT"),
                    a.Total,
                    t
                });
            Console.WriteLine(result);
        }
    }
}
