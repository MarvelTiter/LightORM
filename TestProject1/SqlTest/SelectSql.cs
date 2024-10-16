using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
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
        }

        [TestMethod]
        public void SelectExtension()
        {
            var sql = Db.Select<Power, RolePower, Role>()
                .InnerJoin(w => w.Tb1.PowerName == w.Tb3.RoleName)
                .Distinct()
                .Where(w => w.Tb1.PowerId == w.Tb2.PowerId && w.Tb2.RoleId == w.Tb3.RoleId)
                .ToSql(w => new { w.Tb1 });
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectInclude()
        {
            var sql = Db.Select<User>()
                .Where(u => u.UserRoles.WhereIf(r => r.RoleId.StartsWith("ad")))
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectIncludeWhere()
        {
            var select = Db.Select<User>()
                .Where(u => u.UserRoles.WhereIf(r => r.RoleId.Contains("admin")))
                .ToSql();

            Console.WriteLine(select);
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
            var stnFzjg = Db.FromTemp(info).GroupBy(a => new { a.StnId, a.Fzjg })
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
                    StnId = SqlFn.Nvl(g.StnId, "合计"),
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
                    Jczmc = SqlFn.Nvl(t.StnId,"TT"),
                    a.Total,
                    t
                });
            Console.WriteLine(result);
        }
    }
}
